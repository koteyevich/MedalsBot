using System.Text;

namespace MedalsBot.Utils
{
    public static class IdGenerator
    {
        public static long GenerateMedalId()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static string EncodeBase36(long number)
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            var result = new StringBuilder();
            do
            {
                result.Insert(0, chars[(int)(number % 36)]);
                number /= 36;
            } while (number > 0);

            return result.ToString();
        }
    }
}
