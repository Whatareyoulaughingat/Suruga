using System;

namespace Suruga.Installer.Converters
{
    public class BytesToMbConverter
    {
        private static readonly string[] SizeSuffixes = { "B", "KB", "MB" };

        /// <summary>
        /// Converts the specified value into a size suffix such as KB, MB, etc.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="decimalPlaces">The place of the decimal point.</param>
        /// <returns>[<see cref="string"/>] The converted value.</returns>
        public static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exception: The decimal place is out of range.");
                Console.ResetColor();

                Console.ReadLine();
                Environment.Exit(0);
            }

            if (value < 0)
            {
                return "-" + SizeSuffix(-value, decimalPlaces);
            }

            if (value == 0)
            {
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
            }

            // Mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // Make adjustment when the value is large enough that it would round up to 1000 or more.
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}
