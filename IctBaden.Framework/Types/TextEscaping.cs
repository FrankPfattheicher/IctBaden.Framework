namespace IctBaden.Framework.Types;

public static class TextEscaping
{
    public static string RemoveQuotes(string text)
    {
        if (text.StartsWith("\"", System.StringComparison.OrdinalIgnoreCase) && 
            text.EndsWith("\"", System.StringComparison.OrdinalIgnoreCase))
        {
            return text.Substring(1, text.Length - 2);
        }
        if (text.StartsWith("'", System.StringComparison.OrdinalIgnoreCase) && 
            text.EndsWith("'", System.StringComparison.OrdinalIgnoreCase))
        {
            return text.Substring(1, text.Length - 2);
        }
        return text;
    }
}