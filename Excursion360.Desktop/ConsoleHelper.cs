using System;

namespace Excursion360.Desktop
{
    public static class ConsoleHelper
    {
        public static int SelectOneFromArray(string headerLine, string[] values)
        {
            values = values ?? throw new ArgumentNullException(nameof(values));
            var index = 0;
            while (true)
            {
                Console.Clear();
                Console.WriteLine(headerLine);
                for (int i = 0; i < values.Length; i++)
                {
                    Console.BackgroundColor = index == i ? ConsoleColor.White : ConsoleColor.Black;
                    Console.ForegroundColor = index == i ? ConsoleColor.Black : ConsoleColor.White;
                    Console.WriteLine(values[i]);
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        index = index <= 0 ? 0 : index - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        index = index + 1 >= values.Length ? values.Length - 1 : index + 1;
                        break;
                    case ConsoleKey.Enter:
                        return index;
                    default:
                        break;
                }
            }
        }
    }
}
