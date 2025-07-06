using Telegram.Bot;
using Telegram.Bot.Types;

namespace MedalsBot.Interfaces
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        string[] Aliases { get; }
        Task ExecuteAsync(Message message, TelegramBotClient? bot, Database? db);
        bool RequiresAdmin { get; }
    }

    public abstract class CommandBase : ICommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string[] Aliases { get; }
        public virtual bool RequiresAdmin => true;

        public async Task ExecuteAsync(Message message, TelegramBotClient? bot, Database? db)
        {
            await ExecuteCoreAsync(message, bot, db);
        }

        protected abstract Task ExecuteCoreAsync(Message message, TelegramBotClient? bot, Database? db);
    }
}