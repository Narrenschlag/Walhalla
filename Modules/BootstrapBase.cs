using System.Text;

namespace Walhalla
{
    public class BootstrapBase
    {
        public static BootConfiguration? Config;

        /// <summary>
        /// Starts the boot manager. This can be used to setup everything.
        /// </summary>
        public virtual void StartBootstrap()
        {
            Debug.ResetColor();

            TitleText().Log();
            OpeningText().Log();

            // Boot Config and log
            Road p = new Road("configuration/boot.config");
            if (!p.TryRead(out Config) || Config == null)
                p.Write(new BootConfiguration());
            LogConfig();

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
            + "> You can configure your flags in './configuration/boot.config'.\n"
            + "> (For more information about flags please contact the admin)";

        protected virtual void LogConfig()
        {
            if (Config == null) return;

            2.SetColor();
            "\nBoot Settings ----------------".Log();
            logFlagEntries(new (string, bool)[]{
                    ("safe mode", Config.SafeMode),
                    ("debug logs", Config.Debug)
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

    public class BootConfiguration
    {
        public bool SafeMode;
        public bool Debug;
    }
}