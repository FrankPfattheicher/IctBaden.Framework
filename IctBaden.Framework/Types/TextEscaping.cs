namespace IctBaden.Framework.Types
{
    public static class TextEscaping
    {
        public static string RemoveQuotes(string text)
        {
            if (text.StartsWith("\"") && text.EndsWith("\""))
            {
                return text.Substring(1, text.Length - 2);
            }
            if (text.StartsWith("'") && text.EndsWith("'"))
            {
                return text.Substring(1, text.Length - 2);
            }
            return text;
        }
    }
}