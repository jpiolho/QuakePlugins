using System.Drawing;

namespace QuakePlugins.API
{
    /// <apiglobal />
    public class QConsole
    {

        /// <summary>
        /// Prints some text to the console.
        /// Note that this method does not include the newline character at the end. Make sure you add it yourself whenever you're done, otherwise your text will not be printed.
        /// </summary>
        public static void Print(string text) => Quake.PrintConsole(text);
        public static void Print(string text, Color color) => Quake.PrintConsole(text, color);
        public static void Print(string text, uint color) => Quake.PrintConsole(text, color);

        /// <summary>
        /// Prints a line of text to the console.
        /// </summary>
        public static void PrintLine(string text) => Print($"{text}\n");
        public static void PrintLine(string text, Color color) => Print($"{text}\n",color);
        public static void PrintLine(string text, uint color) => Print($"{text}\n",color);
    }
}
