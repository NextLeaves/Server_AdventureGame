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

using System.Threading;

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
        private Thread th;

        public MainWindow()
        {
            InitializeComponent();
            _server = Server._instance;
        }

        private void startup_btn_Click(object sender, RoutedEventArgs e)
        {
            _server.Startup("192.168.1.105", 1234);
        }

        private void clearUserTable_Click(object sender, RoutedEventArgs e)
        {

            th = new Thread(ClearUserTable);
            th.Start();

        }

        private void ClearUserTable()
        {
            if (DataManager.GetSingleton().ClearUsersTable())
                MessageBox.Show("Clear Successful.");
            else MessageBox.Show("Clear fail.");
        }

        private void clearPlayerTable_Click(object sender, RoutedEventArgs e)
        {

            th = new Thread(ClearPlayerTable);
            th.Start();

        }

        private void ClearPlayerTable()
        {
            if (DataManager.GetSingleton().ClearPlayersTable())
                MessageBox.Show("Clear Successful.");
            else MessageBox.Show("Clear fail.");
        }

        private void clearAllTables_Click(object sender, RoutedEventArgs e)
        {

            th = new Thread(ClearAllTables);
            th.Start();

        }

        private void ClearAllTables()
        {
            if (DataManager.GetSingleton().ClearAllTables())
                MessageBox.Show("Clear Successful.");
            else MessageBox.Show("Clear fail.");
        }

        private void print_btn_Click(object sender, RoutedEventArgs e)
        {
            info_txtbox.Text = "";
            info_txtbox.Text = Server._instance.PrintInformation();
            info_txtbox.Text += "\n";
            info_txtbox.Text += Sys.sb_Log.ToString();
        }
    }
}
