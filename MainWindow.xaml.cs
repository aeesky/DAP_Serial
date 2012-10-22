using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DAP_Serial.Core;
using DAP_Serial.Utilities;
using DAP_Serial.Views;

using MahApps.Metro.Controls;
using System.Text.RegularExpressions;

namespace DAP_Serial
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly Storyboard _showMessageStoryboard = null;
        public static MainWindow Instance { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = MainVM.Instance;
            this.Loaded += MainWindow_Loaded;
            _showMessageStoryboard = Resources["ShowMessageStoryboard"] as Storyboard;
            Instance = this;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HomeRadioButton.IsChecked = true;
        }

        private void Exit_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DataAcquisition_Checked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(DataAcquisitionPage.Instance);
        }

        private void Home_Chceked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(HomePage.Instance);
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            HomeRadioButton.IsChecked = false;
            DataAcquisitionRadioButton.IsChecked = false;
            RealTimeCurveRadioButton.IsChecked = false;
            MainFrame.Navigate(SettingPage.Instance);
        }

        private void RealTimeCurve_Checked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(RealtimeCurvePage.Instance);
        }

        private void Contact_Click(object sender, RoutedEventArgs e)
        {
            var websiteMatch = Regex.Match(SettingVM.Instance.WebSite,
                "((https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|])|[[a-z0-9A-Z.-]+");
            if (websiteMatch.Success)
            {
                System.Diagnostics.Process.Start(websiteMatch.Value);
            }
        }

        /// <summary>
        /// 自动消失消息提示
        /// </summary>
        /// <param name="message"></param>
        public void ShowMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                _showMessageStoryboard.Stop();
            }
            else
            {
                MessageTextBlock.Text = message;
                _showMessageStoryboard.Begin();
            }
        }
    }
}
