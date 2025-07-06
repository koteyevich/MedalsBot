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
        private static TelegramBotClient? bot;
        private static CancellationTokenSource? cts;
        public static Database? Db;
        private static long botId;
        private static CommandRegistry? commandRegistry;

        public static async Task Main()
        {
            Logger.Bot("Bot starting", "INFO");

            cts = new CancellationTokenSource();
            bot = new TelegramBotClient(Secrets.TOKEN);
            var me = await bot.GetMe();
            botId = me.Id;

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

            commandRegistry = new CommandRegistry(Db);

            bot.OnMessage += async (message, _) =>
            {
                if (message.NewChatMembers != null)
                {
                    if (message.NewChatMembers.Any(m => m.Id == botId))
                    {
                        await SendWelcomeMessage.SendWelcomeMessageAsync(message.Chat.Id, bot);
                        await commandRegistry.SendHelpMessage(message, bot);
                    }
                }

                await OnMessage(message);
            };
            bot.OnUpdate += OnUpdate;

            AppDomain.CurrentDomain.ProcessExit += (_, _) => cts?.Cancel();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts?.Cancel();
            };

            await Task.Delay(Timeout.Infinite, cts.Token);
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
                        await StartProcessor.ProcessStartAsync(message, bot, Db);
                    }
                    else
                    {
                        await commandRegistry?.HandleCommandAsync(message, bot)!;
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
            if (bot != null)
            {
                await bot.SendMessage(chatId,
                    $"<b>Ах!</b> <i>Что-то пошло не так...</i> \n<blockquote expandable><i>{exception.Message}</i></blockquote>",
                    ParseMode.Html);
            }
        }
    }
}
