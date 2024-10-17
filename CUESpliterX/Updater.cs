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

namespace CUESpliterX
{
    public class Updater
    {
        // 检查更新的核心方法
        public static async Task<bool> CheckForUpdates(string currentVersion, string owner, string repo)
        {
            // 获取最新的版本信息
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AutoUpdaterApp");

            string url = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
            var response = await client.GetAsync(url);

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
                    return await DownloadAndUpdate(release);
                }
            }

            return false; // 无需更新
        }

        private static readonly HttpClientHandler handler = new() { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator };

        // 下载新版本并更新
        private static async Task<bool> DownloadAndUpdate(GitHubRelease release)
        {
            string downloadUrl = release.Assets[0].BrowserDownloadUrl;
            string downloadFileName = "temp.exe";
            string downloadFilePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath)!, downloadFileName);
            // 检查是否存在，存在则需下载前删除
            if (File.Exists(downloadFilePath))
            {
                File.Delete(downloadFilePath);
            }

            using var client = new HttpClient(handler);
            var response = await client.GetAsync(downloadUrl);

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("下载更新失败。", "更新错误");
                return false;
            }

            await using var fs = new FileStream(downloadFileName, FileMode.Create, FileAccess.Write);
            await response.Content.CopyToAsync(fs);

            DeleteOldAndRenameNew(Application.ExecutablePath, downloadFileName, Program.GitHubRepository + ".exe");
            return true;
        }

        public static void DeleteOldAndRenameNew(string oldExePath, string downloadFileName, string newFileName)
        {
            string batchFilePath = Path.Combine(Path.GetTempPath(), "delete_self.bat");
            //string startFilePath = Path.Combine(Path.GetDirectoryName(oldExePath)!, newFileName);
            // 创建批处理文件内容
            string batchContent = $@"
@echo off
del ""{oldExePath}"" /f /q
if exist ""{downloadFileName}"" ren ""{downloadFileName}"" ""{newFileName}""
timeout /t 1 >nul
start """" ""{newFileName}""
del ""{batchFilePath}"" /f /q
";

            // 写入批处理文件
            File.WriteAllText(batchFilePath, batchContent);

            Program.LogMessage($"oldExePath:{oldExePath}{Environment.NewLine}downloadFileName:{downloadFileName}{Environment.NewLine}startFileName:{newFileName}");

            // 启动批处理文件并退出当前程序
            Process.Start(new ProcessStartInfo
            {
                FileName = batchFilePath,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            //MessageBox.Show("已更新到最新版本，请重启应用。", "更新完成");
            //Application.Exit();
            Environment.Exit(0);
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