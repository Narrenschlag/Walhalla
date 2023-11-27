using System.Text;

namespace Walhalla
{
    public class BootstrapBase
    {
        public static bool LogToConsole = true;
        public static bool SafeMode = false;

        /// <summary>
        /// Starts the boot manager. This can be used to setup everything.
        /// </summary>
        public virtual void StartBootstrap()
        {
            Debug.ResetColor();

            TitleText().Log();
            OpeningText().Log();

            Debug.SetColor(2);
            "FLAGS: ".Log2();
            Debug.ResetColor();

            string? input = Console.ReadLine();
            AssignFlags(input == null || input.IsEmpty() ? "" : input);
            LogFlags();

            "\nStarting...".Log();

            try
            {
                BootLogic();

                42.SetColor();
                $"Success{0.GetColor()}\n".Log();
            }
            catch (Exception ex)
            {
                41.SetColor();
                "Boot has failed. Please contact admin.".Log();

                40.SetColor();
                ("Error Exception:\n" + ex.Message + "\n").Log();

                Debug.ResetColor();
            }
        }

        protected virtual string TitleText()
        {
            return $"-----------------------[ {92.GetColor()}Bootstrap by 🄯Narrenschlag{0.GetColor()} ]-----------------------";
        }

        protected virtual string OpeningText() =>
            $"\nWelcome to the boot manager. This is where you can configure your boot flags.\n"
            + "Are you ready to boot?\n\n"
            + "> You can enter additional flags now or just press return.\n"
            + "> (For more information about flags please contact the admin)";

        protected virtual void AssignFlags(string input)
        {
            // -noLog           prevents application from logging messages to the terminal
            // -safe            puts application into safe mode and throws the whole programm at the first error

            LogToConsole = !input.Contains("-nolog");
            SafeMode = input.Contains("-safe");
        }

        protected virtual void LogFlags()
        {
            2.SetColor();
            "\nBoot Settings ----------------".Log();
            logFlagEntries(new (string, bool)[]{
                    ("safe mode", SafeMode),
                    ("debug logs", LogToConsole)
            }).Log();
            Debug.ResetColor();
        }

        protected virtual string logFlagEntries((string key, bool value)[] entries)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < entries.Length; i++)
            {
                stringBuilder.AppendLine(logFlagFormat(entries[i].key, entries[i].value));
            }

            return stringBuilder.ToString();
        }

        protected virtual string logFlagFormat(string name, bool enabled) =>
                $"{2.GetColor()}{97.GetColor()}{name.ToUpper()}   {(enabled ? $"{92.GetColor()}ON" : $"{91.GetColor()}OFF")}";

        protected virtual void BootLogic() { }
    }
}