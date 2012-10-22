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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DAP_Serial.Core;

namespace DAP_Serial.Views
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : Page
    {
        private static readonly SettingPage _instance = new SettingPage();

        public static SettingPage Instance { get { return _instance; } }

        private SettingPage()
        {
            InitializeComponent();
            this.DataContext = SettingVM.Instance;
            this.Loaded += SettingPage_Loaded;
            this.Unloaded += SettingPage_Unloaded;
        }

        private void SettingPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void SettingPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (
                SettingVM.Instance.SaveCommand.CanExecute(null)
                && MessageBox.Show(MainWindow.Instance,
                "设置已更改，是否确认保存？",
                "提示",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SettingVM.Instance.SaveCommand.Execute(null);
            }
            else
            {
                SettingVM.Instance.Cancel();
            }
        }
    }
}
