using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OhMyDanmaku
{
    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region global
        Random ra = new Random();
        Socket networkSocket;
        Thread networkThread;
        Audit auditWindow = null;
        wpfDanmakulib lib;

        public double _SCREEN_WIDTH = SystemParameters.PrimaryScreenWidth;
        public double _SCREEN_HEIGHT = SystemParameters.PrimaryScreenHeight;
        #endregion

        public MainWindow()
        {
            MessageBox.Show("进入下一步之前,请打开\"自动隐藏任务栏\"以使弹幕区域定位正常!\r\n\r\nBefore next step, Please enable \"Auto hide taskbar\" to make danmaku area get right position!", "Before Initialization...");

            InitializeComponent();

            loadDefaultConfig(); //Load Default config to GlobalVariable

            OhMyDanmaku_Init(); //start init process with default config
        }

        #region Network

        private void networkListenLoop(int _port, bool audit)
        {
            Console.WriteLine("Network Thread is Starting.. Listen on:" + GlobalVariable._user_com_port.ToString());

            networkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Loopback, _port);

            byte[] buffer = new byte[102400];
            int dataLength;
            networkSocket.Bind(ip);

            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)client;

            while (true)
            {
                try
                {
                    dataLength = networkSocket.ReceiveFrom(buffer, ref remote);

                    Thread temp = new Thread(() => { msgHandler(buffer, dataLength, audit); });
                    temp.IsBackground = true;
                    temp.Start();

                    remote = (EndPoint)client;
                }
                catch (ThreadAbortException)
                {
                    Console.WriteLine("Communication Thread is shutting down..");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unknown Exception: \r\n" + e.ToString());
                }
            }
        }

        private void msgHandler(byte[] buffer, int bufferLength, bool audit)
        {
            string msg = System.Text.Encoding.UTF8.GetString(buffer, 0, bufferLength).Replace("\\", "\\\\").Trim();
            if (msg == string.Empty)
            {
                return;
            }

            if (audit)
            {
                auditWindow.addToAuditList(msg);
            }
            else
            {
                this.Dispatcher.Invoke(new Action(() => sendDanmaku(msg)));
            }
        }
        #endregion

        #region Entrys

        private void OhMyDanmaku_Init()
        {
            setSize(GlobalVariable._RENDER_WIDTH, GlobalVariable._RENDER_HEIGHT);

            lib = new wpfDanmakulib(
                danmakuRender,
                ra,
                true,
                InitCompleted,
                GlobalVariable._user_danmaku_Duration,
                GlobalVariable._user_danmaku_FontSize,
                GlobalVariable._user_danmaku_EnableShadow,
                GlobalVariable._user_danmaku_colorR,
                GlobalVariable._user_danmaku_colorG,
                GlobalVariable._user_danmaku_colorB
                );

            if (GlobalVariable._user_audit)
            {
                auditWindow = new Audit(this);
                auditWindow.Show();
            }

            networkThread = new Thread(() => networkListenLoop(GlobalVariable._user_com_port, GlobalVariable._user_audit));
            networkThread.IsBackground = true;
            networkThread.Name = "CommunicationThread";
            networkThread.Start(); //Start listener thread
        }

        private void InitCompleted()
        {
            //Do sth after init
            lib.createDanmaku("OhMyDanmaku 初始化完毕", 0, lib.getNormalRowHeight(), GlobalVariable._user_danmaku_FontSize, GlobalVariable._user_danmaku_Duration, GlobalVariable._user_danmaku_colorR, GlobalVariable._user_danmaku_colorG, GlobalVariable._user_danmaku_colorB, GlobalVariable._user_danmaku_EnableShadow);
            lib.createDanmaku("OhMyDanmaku Initialization Complete", 1, lib.getNormalRowHeight(), GlobalVariable._user_danmaku_FontSize, GlobalVariable._user_danmaku_Duration, GlobalVariable._user_danmaku_colorR, GlobalVariable._user_danmaku_colorG, GlobalVariable._user_danmaku_colorB, GlobalVariable._user_danmaku_EnableShadow);
        }

        public void sendDanmaku(string msg)
        {
            lib.generateDanmaku(msg);
        }

        #endregion

        #region Events

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            if (networkThread != null)
            {
                networkThread.Abort();
                networkSocket.Close();
            }

            if (auditWindow != null)
            {
                auditWindow.Close();
                auditWindow = null;
            }

            Window sw = new Settings();
            sw.ShowDialog();

            OhMyDanmaku_Init();
        }

        private void visualBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void renderWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (auditWindow != null)
            {
                auditWindow.Close();
            }
        }
        #endregion

        #region LittleHelpers

        private void setSize(double _width, double _height)
        {
            //Window size
            renderWindow.Height = _height;
            renderWindow.Width = _width;

            //Render area size
            danmakuRender.Height = _height;
            danmakuRender.Width = _width;

            //Border size
            visualBorder.Height = _height;
            visualBorder.Width = _width;

            //Setting Button postion
            settingButton.SetValue(Canvas.TopProperty, (double)0);
            settingButton.SetValue(Canvas.LeftProperty, (double)0);
        }

        public void loadDefaultConfig()
        {
            GlobalVariable._RENDER_HEIGHT = _SCREEN_HEIGHT;
            GlobalVariable._RENDER_WIDTH = _SCREEN_WIDTH;

            GlobalVariable._user_danmaku_FontSize = 30;
            GlobalVariable._user_danmaku_Duration = 9000;
            GlobalVariable._user_danmaku_EnableShadow = false;

            GlobalVariable._user_danmaku_colorR = 255;
            GlobalVariable._user_danmaku_colorG = 255;
            GlobalVariable._user_danmaku_colorB = 255;

            GlobalVariable._user_com_port = 8585;

            GlobalVariable._user_audit = false;
        }

        #endregion
        private string getRandomString(int _Length)
        {
            string _strList = "qwertyuioplkjhgfdsazxcvbnm1234567890";
            string _buffer = "";
            for (int i = 1; i <= _Length; i++)
            {
                _buffer += _strList[ra.Next(0, 35)];
            }
            return _buffer;
        }
        private void pressureButton_Click(object sender, RoutedEventArgs e)
        {
            Thread test = new Thread(() =>
            {
            for (int i = 0; i < 500; i++)
            {
                Thread.Sleep(500);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    sendDanmaku(getRandomString(20)); 
                }));
                   
                }
            });
            test.IsBackground = true;
            test.Start();
        }
    }
}
