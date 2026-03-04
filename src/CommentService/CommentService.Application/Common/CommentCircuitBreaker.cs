namespace CommentService.Application.Common;

/// <summary>
/// **Critical:** This class is registered as a **Singleton** in DI.
/// If registered as Scoped, the circuit resets on every request and never actually opens.
/// </summary>
public sealed class CommentCircuitBreaker
{
    private enum State
    {
        Closed,
        Open,
        HalfOpen,
    }

    private readonly int _failureThreshold = 3;
    private readonly TimeSpan _openDuration = TimeSpan.FromSeconds(30);

    private State _state = State.Closed;
    private int _failureCount;
    private DateTime _openedAt;

    public bool IsOpen => GetCurrentState() == State.Open;

    // Returns the profanity check result, or null when the circuit is open (caller should flag the comment)
    public async Task<bool?> ExecuteAsync(Func<Task<bool>> profanityCall)
    {
        var current = GetCurrentState();

        if (current == State.Open)
            return null;

        try
        {
            var result = await profanityCall();
            OnSuccess();
            return result;
        }
        catch
        {
            OnFailure();
            return null;
        }
    }

    private State GetCurrentState()
    {
        if (_state == State.Open && DateTime.UtcNow - _openedAt >= _openDuration)
        {
            _state = State.HalfOpen;
        }

        return _state;
    }

    private void OnSuccess()
    {
        _failureCount = 0;
        _state = State.Closed;
    }

    private void OnFailure()
    {
        _failureCount++;
        if (_failureCount >= _failureThreshold)
        {
            _state = State.Open;
            _openedAt = DateTime.UtcNow;
        }
    }
}
