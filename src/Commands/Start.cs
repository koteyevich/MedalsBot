using MedalsBot.Interfaces;
using MedalsBot.Processors;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MedalsBot.Commands
{
    public class Start : CommandBase
    {
        public override string Name => "/start";
        public override string Description => "Запускает бота.";
        
        public override string[] Aliases => [];

        protected override async Task ExecuteCoreAsync(Message message, TelegramBotClient? bot, Database? db)
        {
            await StartProcessor.ProcessStartAsync(message, bot, db);
        }
    }
}