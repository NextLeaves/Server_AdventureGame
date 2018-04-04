using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Server_AdventureGame_wpf.Core;
using Server_AdventureGame_wpf.Data;

namespace Server_AdventureGame_wpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server _server;

        public MainWindow()
        {
            _server = Server._instance;
            InitializeComponent();
        }

        private void startup_btn_Click(object sender, RoutedEventArgs e)
        {
            _server.Startup("192.168.1.105", 1234);
        }

        private void clearUserTable_Click(object sender, RoutedEventArgs e)
        {
            if (DataManager.GetSingleton().ClearUsersTable())
                MessageBox.Show("Clear Successful.");
            else MessageBox.Show("Clear fail.");
        }

        private void clearPlayerTable_Click(object sender, RoutedEventArgs e)
        {
            if (DataManager.GetSingleton().ClearPlayersTable())
                MessageBox.Show("Clear Successful.");
            else MessageBox.Show("Clear fail.");
        }

        private void clearAllTables_Click(object sender, RoutedEventArgs e)
        {
            if (DataManager.GetSingleton().ClearAllTables())
                MessageBox.Show("Clear Successful.");
            else MessageBox.Show("Clear fail.");
        }

        private void print_btn_Click(object sender, RoutedEventArgs e)
        {
            info_txtbox.Text = Server._instance.PrintInformation();
        }
    }
}
