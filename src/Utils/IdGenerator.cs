using System.Text;

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


    public static long DecodeBase36(string str)
    {
        const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
        long result = 0;
        foreach (var c in str)
        {
            result = result * 36 + chars.IndexOf(c);
        }

        return result;
    }
}