using System.Text;
using MedalsBot.Commands;
using MedalsBot.Processors;
using MedalsBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MedalsBot
{
    public static class Program
    {
        private static TelegramBotClient? _bot;
        private static CancellationTokenSource? _cts;
        public static Database? Db;
        private static long _botId;
        private static CommandRegistry? CommandRegistry;

        public static async Task Main()
        {
            Logger.Bot("Bot starting", "INFO");

            _cts = new CancellationTokenSource();
            _bot = new TelegramBotClient(Secrets.Token);
            var me = await _bot.GetMe();
            _botId = me.Id;

            Logger.Bot($"Bot connected as @{me.Username}", "SUCCESS");

            Db = new Database();
            Db.Initialize();

            /*
            try
            {
                Db = new Database("moderatorbot.sql");
                Logger.Database("Database initialized", "SUCCESS");
            }
            catch (BotException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to initialize database: {ex.Message}", "DATABASE");
                return;
            }
            */

            CommandRegistry = new CommandRegistry(Db);

            _bot.OnMessage += async (message, _) =>
            {
                if (message.NewChatMembers != null)
                {
                    if (message.NewChatMembers.Any(m => m.Id == _botId))
                    {
                        await SendWelcomeMessage.SendWelcomeMessageAsync(message.Chat.Id, _bot);
                        await CommandRegistry.SendHelpMessage(message, _bot);
                    }
                }

                await OnMessage(message);
            };
            _bot.OnUpdate += OnUpdate;

            AppDomain.CurrentDomain.ProcessExit += (_, _) => _cts?.Cancel();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                _cts?.Cancel();
            };

            await Task.Delay(Timeout.Infinite, _cts.Token);
            Logger.Bot("Bot shutting down", "INFO");
        }

        private static async Task OnMessage(Message message)
        {
            try
            {
                if (message.Text == null)
                {
                    return;
                }

                if (message.Text.StartsWith("/"))
                {
                    if (message.Text.StartsWith("/start"))
                    {
                        await StartProcessor.ProcessStartAsync(message, _bot, Db);
                    }
                    else
                    {
                        await CommandRegistry?.HandleCommandAsync(message, _bot)!;
                    }
                }
            }
            catch (Exception ex)
            {
                await OnError(ex, message.Chat.Id);
            }
        }

        private static async Task OnUpdate(Update update)
        {
        }

        private static async Task OnError(Exception exception, long chatId)
        {
            if (_bot != null)
            {
                await _bot.SendMessage(chatId,
                    $"<b>Ах!</b> <i>Что-то пошло не так...</i> \n<blockquote expandable><i>{exception.Message}</i></blockquote>",
                    ParseMode.Html);
            }
        }
    }
}