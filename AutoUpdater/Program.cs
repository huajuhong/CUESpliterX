namespace AutoUpdater
{
    internal static class Program
    {
        private static readonly MainForm mainForm = new();
        private static readonly AutoUpdaterByGitHub autoUpdater = new();
        private static string currentVersion = string.Empty;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            if (args.Length > 0)
            {
                if (!Directory.Exists("log"))
                {
                    Directory.CreateDirectory("log");
                }
                currentVersion = args[0];
                autoUpdater.OnBeginUpdate += mainForm.OnBeginUpdate;
                autoUpdater.OnEndUpdate += mainForm.OnEndUpdate;
                autoUpdater.OnProgressChanged += mainForm.ProgressChange;
                mainForm.Shown += MainForm_Shown;

                Application.Run(mainForm);
            }
        }

        private static async void MainForm_Shown(object? sender, EventArgs e)
        {
            mainForm.Hide();
            await autoUpdater.CheckForUpdates(currentVersion);
            Application.Exit();
        }
    }
}