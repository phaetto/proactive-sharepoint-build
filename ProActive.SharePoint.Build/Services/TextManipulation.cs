namespace ProActive.SharePoint.Build.Services
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class TextManipulation
    {
        private static readonly TextInfo TextInfo = CultureInfo.InvariantCulture.TextInfo;
        private static readonly Random random = new Random();

        public static string ToPascalCase(string input)
        {
            var processedInput = Regex.Replace(input, @"[^A-Za-z ]+", string.Empty);
            return TextInfo.ToTitleCase(processedInput)
                .Replace(" ", "")
                .Trim();
        }

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
