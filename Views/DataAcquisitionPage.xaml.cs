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
    /// DataAcquisitionPage.xaml 的交互逻辑
    /// </summary>
    public partial class DataAcquisitionPage : Page
    {
        private readonly static DataAcquisitionPage _instance = new DataAcquisitionPage();
        public static DataAcquisitionPage Instance { get { return _instance; } }
        private DataAcquisitionPage()
        {
            InitializeComponent();

            this.Loaded += DataAcquisitionPage_Loaded;
            this.Unloaded += DataAcquisitionPage_Unloaded;

            this.DataContext = DataAcquisitionVM.Instance;
        }

        private void DataAcquisitionPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DataAcquisitionPage_Unloaded(object sender, RoutedEventArgs e)
        {
            DataAcquisitionVM.Instance.Disconnect();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            OleDbHelper.WriteToAccess("222", "dddd");
        }
    }
}
