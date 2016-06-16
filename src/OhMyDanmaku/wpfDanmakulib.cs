using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace OhMyDanmaku
{
    class wpfDanmakulib
    {
        #region general
        //Init
        private Random ra;
        private Canvas system_RenderCanvas;
        private bool system_enableAPCS;
        public delegate void initCompleteHandler();

        //Danmaku Style Config
        private int danmaku_Duration;
        private int danmaku_FontSize;
        private bool danmaku_EnableShadow;
        private byte danmaku_colorR;
        private byte danmaku_colorG;
        private byte danmaku_colorB;

        //row system
        private int _maxRow;
        private int manual_danmaku_rowHeight;

        //Auto Prevent Cover System
        private bool[] _rowList;
        private ArrayList idleRows;

        //Helper
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
        #endregion

        /// <summary>
        /// wpfDanmakulib initialization Entry point
        /// </summary>
        /// <param name="wpfCanvas">A WPF Canvas for containing danmakus</param>
        /// <param name="random">Use your own random seed or leave it null</param>
        /// <param name="final">Call a method when wpfDanmakulib initialization completed or leave it null</param>
        /// <param name="Duration">Danmaku stay duration (ms)</param>
        /// <param name="FontSize">Danmaku font size</param>
        /// <param name="Shadow">Danmaku Shadow Effect visibility</param>
        /// <param name="ColorR">Danmaku color red value (0-255)</param>
        /// <param name="ColorG">Danmaku color green value (0-255)</param>
        /// <param name="ColorB">Danmaku color blue value (0-255)</param>
        /// <param name="enableAPCS">APCS(Auto Prevent Cover System) is used for maintaining a row slot status list for solving danmaku occlusion issue. Disable APCS can save few system resource.</param>
        public wpfDanmakulib(Canvas wpfCanvas, Random random = null, bool enableAPCS = true, initCompleteHandler final = null, int Duration = 9000, int FontSize = 30, bool Shadow = true, byte ColorR = 255, byte ColorG = 255, byte ColorB = 255)
        {
            system_RenderCanvas = wpfCanvas;
            if (random != null)
            {
                ra = random;
            }
            else
            {
                ra = new Random();
            }
            system_enableAPCS = enableAPCS;

            danmaku_Duration = Duration;
            danmaku_FontSize = FontSize;
            danmaku_EnableShadow = Shadow;
            danmaku_colorR = ColorR;
            danmaku_colorG = ColorG;
            danmaku_colorB = ColorB;

            libInit(final);
        }

        #region Danmaku
        /// <summary>
        /// This is a completely automatic danmaku generater, create a danmaku in a random row slot with default construct method params. This method only available when APCS is enabled.
        /// </summary>
        /// <param name="text">Danmaku Content</param>
        public void generateDanmaku(string text)
        {
            if (system_enableAPCS)
            {
                createDanmaku(
                    text,
                    getAvailableRow(),
                    manual_danmaku_rowHeight,
                    danmaku_FontSize,
                    danmaku_Duration,
                    danmaku_colorR,
                    danmaku_colorG,
                    danmaku_colorB,
                    danmaku_EnableShadow
                    );
            }
            else
            {
                throw new InvalidOperationException("APCS is disabled");
            }
        }
        /// <summary>
        /// Create a danmaku manually, instead of using default construct method params.
        /// </summary>
        /// <param name="_content">Danmaku Content</param>
        /// <param name="_targetRow">Target row slot, check "getRowNumbers" method</param>
        /// <param name="_rowHeight">Row slot height, check "getNormalRowHeight" method</param>
        /// <param name="_fontSize">Danmaku font size</param>
        /// <param name="_duration">Danmaku stay duration cross the render area</param>
        /// <param name="_R">Danmaku color red</param>
        /// <param name="_G">Danmaku color green</param>
        /// <param name="_B">Danmaku color blue</param>
        /// <param name="_enableShadow">Danmaku shadow</param>
        public void createDanmaku(string _content, int _targetRow, int _rowHeight, int _fontSize, int _duration, byte _R, byte _G, byte _B, bool _enableShadow) //_targetRow counting from zero
        {

            OutlinedTextBlock _singleDanmaku = new OutlinedTextBlock();

            _singleDanmaku.Text = _content;
            _singleDanmaku.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Microsoft YaHei");
            _singleDanmaku.Name = "uni_" + getRandomString(ra.Next(5, 8));
            _singleDanmaku.FontSize = _fontSize;
            _singleDanmaku.SetValue(Canvas.TopProperty, (double)_targetRow * _rowHeight);
            _singleDanmaku.Fill = new SolidColorBrush(Color.FromRgb(_R, _G, _B));
            _singleDanmaku.CacheMode = new BitmapCache();
            _singleDanmaku.FontWeight = FontWeights.Bold;

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

            system_RenderCanvas.Children.Add(_singleDanmaku);
            system_RenderCanvas.RegisterName(_singleDanmaku.Name, _singleDanmaku);

            if (system_enableAPCS)
            {
                lockRow(_targetRow);
            }
        }

        private void doAnimation(string _targetUniqueName, int _duration, int _row)
        {
            OutlinedTextBlock _targetDanmaku = system_RenderCanvas.FindName(_targetUniqueName) as OutlinedTextBlock;

            double _danmakuWidth = _targetDanmaku.ActualWidth;
            DoubleAnimation _doubleAnimation = new DoubleAnimation(system_RenderCanvas.Width, -_danmakuWidth, new Duration(TimeSpan.FromMilliseconds(_duration)), FillBehavior.Stop);

            Storyboard _sb = new Storyboard();
            Storyboard.SetTarget(_doubleAnimation, _targetDanmaku);
            Storyboard.SetTargetProperty(_doubleAnimation, new PropertyPath("(Canvas.Left)"));

            _sb.Completed += delegate(object o, EventArgs e) { removeOutdateDanmaku(_targetDanmaku.Name, _row); }; //remove danmaku after animation end

            _sb.Children.Add(_doubleAnimation);
            _sb.Begin();
        }

        private void removeOutdateDanmaku(string _targetUniqueName, int _row)
        {
            OutlinedTextBlock ready2remove = system_RenderCanvas.FindName(_targetUniqueName) as OutlinedTextBlock;
            if (ready2remove != null)
            {
                system_RenderCanvas.Children.Remove(ready2remove);
                system_RenderCanvas.UnregisterName(_targetUniqueName);
                ready2remove = null;

                if (system_enableAPCS)
                {
                    unlockRow(_row);
                }
            }
            else
            {
                Console.WriteLine("Remove Danmaku Error.");
            }
        }
        #endregion

        #region Row system
        private void libInit(initCompleteHandler initCompleted)
        {
            //FormattedText ft = new FormattedText("OhMyDanmaku", CultureInfo.GetCultureInfo("zh-cn"), FlowDirection.LeftToRight, new Typeface("Microsoft YaHei"), danmaku_FontSize, Brushes.Black);

            // Create a test danmaku to calculate row
            OutlinedTextBlock _testDanmaku = new OutlinedTextBlock();

            _testDanmaku.Text = "OhMyDanmaku";
            _testDanmaku.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Microsoft YaHei");
            _testDanmaku.Name = "uni_testheight";
            _testDanmaku.FontSize = danmaku_FontSize;
            _testDanmaku.FontWeight = FontWeights.Bold;

            _testDanmaku.Loaded += delegate(object o, RoutedEventArgs e)
            {
                calcRow(system_RenderCanvas.Height, _testDanmaku.Name, _testDanmaku.ActualHeight);
                if (system_enableAPCS)
                {
                    preventCoverInit(initCompleted);
                }
                else
                {
                    initCompleted();
                }
            };

            system_RenderCanvas.Children.Add(_testDanmaku);
            system_RenderCanvas.RegisterName(_testDanmaku.Name, _testDanmaku);
        }
        private void calcRow(double _renderHeight, string _testTargetName, double _fontHeight)
        {
            //Remove the test danmaku
            OutlinedTextBlock _testtargetDanmaku = system_RenderCanvas.FindName(_testTargetName) as OutlinedTextBlock;
            system_RenderCanvas.Children.Remove(_testtargetDanmaku);
            system_RenderCanvas.UnregisterName(_testTargetName);

            //total Row slots
            _maxRow = (int)(_renderHeight / _fontHeight);

            //Row Height
            manual_danmaku_rowHeight = (int)_fontHeight;
        }
        /// <summary>
        /// Returns total row slot numbers, generated by row slot manage system
        /// </summary>
        /// <returns></returns>
        public int getRowNumbers()
        {
            return _maxRow;
        }
        /// <summary>
        /// Returns normal row slot height, generated by row slot manage system
        /// </summary>
        /// <returns></returns>
        public int getNormalRowHeight()
        {
            return manual_danmaku_rowHeight;
        }
        #endregion

        #region preventCover
        private void preventCoverInit(initCompleteHandler initCompleted)
        {
            _rowList = new bool[_maxRow]; //init a row list for recording row status
            idleRows = new ArrayList();
            initCompleted();
        }
        private int getAvailableRow()
        {
            if (!system_enableAPCS)
            {
                throw new InvalidOperationException("APCS is disabled");
            }
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

                return ra.Next(0, _maxRow - 1);
            }
            else
            {
                return (int)idleRows[ra.Next(0, idleRows.Count - 1)];
            }
        }

        private void lockRow(int _row)
        {
            if (!system_enableAPCS)
            {
                throw new InvalidOperationException("APCS is disabled");
            }
            _rowList[_row] = true;
        }

        private void unlockRow(int _row = -1)
        {
            if (!system_enableAPCS)
            {
                throw new InvalidOperationException("APCS is disabled");
            }
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
    }
}
