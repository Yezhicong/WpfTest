using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfTest
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        private bool hexShow = false;
        private bool hexSend = false;
        Socket socket;
        Thread threadReceive;
        private ReceiveMsgCallBack receiveCallBack;
        private delegate string ConnectSocketDelegate(IPEndPoint ipep, Socket sock);
        private delegate void ReceiveMsgCallBack(string strMsg);

        public Window1()
        {
            InitializeComponent();
        }

        private void ConnectBt_Click(object sender, RoutedEventArgs e)
        {
            if(ConnectBt.Content.Equals("开始连接"))
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ip = IPAddress.Parse(textBoxIP.Text);
                    IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(textBoxPort.Text));

                    ConnectSocketDelegate connect = ConnectSocket;
                    IAsyncResult asyncResult = connect.BeginInvoke(point, socket, null, null);

                    bool connectSuccess = asyncResult.AsyncWaitHandle.WaitOne(1000, false);
                    if (!connectSuccess)
                    {
                        MessageBox.Show("连接超时", "提示");
                        return;
                    }

                    string exmessage = connect.EndInvoke(asyncResult);
                    if (!string.IsNullOrEmpty(exmessage))
                    {
                        MessageBox.Show(string.Format("失败！错误信息：{0}", exmessage));
                    }
                    else
                    {
                        ConnectBt.Content = "断开连接";
                        receiveCallBack = new ReceiveMsgCallBack(SetValue);
                        threadReceive = new Thread(new ThreadStart(Receive));
                        threadReceive.IsBackground = true;
                        threadReceive.Start();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("连接失败!", "提示");
                }
            }
            else
            {
                ConnectBt.Content = "开始连接";
                socket.Close();
                threadReceive.Abort();
            }
        }

        private string ConnectSocket(IPEndPoint ipep, Socket sock)
        {
            string exmessage = "";
            try
            {
                sock.Connect(ipep);
            }
            catch (System.Exception ex)
            {
                exmessage = ex.Message;
            }
            finally
            {
            }

            return exmessage;
        }

        private void ClearBt_Click(object sender, RoutedEventArgs e)
        {
            recvText.Text = "";
            sendText.Text = "";
        }

        private void SendBt_Click(object sender, RoutedEventArgs e)
        {
            if (hexSend == false)
            {
                string strMsg = this.sendText.Text.Trim();
                byte[] buffer = new byte[2048];
                buffer = Encoding.Default.GetBytes(strMsg);
                socket.Send(buffer);
            }
            if (hexSend == true)
            {
                string strMsg = this.sendText.Text.Trim();
                byte[] buffer = new byte[strMsg.Length / 2];
                buffer = HexStringToByteArray(strMsg);
                socket.Send(buffer);
            }
        }

        private void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[2048];
                    int r = socket.Receive(buffer);

                    if (r == 0)
                    {
                        break;
                    }
                    else
                    {
                        if (hexShow == false)
                        {
                            string str = Encoding.Default.GetString(buffer, 0, r);
                            this.recvText.Dispatcher.Invoke(receiveCallBack, str);
                        }
                        if (hexShow == true)
                        {
                            string str = Encoding.Default.GetString(buffer, 0, r);
                            string hexString = StringToHexString(buffer, r);
                            this.recvText.Dispatcher.Invoke(receiveCallBack, hexString);
                        }
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("接收消息失败");
            }
        }

        private void SetValue(string strValue)
        {
            this.recvText.AppendText(strValue);
            this.recvText.ScrollToEnd();
        }

        private void CbShowHex_Checked(object sender, RoutedEventArgs e)
        {
            hexShow = true;
        }

        private void CbShowHex_UnChecked(object sender, RoutedEventArgs e)
        {
            hexShow = false;
        }

        private void CbSendHex_Checked(object sender, RoutedEventArgs e)
        {
            hexSend = true;
        }

        private void CbSendHex_UnChecked(object sender, RoutedEventArgs e)
        {
            hexSend = false;
        }

        public static byte[] HexStringToByteArray(string s)//16进制转字节
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }


        public static string StringToHexString(byte[] buff, int count)
        {
            byte[] buffer = buff;

            StringBuilder strBuider = new StringBuilder();
            for (int index = 0; index < count; index++)
            {
                strBuider.Append(((int)buffer[index]).ToString("x2"));
                strBuider.Append(" ");
            }
            return strBuider.ToString().Trim() + "\r\n";
        }
    }
}
