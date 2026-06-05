namespace FlowMarketService.Infrastructure;

public static class CardValidation
{
    public static bool LuhnCheck(ReadOnlySpan<char> number)
    {
        var sum = 0;
        var alternate = false;
        for (var i = number.Length - 1; i >= 0; i--)
        {
            var c = number[i];
            if (c is < '0' or > '9')
                return false;
            var n = c - '0';
            if (alternate)
            {
                n *= 2;
                if (n > 9)
                    n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }

    public static string DetectBrand(ReadOnlySpan<char> digits)
    {
        if (digits.Length == 0)
            return "Unknown";
        if (digits[0] == '4')
            return "Visa";
        if (digits[0] == '5' || (digits[0] == '2' && digits.Length >= 4))
            return "Mastercard";
        return "Card";
    }
}
