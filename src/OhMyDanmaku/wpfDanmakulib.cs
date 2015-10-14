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
    class wpfDanmakulib
    {
        
        public wpfDanmakulib(Random r, Canvas c, initCompleteHandler h)
        {
            ra = r;
            danmakuRender = c;

            preventCoverInit(GlobalVariable._RENDER_HEIGHT, GlobalVariable._user_danmaku_FontSize, h);
            //init prevent cover system
        }

        #region global
        public delegate void initCompleteHandler();
        Random ra;
        Canvas danmakuRender;
        public int _maxRow;
        bool[] _rowList;
        public int _system_danmaku_rowHeight;
        ArrayList idleRows = new ArrayList();
        #endregion

        #region general
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
        #region Danmaku

        public void createDanmaku(string _content, int _targetRow, int _rowHeight, int _fontSize, int _duration, byte _R, byte _G, byte _B, bool _enableShadow) //_targetRow counting from zero
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

        private void preventCoverInit(double _renderHeight, double _fontSize, initCompleteHandler h)
        {
            //Create a test danmaku
            TextBlock _testDanmaku = new TextBlock();

            _testDanmaku.Text = "OhMyDanmaku";
            _testDanmaku.FontFamily = (FontFamily)new FontFamilyConverter().ConvertFromString("Microsoft YaHei");
            _testDanmaku.Name = "uni_testheight";
            _testDanmaku.FontSize = _fontSize;

            _testDanmaku.Loaded += delegate(object o, RoutedEventArgs e) { calcRow(_renderHeight, _testDanmaku.Name, _testDanmaku.ActualHeight); h(); };

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

        public int getAvailableRow()
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
