using FlutterSDKInstaller.Pages;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace FlutterSDKInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // DWMのウィンドウ属性設定用API
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        // Windows 11向けのダークモード指定定数 (一部の古いWindows 10の場合は 19 の場合もあります)
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private bool _isDarkMode = false;

        public MainWindow()
        {
            InitializeComponent();
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }

        private void ShowAboutWindow()
        {
            var about = new AboutWindow();
            about.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WizardFrame.Navigate(new Pages.Step1Page());
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAboutWindow();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
            if(e.ClickCount == 2)
            {
                if(this.WindowState == WindowState.Normal)
                {
                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    this.WindowState = WindowState.Normal;
                }
            }
        }

        private void DarkThemeChangeButton_Click(object sender, RoutedEventArgs e)
        {
            _isDarkMode = !_isDarkMode;
            ApplyTheme(sender, e);
        }
        private void ApplyTheme(object sender, RoutedEventArgs e)
        {
            IntPtr hWnd = new WindowInteropHelper(this).Handle;
            if (hWnd != IntPtr.Zero)
            {
                int useImmersiveDarkMode = _isDarkMode ? 1 : 0;
                DwmSetWindowAttribute(hWnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
            }

            // senderからボタンを取得してテキストを切り替え
            if (sender is Button btn)
            {
                btn.Content = _isDarkMode ? "ライトテーマにする" : "ダークテーマにする";
            }
        }
        private void Footer_BackClick(object sender, EventArgs e)
        {
            if (WizardFrame.CanGoBack)
            {
                WizardFrame.GoBack();
            }
        }

        private void Footer_NextClick(object sender, EventArgs e)
        {
            if (WizardFrame.Content is Step1Page)
            {
                WizardFrame.Navigate(new Step2Page());
            }
            else if (WizardFrame.Content is Step2Page step2)
            {
                step2.TryGoNext();
            }
            else if (WizardFrame.Content is Step3Page)
            {
                WizardFrame.Navigate(new Step4Page());
            }
            else if (WizardFrame.Content is Step4Page)
            {
                // 完了画面での次へ（または完了ボタン）でアプリを閉じるなど
                this.Close();
            }
        }

        private void Footer_CancelClick(object sender, EventArgs e)
        {
            var Result = MessageBox.Show(this, "キャンセルしますか？", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(Result == MessageBoxResult.Yes)
            {
                this.Close();
            }
            else
            {
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Step4（完了画面など）では確認なしで閉じたい場合
            if (WizardFrame.Content is Pages.Step4Page)
            {
                return;
            }

            var Result = MessageBox.Show(this, "キャンセルしますか？", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (Result == MessageBoxResult.Yes)
            {
                this.Close();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void WizardFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // 今どのページが表示されているかでボタンの有効/無効を切り替える！
            if (WizardFrame.Content is Pages.Step1Page)
            {
                // Step1: 戻る不可、次へ可能
                MyFooter.SetButtonState(canBack: false, canNext: true, canCancel: true);
                MyFooter.SetNextButtonText("次へ >");
            }
            else if (WizardFrame.Content is Step2Page step2) // 👈 現在のStep2Pageのインスタンスを取得！
            {
                step2.TryGoNext();
            }
            else if (WizardFrame.Content is Pages.Step3Page)
            {
                // Step3: 例えば規約同意前は次へ不可、など
                MyFooter.SetButtonState(canBack: true, canNext: false, canCancel: true);
                MyFooter.SetNextButtonText("インストール");
            }
            else if (WizardFrame.Content is Pages.Step4Page)
            {
                // 完了画面: 戻る不可、次へ（完了）のみ
                MyFooter.SetButtonState(canBack: false, canNext: true, canCancel: false);
                MyFooter.SetNextButtonText("完了");
            }
        }

        private void WizardFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            MessageBox.Show(this, $"セットアップ中にエラーが発生しました。 \n理由: {e.Exception.Message} \nエラーコード: {e.Exception.HResult:X8}",this.Title);
            return;
        }
        // 「次へ」ボタンが押されたときなどの処理
        
    }
}