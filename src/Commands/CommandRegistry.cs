using MedalsBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MedalsBot.Commands
{
    public class CommandRegistry
    {
        private readonly Dictionary<string, ICommand> commands = new();
        private readonly Database? db;

        public CommandRegistry(Database? db = null)
        {
            this.db = db;

            var commandList = new List<ICommand>
            {
                new GiveMedalka(),
                new MyMedalki(),
                new Start(),
                // Add more commands here
            };

            foreach (var command in commandList)
            {
                commands[command.Name.ToLower()] = command;

                foreach (string alias in command.Aliases)
                {
                    commands[alias.ToLower()] = command;
                }
            }
        }


        public async Task HandleCommandAsync(Message message, TelegramBotClient? bot)
        {
            if (string.IsNullOrEmpty(message.Text))
                return;

            string commandText = message.Text.Split(' ')[0].ToLower();

            string normalizedCommand = commandText.Split('@')[0];

            var matchingCommand = commands
                .FirstOrDefault(kvp => normalizedCommand.StartsWith(kvp.Key));

            if (matchingCommand.Value != null)
            {
                await matchingCommand.Value.ExecuteAsync(message, bot, db);
            }
            else if (normalizedCommand.StartsWith("/help"))
            {
                await SendHelpMessage(message, bot);
            }
        }

        public async Task SendHelpMessage(Message message, TelegramBotClient? bot)
        {
            string helpMessage = "<b>Доступные команды:</b>\n\n";
            var listed = new HashSet<ICommand>();

            foreach (var command in commands.Values.Distinct())
            {
                if (!listed.Add(command)) continue;

                string aliasText = command.Aliases.Length > 0
                    ? $" ({string.Join(", ", command.Aliases)})"
                    : "";

                helpMessage += $"{command.Name}{aliasText} - {command.Description}\n";
            }

            helpMessage += "\n";
            helpMessage += "<b>Объяснение медалек:</b>\n" +
                           "<i>TLDR; 5 - худшая, бронза; 1 - лучшая, трофей</i>\n" +
                           "<b>1 (🏆)</b> - <i>трофей, легенда чата</i>\n" +
                           "<b>2 (🎖)</b> - <i>орден, база</i>\n" +
                           "<b>3 (🥇)</b> - <i>золото, лучший</i>\n" +
                           "<b>4 (🥈)</b> - <i>серебро, крутой челик</i>\n" +
                           "<b>5 (🥉)</b> - <i>бронза, неплохо сказано</i>\n";

            helpMessage += "\n";
            helpMessage += "<b>Правила для бота</b>\n" +
                           "- Не давать и не злоупотреблять медальками без смысла!\n" +
                           "- Давайте медальки за классные высказывания!\n" +
                           "<i>Эти правила существуют, но не значит что мы можем остановить вас :)</i>";

            if (bot != null)
            {
                await bot.SendMessage(message.Chat.Id, helpMessage, Telegram.Bot.Types.Enums.ParseMode.Html,
                    linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true });
            }
        }

        public IReadOnlyDictionary<string, ICommand> Commands => commands.AsReadOnly();
    }
}
