namespace QuakePlugins.API
{
    /// <apiglobal />
    public class Console
    {
        /// <summary>
        /// Prints some text to the console.
        /// Note that this method does not include the newline character at the end. Make sure you add it yourself whenever you're done, otherwise your text might not appear correctly.
        /// </summary>
        public static void Print(string text, uint? color = null)
        {
            if (color.HasValue)
                Quake.PrintConsole(text, color.Value);
            else
                Quake.PrintConsole(text);
        }

        /// <summary>
        /// Prints a line of text to the console.
        /// </summary>
        public static void PrintLine(string text, uint? color = null)
        {
            Print(text + "\n", color);
        }
    }
}
