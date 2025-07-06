namespace MedalsBot.Models
{
    public class User
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public string? Username { get; set; }
        public List<Medal>? Medals { get; set; }
    }
}
