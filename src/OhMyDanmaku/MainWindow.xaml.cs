using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows.Media.Effects;
using System.Collections;
using System.Net;
using System.Net.Sockets;


namespace OhMyDanmaku
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        Random ra = new Random();

        Socket s;

        Thread t;


        //Render - System
        public double _SCREEN_WIDTH = SystemParameters.PrimaryScreenWidth;
        public double _SCREEN_HEIGHT = SystemParameters.PrimaryScreenHeight;

        //prevent Cover
        public int _maxRow;
        bool[] _rowList;

        //Danmaku - System
        int _system_danmaku_rowHeight; //由防遮挡系统计算

        public MainWindow()
        {
            MessageBox.Show("点击确定开始初始化之前,请打开\"自动隐藏任务栏\"以保证弹幕渲染区域定位正常!","初始化前-操作确认");

            InitializeComponent();

            //载入默认配置到变量中
            loadConfig();

            //初始化
            OhMyDanmaku_Init();

        }


        #region Danmaku

        private void createDanmaku(string _content, int _targetRow, int _rowHeight, int _fontSize, int _duration, byte _R, byte _G, byte _B, bool _enableShadow) //target row num should be start at 0
        {
            TextBlock _singleDanmaku = new TextBlock();

            _singleDanmaku.Text = _content;
            _singleDanmaku.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Microsoft YaHei");
            _singleDanmaku.Name = "uni_" + getRandomString(ra.Next(5, 8));
            _singleDanmaku.FontSize = _fontSize;
            _singleDanmaku.SetValue(Canvas.TopProperty, (double)_targetRow * _rowHeight);
            //颜色
            _singleDanmaku.Foreground = new SolidColorBrush(Color.FromRgb(_R, _G, _B));

            //阴影
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

            _singleDanmaku.Loaded += delegate(object o, RoutedEventArgs e) { doAnimation(_singleDanmaku.Name, _duration, _targetRow); };

            danmakuRender.Children.Add(_singleDanmaku);
            danmakuRender.RegisterName(_singleDanmaku.Name, _singleDanmaku); 

            //锁定当前行
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

            _sb.Completed += delegate(object o, EventArgs e) { removeOutdateDanmaku(_targetDanmaku.Name, _row); };

            _sb.Children.Add(_doubleAnimation);
            _sb.Begin();
        }

        private void removeOutdateDanmaku(string _targetUniqueName,int _row)
        {
            TextBlock ready2remove = danmakuRender.FindName(_targetUniqueName) as TextBlock;
            if (ready2remove != null)
            {
                danmakuRender.Children.Remove(ready2remove);
                danmakuRender.UnregisterName(_targetUniqueName);
                ready2remove = null;


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
            //创建测试弹幕
            TextBlock _testDanmaku = new TextBlock();

            _testDanmaku.Text = "OhMyDanmaku";
            _testDanmaku.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Microsoft YaHei");
            _testDanmaku.Name = "uni_testheight";
            _testDanmaku.FontSize = GlobalVariable._user_danmaku_FontSize;

            _testDanmaku.Loaded += delegate(object o, RoutedEventArgs e) { calcRow(_testDanmaku.Name, _testDanmaku.ActualHeight); InitComplete(); };

            danmakuRender.Children.Add(_testDanmaku);
            danmakuRender.RegisterName(_testDanmaku.Name, _testDanmaku);
        }

        private void calcRow(string _testTargetName, double _fontHeight)
        {
            //移除测试弹幕
            TextBlock _testtargetDanmaku = danmakuRender.FindName(_testTargetName) as TextBlock;
            danmakuRender.Children.Remove(_testtargetDanmaku);
            danmakuRender.UnregisterName(_testTargetName);

            _maxRow = (int)(GlobalVariable._RENDER_HEIGHT / _fontHeight);
            _rowList = new bool[_maxRow - 1];

            //RowHeight
            _system_danmaku_rowHeight = (int)_fontHeight;


        }

        private int getAvailableRow()
        {
            ArrayList idleRows = new ArrayList();
            int i = 0;

            foreach (bool a in _rowList)
            {
                if (a == false)
                {
                    idleRows.Add(i);
                }
                i++;
            }
            if (idleRows.Count == 0)
            {
                unlockRow();
                int ret = ra.Next(0, _maxRow - 1);

                //debug
                Console.WriteLine("All Rows Full,unlock all rows.");

                return ret;
            }
            else
            {
                int ret = (int)idleRows[ra.Next(0, idleRows.Count - 1)];
                return ret;
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
                for (int i = 0; i <= _rowList.Length - 1; i++)
                {
                    _rowList[i] = false;
                }
            }
            else
            {
                if (!(_row > _rowList.Length - 1))
                {
                    _rowList[_row] = false;
                }
            }
        }

        private void loopCheckRow()
        {
            //暂时用移除弹幕后解锁当前行的方法...因为没有想到一个比较完美的检测弹幕是否完全进入屏幕的方法
        }
        #endregion





        #region Communication

        private void networkComLoop()
        {
            Console.WriteLine("communication Thread is Starting..\r\nSocket Listen Port:" + GlobalVariable._user_com_port.ToString());
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Loopback, GlobalVariable._user_com_port);

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

                    Console.WriteLine(recvmsg);
                    //clear all!
                    remote = (EndPoint)client;
                    num = 0;
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        sendDanmaku(recvmsg);
                    }));

                }
                catch (ThreadAbortException)
                {
                    Console.WriteLine("Communication Thread is shutting down..");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Communication throw some unknown Exception! \r\n" + e);
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
            //初始化界面及Render大小
            setSize(GlobalVariable._RENDER_WIDTH, GlobalVariable._RENDER_HEIGHT);

            //初始化弹幕防遮挡机制
            preventCoverInit(GlobalVariable._RENDER_HEIGHT, GlobalVariable._user_danmaku_FontSize);

            //启动通信线程
            t = new Thread(() => networkComLoop());
            t.IsBackground = true;
            t.Name = "CommunicationThread_" + getRandomString(5);
            t.Start();

            //设置StatuText
            statuText.Text = "Port:" + GlobalVariable._user_com_port.ToString();
}

        private void InitComplete() {
            //Do sth after init
            createDanmaku("OhMyDanmaku初始化完毕", 0, 50, 50, GlobalVariable._user_danmaku_Duration, GlobalVariable._user_danmaku_colorR, GlobalVariable._user_danmaku_colorG, GlobalVariable._user_danmaku_colorB, GlobalVariable._user_danmaku_EnableShadow);
            createDanmaku("如果防火墙弹出提示,请点击【允许访问】", 1, 50, 50, GlobalVariable._user_danmaku_Duration, GlobalVariable._user_danmaku_colorR, GlobalVariable._user_danmaku_colorG, GlobalVariable._user_danmaku_colorB, GlobalVariable._user_danmaku_EnableShadow);
        }

        private void sendDanmaku(string _content)
        {
            createDanmaku(
                _content, 
                getAvailableRow(), 
                _system_danmaku_rowHeight,
                GlobalVariable._user_danmaku_FontSize,
                GlobalVariable._user_danmaku_Duration,
                GlobalVariable._user_danmaku_colorR,
                GlobalVariable._user_danmaku_colorG,
                GlobalVariable._user_danmaku_colorB,
                GlobalVariable._user_danmaku_EnableShadow
                );
        }

        #endregion



        #region Events

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            shutdownNetworkComLoop();

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
            //更新窗口大小
            renderWindow.Height = _height;
            renderWindow.Width = _width;

            //更新Canvas Render大小
            danmakuRender.Height = _height;
            danmakuRender.Width = _width;

            //更新可视化边界大小
            visualBorder.Height = _height;
            visualBorder.Width = _width;

            //settingButton
            settingButton.SetValue(Canvas.TopProperty, (double)0);
            settingButton.SetValue(Canvas.LeftProperty, (double)0);

            //statuText
            statuText.SetValue(Canvas.TopProperty, (double)0);
            statuText.SetValue(Canvas.LeftProperty, (double)20);

        }

        public void loadConfig(double renderWidth = -65535, double renderHeight = -65535, int danmakuFontSize = 30, int danmakuDuration = 6000, byte danmakuColorR = 255, byte danmakuColorG = 255, byte danmakuColorB = 255, bool danmakuShadow = true, int comPort = 8585)
        {
            if (renderWidth == -65535 && renderHeight == -65535)
            {
                GlobalVariable._RENDER_HEIGHT = _SCREEN_HEIGHT;
                GlobalVariable._RENDER_WIDTH = _SCREEN_WIDTH;
            }
            else
            {
                GlobalVariable._RENDER_WIDTH = renderWidth;
                GlobalVariable._RENDER_HEIGHT = renderHeight;
            }

            GlobalVariable._user_danmaku_FontSize = danmakuFontSize;
            GlobalVariable._user_danmaku_Duration = danmakuDuration;
            GlobalVariable._user_danmaku_EnableShadow = danmakuShadow;

            GlobalVariable._user_danmaku_colorR = danmakuColorR;
            GlobalVariable._user_danmaku_colorG = danmakuColorG;
            GlobalVariable._user_danmaku_colorB = danmakuColorB;

            GlobalVariable._user_com_port = comPort;
        }

        #endregion

    }
}
