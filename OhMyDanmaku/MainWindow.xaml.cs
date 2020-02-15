using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPFDanmakuLib;

namespace OhMyDanmaku {
    /// <summary>
    /// MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        #region global
        Random ra = new Random();
        Socket networkSocket;
        Thread networkThread;
        Audit auditWindow = null;
        public WPFDanmakuEngine engine;


        #endregion

        public MainWindow() {
            MessageBox.Show("进入下一步之前,请打开\"自动隐藏任务栏\"以使弹幕区域定位正常!\r\n\r\nBefore next step, Please enable \"Auto hide taskbar\" to make danmaku area get right position!", "Before Initialization...");

            InitializeComponent();

            loadDefaultConfig(); //Load Default config to GlobalVariable

            danmakuRender.Loaded += delegate (object sender, RoutedEventArgs e) {
                OhMyDanmaku_Init(); //start init process
            };
        }


        #region Network

        private void networkListenLoop(int _port, bool audit) {
            Console.WriteLine("Network thread is starting.. Listen on:" + GlobalVariable._user_com_port.ToString());

            networkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ip = new IPEndPoint(IPAddress.Loopback, _port);
            networkSocket.Bind(ip);

            byte[] buf = new byte[102400];
            int bufLength;

            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

            while (true) {
                try {
                    bufLength = networkSocket.ReceiveFrom(buf, ref remote);
                    Console.WriteLine(buf.Length);

                    string msg = System.Text.Encoding.UTF8.GetString(buf, 0, bufLength).Replace("\\", "\\\\").Trim();
                    if (msg == string.Empty) {
                        return;
                    }

                    if (audit) {
                        auditWindow.addToAuditList(msg);
                    } else {
                        this.Dispatcher.Invoke(new Action(() => engine.DrawDanmaku(msg)));
                    }

                } catch (ThreadAbortException) {
                    Console.WriteLine("Communication Thread is shutting down..");
                } catch (Exception e) {
                    Console.WriteLine("Unknown Exception: \r\n" + e.ToString());
                }
            }
        }
        #endregion

        #region Entrys
        private void OhMyDanmaku_Init() {
            setSize(GlobalVariable._user_render_width, GlobalVariable._user_render_height);
            setPosition(0, 0);

            engine = new WPFDanmakuEngine(
                engineBehavior: new EngineBehavior(DrawMode.WPF, CollisionPrevention.Enabled),
                defaultStyle: new DanmakuStyle(
                    Duration: GlobalVariable._user_danmaku_Duration,
                    ColorR: GlobalVariable._user_danmaku_colorR,
                    ColorG: GlobalVariable._user_danmaku_colorG,
                    ColorB: GlobalVariable._user_danmaku_colorB,
                    FontSize: GlobalVariable._user_danmaku_FontSize,
                    OutlineEnabled: GlobalVariable._user_danmaku_EnableOutline,
                    ShadowEnabled: GlobalVariable._user_danmaku_EnableShadow,
                    PositionX: GlobalVariable._user_render_width
                    ),
                targetCanvas: danmakuRender
                );

            if (GlobalVariable._user_audit_enabled) {
                auditWindow = new Audit(this);
                auditWindow.Show();
            }

            networkThread = new Thread(() => networkListenLoop(GlobalVariable._user_com_port, GlobalVariable._user_audit_enabled));
            networkThread.IsBackground = true;
            networkThread.Name = "CommunicationThread";
            networkThread.Start();

            InitCompleted();
        }

        private void InitCompleted() {
            //Do sth after init
            engine.DrawDanmaku("OhMyDanmaku 初始化完毕");
            engine.DrawDanmaku("OhMyDanmaku Initialization Complete");
        }
        #endregion

        #region UI Events

        private void Setting_Click(object sender, RoutedEventArgs e) {
            if (networkThread != null) {
                networkThread.Abort();
                networkSocket.Close();
            }

            if (auditWindow != null) {
                auditWindow.Close();
                auditWindow = null;
            }

            Window sw = new Settings();
            sw.ShowDialog();

            OhMyDanmaku_Init();
        }

        private void visualBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                this.DragMove();
            }
        }

        private void renderWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (auditWindow != null) {
                auditWindow.Close();
            }
        }

        private void pressureButton_Click(object sender, RoutedEventArgs e) {
            Thread test = new Thread(() => {
                for (int i = 0; i < 50; i++) {
                    Thread.Sleep(500);
                    this.Dispatcher.Invoke(new Action(() => {
                        engine.DrawDanmaku(Utils.GetRandomString(20));
                    }));

                }
            });
            test.IsBackground = true;
            test.Start();
        }
        private void HideButton_Click(object sender, RoutedEventArgs e) {
            visualBorder.Visibility = Visibility.Hidden;
            settingButton.Visibility = Visibility.Hidden;
            pressureButton.Visibility = Visibility.Hidden;
            HideButton.Visibility = Visibility.Hidden;
        }
        #endregion

        #region LittleHelpers

        private void setSize(double _width, double _height) {
            //Window size
            renderWindow.Height = _height;
            renderWindow.Width = _width;

            //Render area size
            danmakuRender.Height = _height;
            danmakuRender.Width = _width;

            //Border size
            visualBorder.Height = _height;
            visualBorder.Width = _width;

            //Buttons postion
            settingButton.SetValue(Canvas.TopProperty, (double)0);

            pressureButton.SetValue(Canvas.TopProperty, (double)0);

            HideButton.SetValue(Canvas.TopProperty, (double)0);
        }

        private void setPosition(double left, double top) {
            //Window size
            renderWindow.Left = left;
            renderWindow.Top = top;
        }

        public void loadDefaultConfig() {
            GlobalVariable._user_render_height = SystemParameters.PrimaryScreenHeight;
            GlobalVariable._user_render_width = SystemParameters.PrimaryScreenWidth;

            GlobalVariable._user_danmaku_FontSize = 30;
            GlobalVariable._user_danmaku_Duration = 9000;
            GlobalVariable._user_danmaku_EnableShadow = false;
            GlobalVariable._user_danmaku_EnableOutline = true;

            GlobalVariable._user_danmaku_colorR = 255;
            GlobalVariable._user_danmaku_colorG = 255;
            GlobalVariable._user_danmaku_colorB = 255;

            GlobalVariable._user_com_port = 8585;

            GlobalVariable._user_audit_enabled = false;
        }
        #endregion

    }
}
