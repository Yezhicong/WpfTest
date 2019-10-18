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

namespace WpfTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        string username = "admin";
        string password = "admin";
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //DialogResult = false;
            this.Close();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (username == Username.Text && password == Password.Password)
            {
                Window1 myWindow= new Window1();
                myWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("用户名或密码错误!", "提示");
            }
        }
    }
}
