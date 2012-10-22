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
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        private readonly static HomePage _instance = new HomePage();
        public static HomePage Instance { get { return _instance; } }
        private HomePage()
        {
            InitializeComponent();

            this.Loaded += HomePage_Loaded;
            this.Unloaded += HomePage_Unloaded;

            this.DataContext = MainVM.Instance;
        }

        private void HomePage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void HomePage_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
