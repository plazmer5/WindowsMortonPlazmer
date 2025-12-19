using Microsoft.Web.WebView2.Core;
using MortonPlazmer.Properties;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MortonPlazmer
{
    public partial class MainWindow : Window
    {
        private const string Url = "https://mortonplazmer.wixsite.com/plazmerdi/";

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            PreviewKeyDown += OnPreviewKeyDown;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Показываем диалог в UI-потоке синхронно
            await Dispatcher.InvokeAsync(async () =>
            {
                await CheckAgeAsync();
            });
        }


        #region Age Check
        private async Task CheckAgeAsync()
        {
            if (!Settings.Default.IsAdult)
                await AskAgeAsync();

            if (!Settings.Default.IsAdult)
            {
                Close();
                return;
            }

            WebView.Visibility = Visibility.Visible;
            await WebView.EnsureCoreWebView2Async();
            ConfigureWebView(WebView.CoreWebView2);
            WebView.Source = new Uri(Url);
        }

        private Task AskAgeAsync()
        {
            // Показываем модальный диалог
            var dialog = new CustomDialog("Возрастное ограничение 21+", "Вам уже есть 21+ лет?");
            dialog.Owner = this;  // Привязка к MainWindow
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();  // модально

            bool isAdult = dialog.Result;
            Properties.Settings.Default.IsAdult = isAdult;
            Properties.Settings.Default.Save();

            return Task.CompletedTask;
        }

        #endregion

        #region WebView2
        private void ConfigureWebView(CoreWebView2 core)
        {
            core.NewWindowRequested += (sender, e) =>
            {
                WebView.CoreWebView2.Navigate(e.Uri);
                e.Handled = true;
            };

            core.DownloadStarting += OnDownloadStarting;
        }

        private void OnDownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                e.Handled = true;
                string fileName = Path.GetFileName(e.ResultFilePath ?? e.DownloadOperation.Uri);

                var dialog = new CustomDialog("Скачать файл", $"Скачать файл:\n{fileName}?");
                dialog.Owner = this;
                dialog.ShowDialog();

                if (!dialog.Result)
                {
                    e.Cancel = true;
                    return;
                }

                var saveDialog = new SaveFileDialog { FileName = fileName, Filter = "Все файлы|*.*" };
                if (saveDialog.ShowDialog() != true)
                {
                    e.Cancel = true;
                    return;
                }

                e.ResultFilePath = saveDialog.FileName;
                e.Handled = false;
            }
            finally
            {
                deferral.Complete();
            }
        }
        #endregion

        #region Keyboard Navigation
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (WebView?.CanGoBack != true)
                return;

            if (e.Key == Key.Back || (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.Left))
            {
                WebView.GoBack();
                e.Handled = true;
            }
        }
        #endregion
    }
}
