using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CUESpliterX
{
    //��ʽ����ά��������־�����Ҹ���Ŀ��ѭ ���廯�汾��
    //Added(����) ����ӵĹ��ܡ�
    //Changed(���) �����й��ܵı����
    //Deprecated(����) �Ѿ�������ʹ�ã������Ƴ��Ĺ��ܡ�
    //Removed(�Ƴ�) �Ѿ��Ƴ��Ĺ��ܡ�
    //Fixed(�޸�) �� bug ���޸���
    //Security(��ȫ) �԰�ȫ�ԵĸĽ���

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
            // ����ȫ���쳣�������
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // ���� WinForms Ӧ��
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            ApplicationConfiguration.Initialize();
            if (!Directory.Exists("log"))
            {
                Directory.CreateDirectory("log");
            }

            // ����Դ�ļ�����ȡ���³��򲢱���
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
                    MessageBox.Show("δ�ҵ����³���");
                    return;
                }

                // ����Դ�����Ƶ�ָ��·��
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
                LogMessage("���³�����ȡ�ɹ���·��Ϊ��" + path);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        // UI �߳��е�δ�����쳣
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException("UI�߳�", e.Exception);
        }

        // �� UI �߳��е�δ�����쳣���� Task����̨�̣߳�
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            HandleException("��UI�߳�", ex);
        }

        // �Զ�����쳣�����߼�
        private static void HandleException(string title, Exception ex)
        {
            string message = $"�����˴���: {ex.Message}\n����ϵ֧����Ա��";
            // ���ڴ˼�¼��־��ִ�����������߼�
            LogException(ex);

            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // �򵥵���־��¼����
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