namespace ProActive.SharePoint.Build.Console.Extensions
{
    using System;

    public static class ConsoleExtensions
    {
        public static object colorLock = new object();

        public static void WriteLineWithColor(string output, ConsoleColor consoleColor)
        {
            lock (colorLock)
            {
                var previousColor = Console.ForegroundColor;
                Console.ForegroundColor = consoleColor;
                Console.WriteLine(output);
                Console.ForegroundColor = previousColor;
            }
        }
    }
}
