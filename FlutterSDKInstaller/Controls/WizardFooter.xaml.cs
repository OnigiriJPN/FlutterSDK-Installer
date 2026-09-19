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

namespace FlutterSDKInstaller.Controls
{
    /// <summary>
    /// WizardFooter.xaml の相互作用ロジック
    /// </summary>
    public partial class WizardFooter : UserControl
    {
        // メインウィンドウ側から購読するためのイベント
        public event EventHandler? BackClick;
        public event EventHandler? NextClick;
        public event EventHandler? CancelClick;
        public WizardFooter()
        {
            InitializeComponent();
        }
        public void SetButtonState(bool canBack, bool canNext, bool canCancel)
        {
            BackButton.IsEnabled = canBack;
            NextButton.IsEnabled = canNext;
            CancelButton.IsEnabled = canCancel;
        }
        public void SetNextButtonText(string text)
        {
            NextButton.Content = text;
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackClick?.Invoke(this, e);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextClick?.Invoke(this, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClick?.Invoke(this, e);
        }

        // 必要に応じてボタンの有効/無効を切り替えるメソッドがあると便利
        public void SetButtonState(bool canGoBack, bool canGoNext)
        {
            BackButton.IsEnabled = canGoBack;
            NextButton.IsEnabled = canGoNext;
        }
    }
}
