using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlutterSDKInstaller.Pages
{
    /// <summary>
    /// Step2Page.xaml の相互作用ロジック
    /// </summary>
    public partial class Step2Page : Page
    {
        public Step2Page()
        {
            InitializeComponent();
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            TxtInstallPath.Text = System.IO.Path.Combine(userProfile, "flutter");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Flutter SDKのインストール先フォルダを選択",
                InitialDirectory = TxtInstallPath.Text // 現在入力されているパスを初期位置に
            };

            // ダイアログを表示して「選択」が押されたら
            if (dialog.ShowDialog() == true)
            {
                // 選択されたフォルダのパスをテキストボックスに反映
                TxtInstallPath.Text = dialog.FolderName;
            }
        }
        private bool ValidatePath(out string errorMessage)
        {
            errorMessage = string.Empty;
            string path = TxtInstallPath.Text.Trim();

            // 1. 空欄チェック
            if (string.IsNullOrEmpty(path))
            {
                errorMessage = "インストール先のパスが入力されていません。";
                return false;
            }

            try
            {
                // 2. パスとしての構文チェック（不正文字など）
                string fullPath = System.IO.Path.GetFullPath(path);

                // 3. ドライブの存在確認
                string root = System.IO.Path.GetPathRoot(fullPath);
                if (!string.IsNullOrEmpty(root) && !System.IO.Directory.Exists(root))
                {
                    errorMessage = $"指定されたドライブ ({root}) が存在しません。";
                    return false;
                }

                // 4. 空き容量チェック（2GBに設定）
                if (!string.IsNullOrEmpty(root))
                {
                    var driveInfo = new System.IO.DriveInfo(root);
                    if (driveInfo.IsReady)
                    {
                        long requiredSpace = 2L * 1024 * 1024 * 1024; // 2GB
                        if (driveInfo.AvailableFreeSpace < requiredSpace)
                        {
                            double freeGB = (double)driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024);
                            errorMessage = $"ドライブ ({root}) の空き容量が不足しています。\n(現在値: {freeGB:F1} GB / 必要: 2.0 GB)";
                            return false;
                        }
                    }
                }

                // 5. 書き込み権限 / 管理者権限が必要かどうかのチェック
                // 既存の親フォルダを探す、またはルートから辿って実際にアクセス・作成テストできるか
                string testDir = fullPath;
                while (!System.IO.Directory.Exists(testDir))
                {
                    string parent = System.IO.Path.GetDirectoryName(testDir);
                    if (string.IsNullOrEmpty(parent) || parent == testDir) break;
                    testDir = parent;
                }

                // using を取っ払って、単にディレクトリを作成・確認する
                System.IO.Directory.CreateDirectory(testDir);
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "指定された場所への書き込み権限がありません。\nインストーラーを「管理者として実行」するか、別のフォルダを選択してください。";
                return false;
            }
            catch (Exception)
            {
                errorMessage = "パスが無効であるか、アクセスできません。";
                return false;
            }

            return true;
        }
        public bool TryGoNext()
        {
            // パスのバリデーションを実行！
            if (!ValidatePath(out string errorMessage))
            {
                // エラーがあればメッセージを出して次へ進ませない
                MessageBox.Show(errorMessage, "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false; // 進めなかった
            }

            // エラーがなければ次のステップへナビゲート！
            NavigationService.Navigate(new Pages.Step3Page(TxtInstallPath.Text.Trim()));
            return true; // 進めた！
        }
    }
}
