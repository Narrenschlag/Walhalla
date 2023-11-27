namespace Walhalla
{
    public static class Debug
    {
        #region Logging
        /// <summary>
        /// Writes a new line into the console
        /// </summary>
        public static void Log(this object line) => Console.WriteLine(line);

        /// <summary>
        /// Writes plain text with no paragraphs
        /// </summary>
        public static void Log2(this object line) => Console.Write(line);
        #endregion

        #region Text Color
        /// <summary>
        /// Set console color to color code
        /// </summary>
        public static void SetColor(this int colorCode) => Log2($"\x1b[{colorCode}m");

        /// <summary>
        /// Returns color string to build into strings
        /// </summary>
        public static string GetColor(this int colorCode) => $"\x1b[{colorCode}m";

        /// <summary>
        /// Set console color to default color code
        /// </summary>
        public static void ResetColor() => SetColor(0);
        #endregion
    }
}