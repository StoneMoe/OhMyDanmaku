using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;


namespace OhMyDanmaku
{
    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Random ra = new Random();

        Socket s;

        Thread t;

        Audit auditWindow = null;

        wpfDanmakulib lib;

        public double _SCREEN_WIDTH = SystemParameters.PrimaryScreenWidth;
        public double _SCREEN_HEIGHT = SystemParameters.PrimaryScreenHeight;

        public int _maxRow;
        bool[] _rowList;
        int _system_danmaku_rowHeight;
        ArrayList idleRows = new ArrayList();

        public MainWindow()
        {
            MessageBox.Show("进入下一步之前,请打开\"自动隐藏任务栏\"以使弹幕区域定位正常!\r\n\r\nBefore next step, Please enable \"Auto hide taskbar\" to make danmaku area get right position!", "Before Initialization...");

            InitializeComponent();

            loadDefaultConfig(); //Load Default config to GlobalVariable

            OhMyDanmaku_Init(); //start init process with default config
        }

        #region Communication

        private void networkComLoop(int _port, bool audit)
        {
            Console.WriteLine("communication Thread is Starting..\r\nSocket Listen Port:" + GlobalVariable._user_com_port.ToString());

            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Loopback, _port);

            byte[] buffer = new byte[2048];

            s.Bind(ip);

            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)client;

            while (true)
            {
                try
                {
                    int num = s.ReceiveFrom(buffer, ref remote);

                    string recvmsg = System.Text.Encoding.UTF8.GetString(buffer, 0, num);

                    remote = (EndPoint)client;
                    num = 0;

                    //Basically Filter
                    recvmsg = recvmsg.Replace("\\", "\\\\");
                    recvmsg = recvmsg.Trim();
                    if (recvmsg == string.Empty)
                    {
                        continue;
                    }

                    if (audit)
                    {
                        auditWindow.addToAuditList(recvmsg);
                    }
                    else
                    {
                        Thread temp = new Thread(() => sendDanmaku(recvmsg));
                        temp.IsBackground = true;
                        temp.Start();
                    }
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

        private void shutdownNetworkComLoop()
        {
            t.Abort();
            s.Close();
        }
        #endregion

        #region Entrys

        private void OhMyDanmaku_Init()
        {
            setSize(GlobalVariable._RENDER_WIDTH, GlobalVariable._RENDER_HEIGHT);

            lib = new wpfDanmakulib(ra, danmakuRender, InitCompleted);

            if (GlobalVariable._user_audit)
            {
                auditWindow = new Audit(this);
                auditWindow.Show();
            }

            t = new Thread(() => networkComLoop(GlobalVariable._user_com_port, GlobalVariable._user_audit));
            t.IsBackground = true;
            t.Name = "CommunicationThread_" + getRandomString(5);
            t.Start(); //Start listener thread
        }

        private void InitCompleted()
        {
            //Do sth after init
            lib.createDanmaku("OhMyDanmaku 初始化完毕", 0, lib._system_danmaku_rowHeight, GlobalVariable._user_danmaku_FontSize, GlobalVariable._user_danmaku_Duration, GlobalVariable._user_danmaku_colorR, GlobalVariable._user_danmaku_colorG, GlobalVariable._user_danmaku_colorB, GlobalVariable._user_danmaku_EnableShadow);
            lib.createDanmaku("OhMyDanmaku Initialization Complete", 1, lib._system_danmaku_rowHeight, GlobalVariable._user_danmaku_FontSize, GlobalVariable._user_danmaku_Duration, GlobalVariable._user_danmaku_colorR, GlobalVariable._user_danmaku_colorG, GlobalVariable._user_danmaku_colorB, GlobalVariable._user_danmaku_EnableShadow);
        }

        public void sendDanmaku(string _content)
        {
            int row = lib.getAvailableRow();
            this.Dispatcher.Invoke(new Action(() =>
            {
                lib.createDanmaku(
                    _content,
                    row,
                    lib._system_danmaku_rowHeight,
                    GlobalVariable._user_danmaku_FontSize,
                    GlobalVariable._user_danmaku_Duration,
                    GlobalVariable._user_danmaku_colorR,
                    GlobalVariable._user_danmaku_colorG,
                    GlobalVariable._user_danmaku_colorB,
                    GlobalVariable._user_danmaku_EnableShadow
                    );
            }));

        }

        #endregion

        #region Events

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            shutdownNetworkComLoop();

            //close Audit window
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
            this.DragMove();
        }
        #endregion

        #region LittleHelpers

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
            GlobalVariable._user_danmaku_EnableShadow = true;

            GlobalVariable._user_danmaku_colorR = 255;
            GlobalVariable._user_danmaku_colorG = 255;
            GlobalVariable._user_danmaku_colorB = 255;

            GlobalVariable._user_com_port = 8585;

            GlobalVariable._user_audit = false;
        }

        #endregion
    }
}
