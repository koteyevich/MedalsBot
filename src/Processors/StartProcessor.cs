using MedalsBot.Models;
using MedalsBot.Utils;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MedalsBot.Processors
{
    public static class StartProcessor
    {
        public static async Task ProcessStartAsync(Message message, TelegramBotClient? bot, Database? db)
        {
            Logger.Command("Processing /start", "INFO");
            string[]? args = message.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (message.Chat.Type == ChatType.Private)
            {
                if (args?.Length > 1)
                {
                    string medalId = args[1];

                    long userId = message.From?.Id ?? 0;
                    long chatId = message.Chat.Id;

                    var result = db?.DeleteMedal(medalId, userId) ?? DeleteResult.Error;
                    string response = result switch
                    {
                        DeleteResult.Success => "✅ Медаль успешно удалена.",
                        DeleteResult.NotFound => "⚠️ Медаль не найдена или уже удалена.",
                        DeleteResult.Unauthorized => "🚫 У тебя нет прав удалить эту медаль.",
                        _ => "❌ Произошла ошибка при удалении медали."
                    };

                    await bot.SendMessage(chatId, response, parseMode: ParseMode.Html);

                    return; // prevent sending welcome msg after
                }
            }

            if (bot != null)
            {
                var inlineKeyboard = new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        text: "Добавить в группу",
                        url: $"t.me/{(await bot.GetMe()).Username}?startgroup=dm"
                    )
                );

                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Привет! Я - бот для награждения медалек в группе.\n" +
                          "Добавь меня в группу по кнопке ниже:",
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.Html,
                    linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true }
                );
            }

            if (message.From != null)
            {
                var context = new Dictionary<string, object>
                {
                    { "chatId", message.Chat.Id },
                    { "userId", message.From.Id }
                };
                Logger.LogWithContext("Displayed inline button for group addition", context, "COMMAND", "SUCCESS");
            }
        }
    }
}
