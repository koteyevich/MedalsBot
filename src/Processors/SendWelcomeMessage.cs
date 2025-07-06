using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace MedalsBot.Processors
{
    public static class SendWelcomeMessage
    {
        public static async Task SendWelcomeMessageAsync(long chatId, TelegramBotClient bot)
        {
            const string welcome_message = "Привет, я ваш бот для медалек!\n" +
                                           "<b>Спасибо за доверие.</b>";
            await bot.SendMessage(
                chatId: chatId,
                text: welcome_message,
                parseMode: ParseMode.Html
            );
        }
    }
}
