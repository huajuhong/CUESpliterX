using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoUpdater
{
    public class AutoUpdaterByGitHub
    {
        private const string GitHubAccount = "huajuhong";
        private const string GitHubRepository = "CUESpliterX";
        private const string GitHubRepositoryReleaseLatestUrl = $"https://api.github.com/repos/{GitHubAccount}/{GitHubRepository}/releases/latest";

        public event Action? OnBeginUpdate;

        public event Action? OnEndUpdate;

        public event Action<int>? OnProgressChanged;

        // 检查更新的核心方法
        public async Task<bool> CheckForUpdates(string currentVersion)
        {
            // 获取最新的版本信息
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AutoUpdaterApp");

            var response = await client.GetAsync(GitHubRepositoryReleaseLatestUrl).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("无法检查更新，请稍后重试。", "更新错误");
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var release = JsonSerializer.Deserialize<GitHubRelease>(json);

            // 检查版本号是否不同
            if (release != null && release.TagName != currentVersion)
            {
                var result = MessageBox.Show($"检测到新版本{release.TagName}，是否下载并更新？", "更新提示", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    OnBeginUpdate?.Invoke();
                    bool value = await DownloadAndUpdate(release);
                    OnEndUpdate?.Invoke();
                    return value;
                }
            }
            return false; // 无需更新
        }

        private static readonly HttpClientHandler handler = new() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator };

        // 下载新版本并更新
        private async Task<bool> DownloadAndUpdate(GitHubRelease release)
        {
            string downloadUrl = release.Assets[0].BrowserDownloadUrl;
            string downloadFileName = $"{Guid.NewGuid():N}.exe";

            using var client = new HttpClient(handler);
            HttpResponseMessage response = await client.GetAsync(downloadUrl).ConfigureAwait(false);
            //response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("下载更新失败。", "更新错误");
                return false;
            }

            //await using var fs = new FileStream(downloadFileName, FileMode.Create, FileAccess.Write);
            //await response.Content.CopyToAsync(fs);
            //fs.Dispose();

            long? totalBytes = response.Content.Headers.ContentLength;  // 获取文件总大小（字节数）
            if (!totalBytes.HasValue)
            {
                MessageBox.Show("无法获取文件大小。", "更新错误");
                return false;
            }
            await using var contentStream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = new FileStream(downloadFileName, FileMode.Create, FileAccess.Write, FileShare.None);

            byte[] buffer = new byte[8192];  // 每次读取 8KB
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalRead += bytesRead;

                // 更新进度条
                int progress = (int)((totalRead * 100) / totalBytes.Value);
                OnProgressChanged?.Invoke(progress);
            }
            fileStream.Dispose();
            DeleteOldAndAddNew(downloadFileName);
            return true;
        }

        private static void DeleteOldAndAddNew(string downloadFileName)
        {
            //结束 CUESpliterX 程序进程，并删除
            var processes = Process.GetProcessesByName("CUESpliterX");
            List<string> exePaths = [];
            if (processes.Length != 0)
            {
                foreach (Process p in processes)
                {
                    try
                    {
                        // 获取当前程序的路径，删除程序
                        if (p.MainModule != null)
                        {
                            string exePath = p.MainModule.FileName;
                            exePaths.Add(exePath);
                            p.Kill();
                            p.WaitForExit();
                            p.Dispose();
                            //File.Delete(exePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                        return;
                    }
                }
            }
            exePaths.ForEach(File.Delete);
            //重命名更新后得程序为CUESpliterX.exe，重启
            string destFileName = "CUESpliterX.exe";
            File.Move(downloadFileName, destFileName, true);
            Process.Start(destFileName, "AutoUpdater");
        }

        // 简单的日志记录函数
        public static void LogException(Exception ex)
        {
            string logPath = "log\\au-error.log";
            string message = $"{DateTime.Now}: {ex}\n";
            System.IO.File.AppendAllText(logPath, message);
        }

        public static void LogMessage(string content)
        {
            string logPath = "log\\au-message.log";
            string message = $"{DateTime.Now}: {content}\n";
            System.IO.File.AppendAllText(logPath, message);
        }
    }

    // GitHub Release 数据模型
    public class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }

        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; set; }
    }

    public class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }
}