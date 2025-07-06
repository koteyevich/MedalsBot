using MedalsBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MedalsBot.Processors;

public abstract class GiveMedalkaProcessor
{
    public static async Task ProcessGiveMedalkaAsync(Message message, TelegramBotClient? bot, Database? db)
    {
        if (message.ReplyToMessage?.From == null)
        {
            if (bot != null)
                await bot.SendMessage(message.Chat.Id,
                    "Тебе надо ответить на сообщение человека которому ты хочешь подарить медаль.");
            return;
        }

        if (message.From != null && message.From.Id == message.ReplyToMessage.From.Id)
        {
            if (bot != null)
                await bot.SendMessage(message.Chat.Id, "Самолечение может быть вредным для вашего здоровья.");
            return;
        }

        var parts = message.Text?.Split(' ', 3);

        if (parts == null || parts.Length < 2)
        {
            if (bot != null)
                await bot.SendMessage(message.Chat.Id, "Использование: /givemedalka (1,2,3,4,5) <сообщение>");
            return;
        }

        if (!int.TryParse(parts[1], out var medalTypeInt) || medalTypeInt is < 1 or > 5)
        {
            if (bot != null)
                await bot.SendMessage(message.Chat.Id, "Неправильный тип медали. Должно быть цифрой от 1 до 5.");
            return;
        }

        var medalType = (MedalType)medalTypeInt;
        var explanation = parts.Length >= 3 && !string.IsNullOrWhiteSpace(parts[2])
            ? parts[2]
            : "<i>null</i>"; // fallback when explanation is missing or empty
        var originalMessageText = message.ReplyToMessage.Text ?? "<i>нет сообщения</i>";

        // recipient info from the replied-to message
        var recipientUserId = message.ReplyToMessage.From.Id;
        var recipientUsername = message.ReplyToMessage.From.Username;
        var chatId = message.Chat.Id;

        db?.AddMedal(recipientUserId, chatId, recipientUsername, medalType, explanation, originalMessageText);

        var medalTypeDictionary = new Dictionary<MedalType, string>
        {
            { MedalType.Bronze, "🥉" },
            { MedalType.Silver, "🥈" },
            { MedalType.Gold, "🥇" },
            { MedalType.OrdenMedal, "🎖" },
            { MedalType.Trophy, "🏆" }
        };

        if (bot != null)
            await bot.SendMessage(message.Chat.Id,
                $"Медаль {medalTypeDictionary[medalType]} награждена {recipientUsername ?? recipientUserId.ToString()} с сообщением:\n\"{explanation}\"",
                Telegram.Bot.Types.Enums.ParseMode.Html);
    }
}