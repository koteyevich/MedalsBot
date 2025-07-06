using System.Text;
using LiteDB;
using MedalsBot.Models;
using MedalsBot.Utils;
using User = MedalsBot.Models.User;

namespace MedalsBot
{
    public class Database
    {
        private LiteDatabase? db;
        private ILiteCollection<User>? userCollection;


        public void Initialize()
        {
            //! Database HAS to be initialized, otherwise every interaction with db is DOOMED TO FAIL
            db = new LiteDatabase(@"./medals.db");
            userCollection = db.GetCollection<User>("users");

            Console.WriteLine($"Users:\n" +
                              $"Count: {userCollection.Count()}\n");
        }


        /// <summary>
        ///
        /// </summary>
        public void AddMedal(long recipientUserId, long chatId, string? recipientUsername, MedalType medalType,
            string explanation, string? originalMessage)
        {
            try
            {
                var user = userCollection?.FindOne(x => x.ChatId == chatId && x.Id == recipientUserId) ?? new User
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

                userCollection?.Upsert(user);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public string ShowUserMedal(long userId, long chatId)
        {
            var user = userCollection?.FindOne(x => x.ChatId == chatId && x.Id == userId);

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
                (string emoji, string name) = medalInfoMap.TryGetValue(medal.MedalType, out var info)
                    ? info
                    : ("", medal.MedalType.ToString());

                string? explanation = string.IsNullOrWhiteSpace(medal.Explanation) ? "<i>null</i>" : medal.Explanation;
                string? originalMsg = string.IsNullOrWhiteSpace(medal.OriginalMessage)
                    ? "<i>нет сообщения</i>"
                    : medal.OriginalMessage;
                string date = medal.AwardedAt.ToString("yyyy-MM-dd");

                string deepLink = $"https://t.me/medalkimedal_bot?start={medal.Id}";

                sb.AppendLine(
                    $"{emoji} <b>{name}</b> — <b>За:</b> {explanation}\n" +
                    $"<blockquote>{originalMsg}</blockquote>\n" +
                    $"(получено: {date}) [<a href=\"{deepLink}\">Удалить медаль</a>]");
            }


            return sb.ToString();
        }

        public DeleteResult DeleteMedal(string medalId, long requesterUserId)
        {
            if (userCollection == null) return DeleteResult.Error;

            var users = userCollection.Find(u => u.Medals != null);

            var user = users.FirstOrDefault(u => u.Medals.Any(m => m.Id == medalId));

            if (user == null) return DeleteResult.NotFound;

            var medal = user.Medals.First(m => m.Id == medalId);

            if (user.Id != requesterUserId)
                return DeleteResult.Unauthorized;

            user.Medals.Remove(medal);

            userCollection.Update(user);

            return DeleteResult.Success;
        }
    }
}
