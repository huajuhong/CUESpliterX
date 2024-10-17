using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CUESpliterX
{
    public partial class MainForm : Form
    {
        private static readonly Encoding gb2312 = Encoding.GetEncoding("GB2312");

        public MainForm()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // ���ô����С
            this.Size = new System.Drawing.Size(800, 500); // ���ô����ʼ��С

            // �̶������С
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // �̶��߿���ʽ
            this.MaximizeBox = false; // ������󻯰�ť

            // ��ѡ��������С������С��ͬ
            this.MinimumSize = this.Size; // ������С�ߴ�
            this.MaximumSize = this.Size; // �������ߴ�
            label7.Text = Program.CurrentVersion;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string cueFilePath = textBox1.Text.Trim();
            string outputDirectory = textBox2.Text.Trim();
            if (!File.Exists(cueFilePath))
            {
                MessageBox.Show("CUE�ļ�������");
                return;
            }
            if (string.IsNullOrEmpty(outputDirectory))
            {
                MessageBox.Show("�������ѡ�����·��");
                return;
            }
            // ����CUE�ļ�
            AlbumInfo album = GetAlbumFormCueFile(cueFilePath);
            if (!File.Exists(album.File))
                throw new FileNotFoundException($"δ�ҵ�����Դ�ļ���{album.File}");

            string performer = textBox3.Text.Trim();
            string title = textBox4.Text.Trim();
            if (!string.IsNullOrEmpty(performer)) album.Performer = performer;
            if (!string.IsNullOrEmpty(title)) album.Title = title;

            // ���·��
            outputDirectory = Path.Combine(outputDirectory, $"{album.Performer} - {album.Title}");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // �ָ���Ƶ�ļ������Ԫ����
            for (int i = 0; i < album.Tracks.Count; i++)
            {
                var track = album.Tracks[i];
                if (i + 1 < album.Tracks.Count)
                {
                    track.NextStartTime = album.Tracks[i + 1].StartTime;
                }
                else
                {
                    track.NextStartTime = album.TotalTime;
                }
                string cleanedPath = ReplaceInvalidFileChars(track.Title);
                string outputFile = Path.Combine(outputDirectory, $"{track.Index:D2} - {cleanedPath}{Path.GetExtension(album.File)}");
                bool overridden = false;
                if (File.Exists(outputFile))
                {
                    if (checkBox1.Checked)
                    {
                        File.Delete(outputFile);
                        overridden = true;
                    }
                    else
                    {
                        WriteTrackLog(track, "�Ѵ��ڣ�����");
                        continue;
                    }
                }
                await CutAudioSegmentAsync(album.File, track.StartTime!.Value, track.NextStartTime!.Value, outputFile);
                WriteTrackLog(track, "�����" + (overridden ? "������" : ""));
                AddMetadata(outputFile, track.Title, album.Performer, album.Title);
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            // ��ȡ��ק���ļ�·��
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                // ����һ���ļ���·����ʾ�� TextBox ��
                textBox1.Text = files[0];
                LoadAlbum(textBox1.Text);
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            // �����ק�������Ƿ����ļ�
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // ������ļ�������Ч��Ϊ Copy
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                // ��������ļ�������Ч��Ϊ None
                e.Effect = DragDropEffects.None;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // ���������� FolderBrowserDialog
            using FolderBrowserDialog folderDialog = new();
            folderDialog.Description = "��ѡ��һ���ļ���";
            folderDialog.ShowNewFolderButton = true; // �����½��ļ���

            // ��ʾ�Ի��򲢴���ѡ��Ľ��
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderDialog.SelectedPath; // ��ʾѡ����ļ���·��
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // ���������� OpenFileDialog
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Title = "��ѡ��һ�� CUE �ļ�";
                fileDialog.Filter = "CUE �ļ� (*.cue)|*.cue"; // ���������ɸ�����Ҫ�޸�

                // ��ʾ�Ի��򲢴���ѡ��Ľ��
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = fileDialog.FileName; // ��ʾѡ����ļ�·��
                    LoadAlbum(textBox1.Text);
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            Process.Start("explorer.exe", Program.GitHubRepositoryUrl);
        }

        private void WriteTrackLog(TrackInfo track, string content)
        {
            AppendTextSafe(textBox5, $"{track.Index} {track.Title} {track.Performer} {track.StartTime} {content}");
        }

        private static AlbumInfo GetAlbumFormCueFile(string cueFilePath, string newPerformer = "", string newTitle = "")
        {
            AlbumInfo album = new() { Tracks = [] };
            var lines = File.ReadLines(cueFilePath, gb2312).ToList();

            // �������ڽ�������ļ�����
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("FILE"))
                {
                    album.File = ExtractFileName(line, cueFilePath);
                }
                else if (line.StartsWith("PERFORMER"))
                {
                    album.Performer = ExtractQuotedValue(line);
                }
                else if (line.StartsWith("TITLE"))
                {
                    album.Title = ExtractQuotedValue(line);
                }
                else if (line.StartsWith("TRACK"))
                {
                    var track = ParseTrack(lines, ref i);
                    album.Tracks.Add(track);
                }
            }

            album.TotalTime = GetAudioDuration(album.File);
            if (!string.IsNullOrEmpty(newPerformer)) album.Performer = newPerformer;
            if (!string.IsNullOrEmpty(newTitle)) album.Title = newTitle;

            return album;
        }

        // ��ȡ��Ƶ�ļ���
        private static string ExtractFileName(string line, string cueFilePath)
        {
            var parts = line.Split('"');
            return parts.Length > 1 ? Path.Combine(Path.GetDirectoryName(cueFilePath)!, parts[1].Trim()) : string.Empty;
        }

        // ��ȡ�����ڵ��ַ���
        private static string ExtractQuotedValue(string line)
        {
            var parts = line.Split('"');
            return parts.Length > 1 ? parts[1].Trim() : string.Empty;
        }

        // ��������
        private static TrackInfo ParseTrack(List<string> lines, ref int index)
        {
            int length = lines.Count;
            string line = lines[index].Trim();
            var parts = line.Split(' ');
            int.TryParse(parts[1], out int trackNumber);
            TrackInfo track = new TrackInfo { Index = trackNumber };
            bool read = false;
            index++;
            while (read == false)
            {
                line = lines[index].Trim();
                // �������
                if (index < length && line.StartsWith("TITLE"))
                {
                    track.Title = ExtractQuotedValue(line);
                }
                // ����������
                else if (index < length && line.StartsWith("PERFORMER"))
                {
                    track.Performer = ExtractQuotedValue(line);
                }
                // ������ʼʱ��
                else if (index < length && line.StartsWith("INDEX 01"))
                {
                    track.StartTime = ExtractTimeFromIndex(line);
                }
                if (string.IsNullOrEmpty(track.Title) == false && track.StartTime != null)
                {
                    read = true;
                }
                else
                {
                    index++;
                }
            }
            return track;
        }

        // �� INDEX ����ȡʱ��
        private static TimeSpan ExtractTimeFromIndex(string line)
        {
            string[] times = line[8..].Trim().Split(':');
            int min = int.TryParse(times[0], out min) ? min : 0;
            int sec = int.TryParse(times[1], out sec) ? sec : 0;
            int millsec = int.TryParse(times[2], out millsec) ? millsec : 0;
            return new TimeSpan(0, 0, min, sec, millsec);
        }

        private static TimeSpan GetAudioDuration(string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-i \"{filePath}\" -show_entries format=duration -v quiet -of csv=\"p=0\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            double.TryParse(output, out double duration);
            return TimeSpan.FromSeconds(duration);
        }

        public static string ReplaceInvalidFileChars(string input, char replacement = '_')
        {
            // ��ȡ�Ƿ���·���ַ�
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // �滻�Ƿ��ַ���������������
            return new string(input.Select(c => invalidChars.Contains(c) ? replacement : c).ToArray());
        }

        private async Task CutAudioSegmentAsync(string inputFile, TimeSpan start, TimeSpan end, string outputFile)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{inputFile}\" -ss {start} -t {(end - start).TotalSeconds} -c copy \"{outputFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            // 1
            //process.OutputDataReceived += (sender, e) =>
            //{
            //    if (!string.IsNullOrEmpty(e.Data))
            //        Console.WriteLine("FFmpeg output: " + e.Data);
            //};

            //process.ErrorDataReceived += (sender, e) =>
            //{
            //    if (!string.IsNullOrEmpty(e.Data))
            //        Console.WriteLine("FFmpeg error: " + e.Data);
            //};
            //process.Start();

            //process.BeginOutputReadLine();
            //process.BeginErrorReadLine();

            //process.WaitForExit();

            //if (process.ExitCode != 0)
            //    throw new Exception("FFmpeg execution failed");

            // 2
            //process.Start();

            //var outputBuilder = new StringBuilder();
            //var errorBuilder = new StringBuilder();

            //Task.Run(() =>
            //{
            //    while (!process.StandardOutput.EndOfStream)
            //        outputBuilder.AppendLine(process.StandardOutput.ReadLine());
            //});

            //Task.Run(() =>
            //{
            //    while (!process.StandardError.EndOfStream)
            //        errorBuilder.AppendLine(process.StandardError.ReadLine());
            //});

            //process.WaitForExit();

            //if (process.ExitCode != 0)
            //    throw new Exception($"FFmpeg error: {errorBuilder}");

            //Console.WriteLine("FFmpeg output: " + outputBuilder);

            // 3
            process.Start();

            // �첽��ȡ����ʹ���
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask); // �ȴ����ж�ȡ���
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"FFmpeg error: {errorTask.Result}");

            //AppendTextSafe(textBox5, "FFmpeg output: " + outputTask.Result);
        }

        private static void AddMetadata(string filePath, string title, string artist, string album)
        {
            const string titleId = "INAM";  // Title
            const string artistId = "IART"; // Artist
            const string albumId = "IPRD";  // Album

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
            // �ƶ����ļ�ĩβ������µ� INFO ��
            stream.Seek(0, SeekOrigin.End);

            // д�� INFO ��
            WriteInfoChunk(stream, titleId, title);
            WriteInfoChunk(stream, artistId, artist);
            WriteInfoChunk(stream, albumId, album);
        }

        private static void WriteInfoChunk(Stream stream, string chunkId, string data)
        {
            byte[] idBytes = Encoding.ASCII.GetBytes(chunkId);
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);

            // д��� ID
            stream.Write(idBytes, 0, idBytes.Length);

            // д�����ݳ��ȣ������ַ����ĳ��ȣ�
            ushort dataLength = (ushort)data.Length;
            stream.Write(BitConverter.GetBytes(dataLength), 0, 2);

            // д������
            stream.Write(dataBytes, 0, dataBytes.Length);
        }

        private void LoadAlbum(string path)
        {
            textBox2.Text = Path.GetDirectoryName(path);
            var album = GetAlbumFormCueFile(path);
            if (!File.Exists(album.File))
                throw new FileNotFoundException($"δ�ҵ�����Դ�ļ���{album.File}");
            textBox3.Text = album.Performer;
            textBox4.Text = album.Title;
            AppendTextSafe(textBox5, $"{album.Title} {album.Performer} {album.TotalTime}");
            album.Tracks.ForEach(track =>
            {
                AppendTextSafe(textBox5, $"{track.Index} {track.Title} {track.Performer} {track.StartTime}");
            });
        }

        private static void AppendTextSafe(TextBox textBox, string text)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action<TextBox, string>(AppendTextSafe), text);
            }
            else
            {
                textBox.AppendText(text + Environment.NewLine); // ��ӻ��з�
            }
        }
    }

    internal class AlbumInfo
    {
        public string Performer { get; set; }
        public string Title { get; set; }
        public string File { get; set; }
        public List<TrackInfo> Tracks { get; set; }
        public TimeSpan TotalTime { get; set; }
    }

    internal class TrackInfo
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public string Performer { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? NextStartTime { get; set; }
    }
}