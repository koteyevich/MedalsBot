using MedalsBot.Interfaces;
using MedalsBot.Processors;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MedalsBot.Commands
{
    public class GiveMedalka : CommandBase
    {
        public override string Name => "/givemedalka";

        public override string Description => "Даёт медальку.";

        public override string[] Aliases => ["/gm"];

        protected override async Task ExecuteCoreAsync(Message message, TelegramBotClient? bot, Database? db)
        {
            await GiveMedalkaProcessor.ProcessGiveMedalkaAsync(message, bot, db);
        }
    }
}
