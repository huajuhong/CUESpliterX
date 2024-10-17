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
            // 设置窗体大小
            this.Size = new System.Drawing.Size(800, 500); // 设置窗体初始大小

            // 固定窗体大小
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // 固定边框样式
            this.MaximizeBox = false; // 禁用最大化按钮

            // 可选：设置最小和最大大小相同
            this.MinimumSize = this.Size; // 设置最小尺寸
            this.MaximumSize = this.Size; // 设置最大尺寸
            label7.Text = Program.CurrentVersion;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string cueFilePath = textBox1.Text.Trim();
            string outputDirectory = textBox2.Text.Trim();
            if (!File.Exists(cueFilePath))
            {
                MessageBox.Show("CUE文件不存在");
                return;
            }
            if (string.IsNullOrEmpty(outputDirectory))
            {
                MessageBox.Show("请输入或选择输出路径");
                return;
            }
            // 解析CUE文件
            AlbumInfo album = GetAlbumFormCueFile(cueFilePath);
            if (!File.Exists(album.File))
                throw new FileNotFoundException($"未找到整轨源文件：{album.File}");

            string performer = textBox3.Text.Trim();
            string title = textBox4.Text.Trim();
            if (!string.IsNullOrEmpty(performer)) album.Performer = performer;
            if (!string.IsNullOrEmpty(title)) album.Title = title;

            // 输出路径
            outputDirectory = Path.Combine(outputDirectory, $"{album.Performer} - {album.Title}");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // 分割音频文件并添加元数据
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
                        WriteTrackLog(track, "已存在，跳过");
                        continue;
                    }
                }
                await CutAudioSegmentAsync(album.File, track.StartTime!.Value, track.NextStartTime!.Value, outputFile);
                WriteTrackLog(track, "已完成" + (overridden ? "，覆盖" : ""));
                AddMetadata(outputFile, track.Title, album.Performer, album.Title);
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            // 获取拖拽的文件路径
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                // 将第一个文件的路径显示在 TextBox 中
                textBox1.Text = files[0];
                LoadAlbum(textBox1.Text);
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            // 检查拖拽的内容是否是文件
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // 如果是文件，设置效果为 Copy
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                // 如果不是文件，设置效果为 None
                e.Effect = DragDropEffects.None;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 创建并配置 FolderBrowserDialog
            using FolderBrowserDialog folderDialog = new();
            folderDialog.Description = "请选择一个文件夹";
            folderDialog.ShowNewFolderButton = true; // 允许新建文件夹

            // 显示对话框并处理选择的结果
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderDialog.SelectedPath; // 显示选择的文件夹路径
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 创建并配置 OpenFileDialog
            using (OpenFileDialog fileDialog = new OpenFileDialog())
            {
                fileDialog.Title = "请选择一个 CUE 文件";
                fileDialog.Filter = "CUE 文件 (*.cue)|*.cue"; // 过滤器，可根据需要修改

                // 显示对话框并处理选择的结果
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = fileDialog.FileName; // 显示选择的文件路径
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

            // 设置用于解析音轨的计数器
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

        // 提取音频文件名
        private static string ExtractFileName(string line, string cueFilePath)
        {
            var parts = line.Split('"');
            return parts.Length > 1 ? Path.Combine(Path.GetDirectoryName(cueFilePath)!, parts[1].Trim()) : string.Empty;
        }

        // 提取引号内的字符串
        private static string ExtractQuotedValue(string line)
        {
            var parts = line.Split('"');
            return parts.Length > 1 ? parts[1].Trim() : string.Empty;
        }

        // 解析音轨
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
                // 处理标题
                if (index < length && line.StartsWith("TITLE"))
                {
                    track.Title = ExtractQuotedValue(line);
                }
                // 处理艺术家
                else if (index < length && line.StartsWith("PERFORMER"))
                {
                    track.Performer = ExtractQuotedValue(line);
                }
                // 处理起始时间
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

        // 从 INDEX 行提取时间
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
            // 获取非法的路径字符
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // 替换非法字符，保留其他部分
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

            // 异步读取输出和错误
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask); // 等待所有读取完成
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
            // 移动到文件末尾以添加新的 INFO 块
            stream.Seek(0, SeekOrigin.End);

            // 写入 INFO 块
            WriteInfoChunk(stream, titleId, title);
            WriteInfoChunk(stream, artistId, artist);
            WriteInfoChunk(stream, albumId, album);
        }

        private static void WriteInfoChunk(Stream stream, string chunkId, string data)
        {
            byte[] idBytes = Encoding.ASCII.GetBytes(chunkId);
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);

            // 写入块 ID
            stream.Write(idBytes, 0, idBytes.Length);

            // 写入数据长度（包括字符串的长度）
            ushort dataLength = (ushort)data.Length;
            stream.Write(BitConverter.GetBytes(dataLength), 0, 2);

            // 写入数据
            stream.Write(dataBytes, 0, dataBytes.Length);
        }

        private void LoadAlbum(string path)
        {
            textBox2.Text = Path.GetDirectoryName(path);
            var album = GetAlbumFormCueFile(path);
            if (!File.Exists(album.File))
                throw new FileNotFoundException($"未找到整轨源文件：{album.File}");
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
                textBox.AppendText(text + Environment.NewLine); // 添加换行符
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