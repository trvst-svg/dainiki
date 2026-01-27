namespace dainiki.Components.Services
{
    using System.Net;
    using System.Text;

    public static class TextHelpers
    {
        public static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            bool insideTag = false;

            for (int i = 0; i < html.Length; i++)
            {
                char ch = html[i];
                if (ch == '<')
                {
                    insideTag = true;
                    continue;
                }

                if (ch == '>')
                {
                    insideTag = false;
                    continue;
                }

                if (!insideTag)
                {
                    builder.Append(ch);
                }
            }

            string decoded = WebUtility.HtmlDecode(builder.ToString());
            return NormalizeWhitespace(decoded);
        }

        public static int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            int count = 0;
            bool inWord = false;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (char.IsWhiteSpace(ch))
                {
                    if (inWord)
                    {
                        count++;
                        inWord = false;
                    }
                }
                else
                {
                    inWord = true;
                }
            }

            if (inWord)
            {
                count++;
            }

            return count;
        }

        public static bool IsContentEmpty(string html)
        {
            string plain = StripHtml(html);
            return string.IsNullOrWhiteSpace(plain);
        }

        private static string NormalizeWhitespace(string text)
        {
            StringBuilder builder = new StringBuilder();
            bool lastWasSpace = false;

            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (char.IsWhiteSpace(ch))
                {
                    if (!lastWasSpace)
                    {
                        builder.Append(' ');
                        lastWasSpace = true;
                    }
                }
                else
                {
                    builder.Append(ch);
                    lastWasSpace = false;
                }
            }

            return builder.ToString().Trim();
        }
    }
}
