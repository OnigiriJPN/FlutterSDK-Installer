using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace FlutterSDKInstaller.Pages
{
    /// <summary>
    /// Step3Page.xaml の相互作用ロジック
    /// </summary>
    public partial class Step3Page : Page
    {
        private readonly string _installPath;

        public Step3Page(string installPath)
        {
            InitializeComponent();
            _installPath = installPath;

            this.Loaded += async (s, e) => await StartInstallationAsync();
        }
        private async Task StartInstallationAsync()
        {
            // 1. まず管理者権限を持っているかチェック！
            if (!IsAdministrator())
            {
                var result = MessageBox.Show(
                    "Flutter SDKのインストールには管理者権限が必要です。\n「はい」を押すとUAC（管理者昇格）プロンプトが表示されます。",
                    "管理者権限の確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    // 管理者権限で自分自身を再起動してUACを出す
                    if (RestartAsAdministrator())
                    {
                        // 昇格版が起動成功したら、今の非管理者版アプリは終了する
                        Application.Current.Shutdown();
                        return;
                    }
                }

                // ユーザーが拒否した場合や失敗した場合の処理
                MessageBox.Show("管理者権限が取得できなかったため、インストールを中止します。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ==========================================
            // 2. ここから先は「管理者権限確定」の安全地帯！
            // ==========================================
            try
            {
                string downloadUrl = "https://storage.googleapis.com/flutter_infra_release/releases/stable/windows/flutter_windows_3.19.0-stable.zip";
                string tempZipPath = Path.Combine(Path.GetTempPath(), "flutter_sdk.zip");

                // ダウンロード処理...
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            await contentStream.CopyToAsync(fileStream);
                        }
                    }
                }

                if (!Directory.Exists(_installPath))
                {
                    Directory.CreateDirectory(_installPath);
                }

                // 展開処理...
                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(tempZipPath, _installPath, overwriteFiles: true);
                });

                if (File.Exists(tempZipPath))
                {
                    File.Delete(tempZipPath);
                }

                MessageBox.Show("Flutter SDKのインストールが完了しました！", "完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"インストール中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 現在のプロセスが管理者かどうかを判定するヘルパー
        private bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        // 管理者権限で自分を再起動するヘルパー（ここでUACが出る！）
        private bool RestartAsAdministrator()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas" // ★この "runas" がWindowsにUACを出させる魔法の言葉！
                };

                Process.Start(startInfo);
                return true;
            }
            catch (Exception)
            {
                // ユーザーがUACの画面で「いいえ」を押した場合などは例外が発生する
                return false;
            }
        }
    }
}
