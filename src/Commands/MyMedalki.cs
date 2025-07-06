using MedalsBot.Interfaces;
using MedalsBot.Processors;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MedalsBot.Commands
{
    public class MyMedalki : CommandBase
    {
        public override string Name => "/my_medalki";

        public override string Description => "Твои медальки.";

        public override string[] Aliases => ["/mm"];

        protected override async Task ExecuteCoreAsync(Message message, TelegramBotClient? bot, Database? db)
        {
            await MyMedalkiProcessor.ProcessMyMedalkiAsync(message, bot, db);
        }
    }
}
