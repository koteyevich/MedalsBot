namespace MedalsBot.Models;

public class Medal
{
    public MedalType MedalType { get; set; }
    public string Id { get; set; }
    public string? OriginalMessage { get; set; }
    public string? Explanation { get; set; }
    public DateTime AwardedAt { get; set; }
}