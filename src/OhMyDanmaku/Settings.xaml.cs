using System;
using System.Windows;

namespace OhMyDanmaku
{
    /// <summary>
    /// Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //load Current config to textbox
            render_width.Text = GlobalVariable._RENDER_WIDTH.ToString();
            render_height.Text = GlobalVariable._RENDER_HEIGHT.ToString();

            danmaku_size.Text = GlobalVariable._user_danmaku_FontSize.ToString();
            danmaku_duration.Text = GlobalVariable._user_danmaku_Duration.ToString();
            danmaku_R.Text = GlobalVariable._user_danmaku_colorR.ToString();
            danmaku_G.Text = GlobalVariable._user_danmaku_colorG.ToString();
            danmaku_B.Text = GlobalVariable._user_danmaku_colorB.ToString();
            danmaku_shadow.IsChecked = GlobalVariable._user_danmaku_EnableShadow;
            audit_mode.IsChecked = GlobalVariable._user_audit;

            com_port.Text = GlobalVariable._user_com_port.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GlobalVariable._RENDER_WIDTH = Convert.ToDouble(render_width.Text);
                GlobalVariable._RENDER_HEIGHT = Convert.ToDouble(render_height.Text);
                GlobalVariable._user_danmaku_FontSize = Convert.ToInt32(danmaku_size.Text);
                GlobalVariable._user_danmaku_Duration = Convert.ToInt32(danmaku_duration.Text);

                GlobalVariable._user_danmaku_colorR = Convert.ToByte(danmaku_R.Text);
                GlobalVariable._user_danmaku_colorG = Convert.ToByte(danmaku_G.Text);
                GlobalVariable._user_danmaku_colorB = Convert.ToByte(danmaku_B.Text);

                GlobalVariable._user_danmaku_EnableShadow = danmaku_shadow.IsChecked.Value;

                GlobalVariable._user_audit = audit_mode.IsChecked.Value;
            }
            catch (Exception)
            {
                MessageBox.Show("存在无效的值, 部分设置将不会生效\r\n\r\nInput value Invalid,Some setting won't change");
            }
            
            if (Convert.ToInt32(com_port.Text) > 65535 || Convert.ToInt32(com_port.Text) < 1) {
                MessageBox.Show("端口无效, 端口设置将不会改变\r\n\r\nPort Invalid, port will not change");
            }
            else {
                GlobalVariable._user_com_port = Convert.ToInt32(com_port.Text);
            }

            this.Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
