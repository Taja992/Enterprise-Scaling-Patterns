namespace ProfanityService.Domain.Entities;

public class ProfaneWord
{
    public Guid Id { get; private set; }
    public string Word { get; private set; } = string.Empty;

    private ProfaneWord() { }

    public static ProfaneWord Create(string word)
    {
        return new ProfaneWord { Id = Guid.NewGuid(), Word = word.ToLowerInvariant() };
    }
}
