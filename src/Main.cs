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
        private static Database? db;
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

            db = new Database();
            db.Initialize();

            commandRegistry = new CommandRegistry(db);

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

                if (message.Text.StartsWith('/'))
                {
                    if (message.Text.StartsWith("/start"))
                    {
                        await StartProcessor.ProcessStartAsync(message, bot, db);
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

        private static Task OnUpdate(Update update)
        {
            return Task.CompletedTask;
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
