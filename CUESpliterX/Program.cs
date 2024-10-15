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
            // ����ȫ���쳣�������
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // ���� WinForms Ӧ��
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            ApplicationConfiguration.Initialize();
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
            string logPath = "error.log";
            string message = $"{DateTime.Now}: {ex}\n";
            System.IO.File.AppendAllText(logPath, message);
        }
    }
}