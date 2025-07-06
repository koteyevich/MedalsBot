using System.Text;
using LiteDB;
using MedalsBot.Models;
using User = MedalsBot.Models.User;

namespace MedalsBot
{
    public class Database
    {
        private LiteDatabase? _db;
        private ILiteCollection<User>? _userCollection;


        public void Initialize()
        {
            //! Database HAS to be initialized, otherwise every interaction with db is DOOMED TO FAIL
            _db = new LiteDatabase(@"./medals.db");
            _userCollection = _db.GetCollection<User>("users");

            Console.WriteLine($"Users:\n" +
                              $"Count: {_userCollection.Count()}\n");
        }


        /// <summary>
        ///  
        /// </summary>
        public void AddMedal(long recipientUserId, long chatId, string? recipientUsername, MedalType medalType,
            string explanation, string? originalMessage)
        {
            try
            {
                var user = _userCollection?.FindOne(x => x.ChatId == chatId && x.Id == recipientUserId) ?? new User
                {
                    Id = recipientUserId,
                    ChatId = chatId,
                    Username = recipientUsername,
                    Medals = new List<Medal>()
                };

                user.Medals ??= new List<Medal>();

                user.Medals.Add(new Medal
                {
                    Id = IdGenerator.EncodeBase36(IdGenerator.GenerateMedalId()),
                    MedalType = medalType,
                    Explanation = explanation,
                    OriginalMessage = originalMessage,
                    AwardedAt = DateTime.UtcNow
                });

                _userCollection?.Upsert(user);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public string ShowUserMedal(long userId, long chatId)
        {
            var user = _userCollection?.FindOne(x => x.ChatId == chatId && x.Id == userId);

            if (user?.Medals == null || user.Medals.Count == 0)
            {
                return "У тебя пока нет медалей.";
            }

            var medalInfoMap = new Dictionary<MedalType, (string Emoji, string Name)>
            {
                { MedalType.Bronze, ("🥉", "Бронза") },
                { MedalType.Silver, ("🥈", "Серебро") },
                { MedalType.Gold, ("🥇", "Золото") },
                { MedalType.OrdenMedal, ("🎖", "Орден") },
                { MedalType.Trophy, ("🏆", "Трофей") }
            };

            var sb = new StringBuilder();
            sb.AppendLine("<b>Твои медали:</b>");

            foreach (var medal in user.Medals)
            {
                var (emoji, name) = medalInfoMap.TryGetValue(medal.MedalType, out var info)
                    ? info
                    : ("", medal.MedalType.ToString());

                var explanation = string.IsNullOrWhiteSpace(medal.Explanation) ? "<i>null</i>" : medal.Explanation;
                var originalMsg = string.IsNullOrWhiteSpace(medal.OriginalMessage)
                    ? "<i>нет сообщения</i>"
                    : medal.OriginalMessage;
                var date = medal.AwardedAt.ToString("yyyy-MM-dd");

                var deepLink = $"https://t.me/medalkimedal_bot?start={medal.Id}";

                sb.AppendLine(
                    $"{emoji} <b>{name}</b> — <b>За:</b> {explanation}\n" +
                    $"<blockquote>{originalMsg}</blockquote>\n" +
                    $"(получено: {date}) [<a href=\"{deepLink}\">Удалить медаль</a>]");
            }


            return sb.ToString();
        }

        public DeleteResult DeleteMedal(string medalId, long requesterUserId)
        {
            if (_userCollection == null) return DeleteResult.Error;

            var users = _userCollection.Find(u => u.Medals != null);

            var user = users.FirstOrDefault(u => u.Medals.Any(m => m.Id == medalId));

            if (user == null) return DeleteResult.NotFound;

            var medal = user.Medals.First(m => m.Id == medalId);

            if (user.Id != requesterUserId)
                return DeleteResult.Unauthorized;

            user.Medals.Remove(medal);

            _userCollection.Update(user);

            return DeleteResult.Success;
        }
    }
}