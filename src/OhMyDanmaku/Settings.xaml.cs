using System;
using System.Windows;

namespace OhMyDanmaku
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //载入数据到TextBox
            render_width.Text = GlobalVariable._RENDER_WIDTH.ToString();
            render_height.Text = GlobalVariable._RENDER_HEIGHT.ToString();

            danmaku_size.Text = GlobalVariable._user_danmaku_FontSize.ToString();
            danmaku_duration.Text = GlobalVariable._user_danmaku_Duration.ToString();
            danmaku_R.Text = GlobalVariable._user_danmaku_colorR.ToString();
            danmaku_G.Text = GlobalVariable._user_danmaku_colorG.ToString();
            danmaku_B.Text = GlobalVariable._user_danmaku_colorB.ToString();
            danmaku_shadow.IsChecked = GlobalVariable._user_danmaku_EnableShadow;

            com_port.Text = GlobalVariable._user_com_port.ToString();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariable._RENDER_WIDTH = Convert.ToDouble(render_width.Text);
            GlobalVariable._RENDER_HEIGHT = Convert.ToDouble(render_height.Text);
            GlobalVariable._user_danmaku_FontSize = Convert.ToInt32(danmaku_size.Text);
            GlobalVariable._user_danmaku_Duration = Convert.ToInt32(danmaku_duration.Text);
            GlobalVariable._user_danmaku_colorR = Convert.ToByte(danmaku_R.Text);
            GlobalVariable._user_danmaku_colorG = Convert.ToByte(danmaku_G.Text);
            GlobalVariable._user_danmaku_colorB = Convert.ToByte(danmaku_B.Text);
            GlobalVariable._user_danmaku_EnableShadow = danmaku_shadow.IsChecked.Value;
            GlobalVariable._user_com_port = Convert.ToInt32(com_port.Text);

            this.Close();
        }
    }
}
