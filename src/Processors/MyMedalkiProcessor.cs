using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MedalsBot.Processors;

public class MyMedalkiProcessor
{
    public static async Task ProcessMyMedalkiAsync(Message message, TelegramBotClient? bot, Database? db)
    {
        if (message.From != null)
            if (bot != null)
                if (db != null)
                    await bot.SendMessage(message.Chat.Id, db.ShowUserMedal(message.From.Id, message.Chat.Id),
                        ParseMode.Html);
    }
}