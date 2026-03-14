param(
    [string]$ArticleBaseUrl = "http://localhost:8081",
    [string]$CommentBaseUrl = "http://localhost:8087",
    [string]$PrometheusUrl = "http://localhost:9090",
    [string]$SeqUrl = "http://localhost:5380",
    [string]$GrafanaUrl = "http://localhost:3000",
    [switch]$SkipStartup
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

function Write-Step {
    param([string]$Message)

    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function Test-HttpEndpoint {
    param([string]$Url)

    try {
        Invoke-WebRequest -Uri $Url -TimeoutSec 5 | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

function Wait-ForHttpEndpoint {
    param(
        [string]$Url,
        [string]$Name,
        [int]$TimeoutSeconds = 180
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        if (Test-HttpEndpoint -Url $Url) {
            Write-Host "$Name is ready at $Url" -ForegroundColor Green
            return
        }

        Start-Sleep -Seconds 3
    }

    throw "$Name did not become ready within $TimeoutSeconds seconds: $Url"
}

function Ensure-StackRunning {
    $requiredEndpoints = @(
        @{ Name = "ArticleService"; Url = "$ArticleBaseUrl/metrics" },
        @{ Name = "CommentService"; Url = "$CommentBaseUrl/metrics" },
        @{ Name = "Prometheus"; Url = "$PrometheusUrl/-/ready" },
        @{ Name = "Seq"; Url = $SeqUrl },
        @{ Name = "Grafana"; Url = "$GrafanaUrl/login" }
    )

    $needsStartup = $requiredEndpoints | Where-Object { -not (Test-HttpEndpoint -Url $_.Url) }

    if ($SkipStartup -and $needsStartup.Count -gt 0) {
        $missing = ($needsStartup | ForEach-Object { $_.Name }) -join ", "
        throw "Required services are not reachable and -SkipStartup was used. Missing: $missing"
    }

    if ($needsStartup.Count -gt 0) {
        if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
            throw "Docker is required to start the stack automatically."
        }

        Write-Step "Starting Docker Compose stack"
        Push-Location $repoRoot
        try {
            docker compose up -d | Out-Host
        }
        finally {
            Pop-Location
        }
    }

    foreach ($endpoint in $requiredEndpoints) {
        Wait-ForHttpEndpoint -Url $endpoint.Url -Name $endpoint.Name
    }
}

function Invoke-SafeGet {
    param([string]$Url)

    try {
        Invoke-RestMethod -Method Get -Uri $Url | Out-Null
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -ne 404) {
            throw
        }
    }
}

function Get-MetricsLines {
    param(
        [string]$MetricsUrl,
        [string[]]$Patterns
    )

    $metrics = (Invoke-WebRequest -Uri $MetricsUrl).Content -split "`n"
    return $metrics | Select-String -Pattern ($Patterns -join "|") | ForEach-Object { $_.Line.Trim() }
}

function Get-PrometheusScalar {
    param([string]$Query)

    $encodedQuery = [uri]::EscapeDataString($Query)
    $response = Invoke-RestMethod -Uri "$PrometheusUrl/api/v1/query?query=$encodedQuery"
    if (-not $response.data.result -or $response.data.result.Count -eq 0) {
        return $null
    }

    return [double]$response.data.result[0].value[1]
}

Write-Step "Checking stack availability"
Ensure-StackRunning

Write-Step "Generating article cache misses"
$continent = "Europe"
$articleMissIds = @()
for ($index = 0; $index -lt 3; $index++) {
    $missId = [guid]::NewGuid().ToString()
    $articleMissIds += $missId
    Invoke-SafeGet -Url "$ArticleBaseUrl/api/articles/${missId}?continent=${continent}"
}

Write-Step "Generating article cache hits"
$articleBody = @{
    title = "Observability demo article"
    content = "Proof-of-concept traffic for Seq and Grafana"
    continent = $continent
    publisherId = "33333333-3333-3333-3333-333333333333"
} | ConvertTo-Json

$createdArticle = Invoke-RestMethod -Method Post -Uri "$ArticleBaseUrl/api/articles" -ContentType "application/json" -Body $articleBody

1..4 | ForEach-Object {
    Invoke-RestMethod -Method Get -Uri "$ArticleBaseUrl/api/articles/$($createdArticle.id)?continent=$continent" | Out-Null
}

Write-Step "Generating comment cache miss and hits"
$commentArticleId = [guid]::NewGuid().ToString()

1..2 | ForEach-Object {
    $commentBody = @{
        articleId = $commentArticleId
        authorId = "22222222-2222-2222-2222-222222222222"
        body = "Observability demo comment"
    } | ConvertTo-Json

    Invoke-RestMethod -Method Post -Uri "$CommentBaseUrl/api/comments" -ContentType "application/json" -Body $commentBody | Out-Null
}

1..4 | ForEach-Object {
    Invoke-RestMethod -Method Get -Uri "$CommentBaseUrl/api/comments/article/$commentArticleId" | Out-Null
}

Write-Step "Waiting for Prometheus scrape"
Start-Sleep -Seconds 20

$articleMetricLines = Get-MetricsLines -MetricsUrl "$ArticleBaseUrl/metrics" -Patterns @(
    "article_cache_hits_total",
    "article_cache_misses_total"
)

$commentMetricLines = Get-MetricsLines -MetricsUrl "$CommentBaseUrl/metrics" -Patterns @(
    "comment_cache_hits_total",
    "comment_cache_misses_total",
    "comment_cache_size_articles"
)

$articleRatio = Get-PrometheusScalar -Query "sum(increase(article_cache_hits_total[15m])) / clamp_min(sum(increase(article_cache_hits_total[15m])) + sum(increase(article_cache_misses_total[15m])), 1)"
$commentRatio = Get-PrometheusScalar -Query "sum(increase(comment_cache_hits_total[15m])) / clamp_min(sum(increase(comment_cache_hits_total[15m])) + sum(increase(comment_cache_misses_total[15m])), 1)"

Write-Step "Proof-of-concept summary"
Write-Host "Article MISS ids:" -ForegroundColor Yellow
$articleMissIds | ForEach-Object { Write-Host "  $_" }

Write-Host "`nArticle HIT id:" -ForegroundColor Yellow
Write-Host "  $($createdArticle.id)"

Write-Host "`nComment cache article id:" -ForegroundColor Yellow
Write-Host "  $commentArticleId"

Write-Host "`nArticle metrics:" -ForegroundColor Yellow
$articleMetricLines | ForEach-Object { Write-Host "  $_" }

Write-Host "`nComment metrics:" -ForegroundColor Yellow
$commentMetricLines | ForEach-Object { Write-Host "  $_" }

Write-Host "`nPrometheus ratios:" -ForegroundColor Yellow
if ($null -ne $articleRatio) {
    Write-Host ("  Article hit ratio: {0:P2}" -f $articleRatio)
}
if ($null -ne $commentRatio) {
    Write-Host ("  Comment hit ratio: {0:P2}" -f $commentRatio)
}

Write-Host "`nOpen these URLs:" -ForegroundColor Yellow
Write-Host "  Seq: $SeqUrl"
Write-Host "  Grafana: $GrafanaUrl"

Write-Host "`nPaste these into Seq:" -ForegroundColor Yellow
Write-Host "  ServiceName = 'article-service' and (@Message like '%Cache MISS for article%' or @Message like '%Cache HIT for article%' or @Message like '%not in cache%' or @Message like '%served from cache%')"
Write-Host "  ServiceName = 'comment-service' and (@Message like '%Comment cache MISS%' or @Message like '%Comment cache HIT%' or @Message like '%not in cache%' or @Message like '%served from cache%')"

Write-Host "`nGrafana dashboard:" -ForegroundColor Yellow
Write-Host "  HappyHeadlines - Cache Hit Ratios"
Write-Host "  Suggested time range: Last 15 minutes"