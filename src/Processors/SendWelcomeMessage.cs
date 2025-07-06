using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace MedalsBot.Processors;

public static class SendWelcomeMessage
{
    public static async Task SendWelcomeMessageAsync(long chatId, TelegramBotClient bot)
    {
        const string welcomeMessage = "Привет, я ваш бот для медалек!\n" +
                                      "<b>Спасибо за доверие.</b>";
        await bot.SendMessage(
            chatId: chatId,
            text: welcomeMessage,
            parseMode: ParseMode.Html
        );
    }
}