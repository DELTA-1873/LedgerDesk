using System.Globalization;

namespace LedgerDesk;

public static class AmountExpression
{
    public static bool TryEvaluate(string? expression, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(expression)) return false;
        try
        {
            var parser = new Parser(expression.Replace('×', '*').Replace('÷', '/').Replace('，', '.').Replace(',', '.'));
            value = parser.Parse();
            return value > 0 && value <= 999_999_999_999m;
        }
        catch { return false; }
    }

    sealed class Parser(string source)
    {
        int position;

        public decimal Parse()
        {
            var result = Expression();
            WhiteSpace();
            if (position != source.Length) throw new FormatException();
            return decimal.Round(result, 2, MidpointRounding.AwayFromZero);
        }

        decimal Expression()
        {
            var value = Term();
            while (true)
            {
                WhiteSpace();
                if (Take('+')) value += Term();
                else if (Take('-')) value -= Term();
                else return value;
            }
        }

        decimal Term()
        {
            var value = Factor();
            while (true)
            {
                WhiteSpace();
                if (Take('*')) value *= Factor();
                else if (Take('/'))
                {
                    var divisor = Factor();
                    if (divisor == 0) throw new DivideByZeroException();
                    value /= divisor;
                }
                else return value;
            }
        }

        decimal Factor()
        {
            WhiteSpace();
            if (Take('+')) return Factor();
            if (Take('-')) return -Factor();
            if (Take('('))
            {
                var value = Expression();
                WhiteSpace();
                if (!Take(')')) throw new FormatException();
                return value;
            }
            var start = position;
            while (position < source.Length && (char.IsDigit(source[position]) || source[position] == '.')) position++;
            if (start == position || !decimal.TryParse(source[start..position], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var number)) throw new FormatException();
            return number;
        }

        bool Take(char expected)
        {
            if (position >= source.Length || source[position] != expected) return false;
            position++;
            return true;
        }

        void WhiteSpace() { while (position < source.Length && char.IsWhiteSpace(source[position])) position++; }
    }
}
