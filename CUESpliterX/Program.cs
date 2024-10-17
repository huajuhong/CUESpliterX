using System.Diagnostics;

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
        public const string GitHubAccount = "huajuhong";
        public const string GitHubRepository = "CUESpliterX";
        public const string GitHubRepositoryUrl = $"https://www.github.com/{GitHubAccount}/{GitHubRepository}"; // ��ǰ�汾��

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
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
            // �����²�����������
            Updater.CheckForUpdates(CurrentVersion, GitHubAccount, GitHubRepository).GetAwaiter().GetResult();
            Application.Run(new MainForm());
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
        private static void LogException(Exception ex)
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