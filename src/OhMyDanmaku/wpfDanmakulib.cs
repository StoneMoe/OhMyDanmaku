using System;
using System.Collections;
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
        private Random ra = new Random();
        private Canvas danmakuRender;
        private bool enablePreventCoverSystem;
        public delegate void initCompleteHandler();
        private double Canvas_Width;
        private double Canvas_Height;

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
        private ArrayList idleRows = new ArrayList();

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
        /// <param name="c">A WPF Canvas for generating danmaku</param>
        /// <param name="final">Delegate a method when wpfDanmakulib initialization completed</param>
        /// <param name="Duration">Default danmaku style</param>
        /// <param name="FontSize">Default danmaku style</param>
        /// <param name="Shadow">Default danmaku style</param>
        /// <param name="ColorR">Default danmaku style</param>
        /// <param name="ColorG">Default danmaku style</param>
        /// <param name="ColorB">Default danmaku style</param>
        /// <param name="enableAPCS">Enable auto prevent cover system</param>
        public wpfDanmakulib(Canvas c, initCompleteHandler final, int Duration, int FontSize, bool Shadow, byte ColorR, byte ColorG, byte ColorB, bool enableAPCS)
        {
            danmakuRender = c;
            enablePreventCoverSystem = enableAPCS;

            Canvas_Width = c.Width;
            Canvas_Height = c.Height;

            danmaku_Duration = Duration;
            danmaku_FontSize = FontSize;
            danmaku_EnableShadow = Shadow;
            danmaku_colorR = ColorR;
            danmaku_colorG = ColorG;
            danmaku_colorB = ColorB;

            rowSystemInit();

            if (enablePreventCoverSystem)
            {
                preventCoverInit();
            }

            //Complete
            final();
        }

        #region Danmaku
        /// <summary>
        /// This is a completely automatic danmaku generater, using default construct method params. This methodonly available when APCS is enabled.
        /// </summary>
        /// <param name="text">Danmaku Content</param>
        public void generateDanmaku(string text)
        {
            if (enablePreventCoverSystem)
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
            TextBlock _singleDanmaku = new TextBlock();

            _singleDanmaku.Text = _content;
            _singleDanmaku.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Microsoft YaHei");
            _singleDanmaku.Name = "uni_" + getRandomString(ra.Next(5, 8));
            _singleDanmaku.FontSize = _fontSize;
            _singleDanmaku.SetValue(Canvas.TopProperty, (double)_targetRow * _rowHeight);
            _singleDanmaku.Foreground = new SolidColorBrush(Color.FromRgb(_R, _G, _B));

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

            if (enablePreventCoverSystem)
            {
                lockRow(_targetRow);
            }
        }

        private void doAnimation(string _targetUniqueName, int _duration, int _row)
        {
            TextBlock _targetDanmaku = danmakuRender.FindName(_targetUniqueName) as TextBlock;

            double _danmakuWidth = _targetDanmaku.ActualWidth;
            DoubleAnimation _doubleAnimation = new DoubleAnimation(Canvas_Width, -_danmakuWidth, new Duration(TimeSpan.FromMilliseconds(_duration)), FillBehavior.Stop);

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

                if (enablePreventCoverSystem)
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
        private void rowSystemInit()
        {
            //Create a test danmaku to calculate row
            TextBlock _testDanmaku = new TextBlock();

            _testDanmaku.Text = "OhMyDanmaku";
            _testDanmaku.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Microsoft YaHei");
            _testDanmaku.Name = "uni_testheight";
            _testDanmaku.FontSize = danmaku_FontSize;

            _testDanmaku.Loaded += delegate(object o, RoutedEventArgs e)
            {
                calcRow(Canvas_Height, _testDanmaku.Name, _testDanmaku.ActualHeight);
            };

            danmakuRender.Children.Add(_testDanmaku);
            danmakuRender.RegisterName(_testDanmaku.Name, _testDanmaku);
        }
        private void calcRow(double _renderHeight, string _testTargetName, double _fontHeight)
        {
            //Remove the test danmaku
            TextBlock _testtargetDanmaku = danmakuRender.FindName(_testTargetName) as TextBlock;
            danmakuRender.Children.Remove(_testtargetDanmaku);
            danmakuRender.UnregisterName(_testTargetName);

            //total Row
            _maxRow = (int)(_renderHeight / _fontHeight);

            //Row Height
            manual_danmaku_rowHeight = (int)_fontHeight;
        }
        /// <summary>
        /// Returns total row slot numbers, which generated by construct method with the params passd in
        /// </summary>
        /// <returns></returns>
        public int getRowNumbers()
        {
            return _maxRow;
        }
        /// <summary>
        /// Returns normal row slot height, which generated by construct method with the params passd in
        /// </summary>
        /// <returns></returns>
        public int getNormalRowHeight()
        {
            return manual_danmaku_rowHeight;
        }
        #endregion

        #region preventCover
        private void preventCoverInit()
        {
            _rowList = new bool[_maxRow]; //init a row list for recording row status
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
    }
}
