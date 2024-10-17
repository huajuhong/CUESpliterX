using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CUESpliterX
{
    //格式基于维护更新日志，并且该项目遵循 语义化版本。
    //Added(新增) 新添加的功能。
    //Changed(变更) 对现有功能的变更。
    //Deprecated(废弃) 已经不建议使用，即将移除的功能。
    //Removed(移除) 已经移除的功能。
    //Fixed(修复) 对 bug 的修复。
    //Security(安全) 对安全性的改进。

    internal static class Program
    {
        public const string CurrentVersion = "1.0.2";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string AutoUpdater = args[0];
                if (AutoUpdater == "AutoUpdater")
                {
                    string path = "AutoUpdater.exe";
                    var processes = Process.GetProcessesByName(AutoUpdater);
                    foreach (Process p in processes)
                    {
                        p.Kill();
                        p.WaitForExit();
                        p.Dispose();
                    }
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
            // 设置全局异常处理程序
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // 启动 WinForms 应用
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            ApplicationConfiguration.Initialize();
            if (!Directory.Exists("log"))
            {
                Directory.CreateDirectory("log");
            }

            // 从资源文件中提取更新程序并保存
            ExtractAndRunAutoUpdater(out string updaterFilePath);

            Application.Run(new MainForm());
        }

        private static void ExtractAndRunAutoUpdater(out string path)
        {
            path = string.Empty;
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string fileName = "AutoUpdater.exe";
                path = Path.Combine(currentDirectory, fileName);
                string resourceName = $"CUESpliterX.Resources.{fileName}";
                var assembly = Assembly.GetExecutingAssembly();
                if (assembly == null)
                {
                    return;
                }
                using Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
                if (resourceStream == null)
                {
                    MessageBox.Show("未找到更新程序。");
                    return;
                }

                // 将资源流复制到指定路径
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fs);
                    fs.Dispose();
                }
                var process = Process.Start(path, CurrentVersion);
                process.WaitForExit();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                LogMessage("更新程序提取成功，路径为：" + path);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
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
        public static void LogException(Exception ex)
        {
            string logPath = "log\\error.log";
            string message = $"{DateTime.Now}: {ex}\n";
            System.IO.File.AppendAllText(logPath, message);
        }

        public static void LogMessage(string content)
        {
            string logPath = "log\\message.log";
            string message = $"{DateTime.Now}: {content}\n";
            System.IO.File.AppendAllText(logPath, message);
        }
    }
}