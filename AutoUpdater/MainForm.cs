namespace AutoUpdater
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public void ProgressChange(int value)
        {
            BeginInvoke(() =>
            {
                progressBar1.Value = value;
                Thread.Sleep(10);
            });
        }

        public void OnBeginUpdate()
        {
            BeginInvoke(Show);
        }

        public void OnEndUpdate()
        {
            BeginInvoke(Close);
        }
    }
}