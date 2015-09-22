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


        #region Danmaku

        private void createDanmaku(string _content, int _targetRow, int _rowHeight, int _fontSize, int _duration, byte _R, byte _G, byte _B, bool _enableShadow) //_targetRow counting from zero
        {
            TextBlock _singleDanmaku = new TextBlock();

            _singleDanmaku.Text = _content;
            _singleDanmaku.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Microsoft YaHei");
            _singleDanmaku.Name = "uni_" + getRandomString(ra.Next(5, 8));
            _singleDanmaku.FontSize = _fontSize;
            _singleDanmaku.SetValue(Canvas.TopProperty, (double)_targetRow * _rowHeight);
            _singleDanmaku.Foreground = new SolidColorBrush(Color.FromRgb(_R, _G, _B)); //Color

            //Shadow
            if (_enableShadow == true)
            {
                DropShadowEffect _ef = new DropShadowEffect();

                _ef.RenderingBias = RenderingBias.Performance;
                _ef.Opacity = (double)100;
                _ef.ShadowDepth = (double)0;
                _ef.BlurRadius = (double)11;

                if (_R == 0 && _G == 0 && _B == 0)
                {
                    _ef.Color = Color.FromRgb(255, 255, 255);
                }
                else
                {
                    _ef.Color = Color.FromRgb(0, 0, 0);
                }

                _singleDanmaku.Effect = _ef;
            }

            _singleDanmaku.Loaded += delegate(object o, RoutedEventArgs e) { doAnimation(_singleDanmaku.Name, _duration, _targetRow); }; //add animation

            danmakuRender.Children.Add(_singleDanmaku);
            danmakuRender.RegisterName(_singleDanmaku.Name, _singleDanmaku);

            //Lock this row
            lockRow(_targetRow);
        }

        private void doAnimation(string _targetUniqueName, int _duration, int _row)
        {
            TextBlock _targetDanmaku = danmakuRender.FindName(_targetUniqueName) as TextBlock;

            double _danmakuWidth = _targetDanmaku.ActualWidth;
            DoubleAnimation _doubleAnimation = new DoubleAnimation(GlobalVariable._RENDER_WIDTH, -_danmakuWidth, new Duration(TimeSpan.FromMilliseconds(_duration)), FillBehavior.Stop);

            Storyboard _sb = new Storyboard();
            Storyboard.SetTarget(_doubleAnimation, _targetDanmaku);
            Storyboard.SetTargetProperty(_doubleAnimation, new PropertyPath("(Canvas.Left)"));

            _sb.Completed += delegate(object o, EventArgs e) { removeOutdateDanmaku(_targetDanmaku.Name, _row); }; //remove danmaku after animation end

            _sb.Children.Add(_doubleAnimation);
            _sb.Begin();
        }

        private void removeOutdateDanmaku(string _targetUniqueName, int _row)
        {
            TextBlock ready2remove = danmakuRender.FindName(_targetUniqueName) as TextBlock;
            if (ready2remove != null)
            {
                danmakuRender.Children.Remove(ready2remove);
                danmakuRender.UnregisterName(_targetUniqueName);
                ready2remove = null;

                //Unlock this row
                unlockRow(_row);
            }
            else
            {
                Console.WriteLine("Remove Danmaku Error.");
            }
        }
        #endregion

        #region PreventCover

        private void preventCoverInit(double _renderHeight, double _fontSize)
        {
            //Create a test danmaku
            TextBlock _testDanmaku = new TextBlock();

            _testDanmaku.Text = "OhMyDanmaku";
            _testDanmaku.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Microsoft YaHei");
            _testDanmaku.Name = "uni_testheight";
            _testDanmaku.FontSize = _fontSize;

            _testDanmaku.Loaded += delegate(object o, RoutedEventArgs e) { calcRow(_renderHeight, _testDanmaku.Name, _testDanmaku.ActualHeight); InitCompleted(); };

            danmakuRender.Children.Add(_testDanmaku);
            danmakuRender.RegisterName(_testDanmaku.Name, _testDanmaku);
        }

        private void calcRow(double _renderHeight, string _testTargetName, double _fontHeight)
        {
            //Remove the test danmaku
            TextBlock _testtargetDanmaku = danmakuRender.FindName(_testTargetName) as TextBlock;
            danmakuRender.Children.Remove(_testtargetDanmaku);
            danmakuRender.UnregisterName(_testTargetName);

            //get RowNumbers
            _maxRow = (int)(_renderHeight / _fontHeight);

            //get a row list
            _rowList = new bool[_maxRow];

            //get RowHeight
            _system_danmaku_rowHeight = (int)_fontHeight;
        }

        private int getAvailableRow()
        {
            idleRows.Clear();

            for (int i = 0; i < _rowList.Length; i++)
            {
                if (_rowList[i] == false)
                {
                    idleRows.Add(i);
                }
            }

            if (idleRows.Count == 0)
            {
                Console.WriteLine("Unlock all rows.");
                unlockRow();

                return ra.Next(0, _maxRow + 1);
            }
            else
            {
                return (int)idleRows[ra.Next(0, idleRows.Count)];
            }
        }

        private void lockRow(int _row)
        {
            _rowList[_row] = true;
        }

        private void unlockRow(int _row = -1)
        {
            if (_row == -1)
            {
                _rowList = new bool[_maxRow];
            }
            else
            {
                _rowList[_row] = false;
            }
        }
        #endregion

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

            preventCoverInit(GlobalVariable._RENDER_HEIGHT, GlobalVariable._user_danmaku_FontSize); //init prevent cover system

            if (GlobalVariable._user_audit)
            {
                auditWindow = new Audit();
                auditWindow.Show();
            }

            t = new Thread(() => networkComLoop(GlobalVariable._user_com_port, GlobalVariable._user_audit));
            t.IsBackground = true;
            t.Name = "CommunicationThread_" + getRandomString(5);
            t.Start(); //Start listener thread


            statuText.Text = "Listen Port:" + GlobalVariable._user_com_port.ToString(); //Show listen port to statuText
        }

        private void InitCompleted()
        {
            //Do sth after init
            createDanmaku("OhMyDanmaku 初始化完毕", 0, _system_danmaku_rowHeight, GlobalVariable._user_danmaku_FontSize, GlobalVariable._user_danmaku_Duration, GlobalVariable._user_danmaku_colorR, GlobalVariable._user_danmaku_colorG, GlobalVariable._user_danmaku_colorB, GlobalVariable._user_danmaku_EnableShadow);
            createDanmaku("OhMyDanmaku Initialization Complete", 1, _system_danmaku_rowHeight, GlobalVariable._user_danmaku_FontSize, GlobalVariable._user_danmaku_Duration, GlobalVariable._user_danmaku_colorR, GlobalVariable._user_danmaku_colorG, GlobalVariable._user_danmaku_colorB, GlobalVariable._user_danmaku_EnableShadow);
        }

        private void sendDanmaku(string _content)
        {
            int row = getAvailableRow();
            this.Dispatcher.Invoke(new Action(() =>
            {
                createDanmaku(
                    _content,
                    row,
                    _system_danmaku_rowHeight,
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
            statuText.Visibility = Visibility.Visible;
            this.DragMove();
        }

        private void visualBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            statuText.Visibility = Visibility.Hidden;
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

            //StatuText position
            statuText.SetValue(Canvas.TopProperty, (double)20);
            statuText.SetValue(Canvas.LeftProperty, (double)0);

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
