namespace CUESpliterX
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // 设置全局异常处理程序
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // 启动 WinForms 应用
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        // UI 线程中的未处理异常
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException("UI线程", e.Exception);
        }

        // 非 UI 线程中的未处理异常（如 Task、后台线程）
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            HandleException("非UI线程", ex);
        }

        // 自定义的异常处理逻辑
        private static void HandleException(string title, Exception ex)
        {
            string message = $"发生了错误: {ex.Message}\n请联系支持人员。";
            // 可在此记录日志或执行其他处理逻辑
            LogException(ex);

            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // 简单的日志记录函数
        private static void LogException(Exception ex)
        {
            string logPath = "error.log";
            string message = $"{DateTime.Now}: {ex}\n";
            System.IO.File.AppendAllText(logPath, message);
        }
    }
}