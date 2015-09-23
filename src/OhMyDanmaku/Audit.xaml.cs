using System;
using System.Windows;
using System.Windows.Input;

namespace OhMyDanmaku
{
    /// <summary>
    /// Interaction logic for Audit.xaml
    /// </summary>
    public partial class Audit : Window
    {
        MainWindow mw;
        public Audit(MainWindow that)
        {
            InitializeComponent();
            mw = that;
        }

        public void addToAuditList(string content)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (currentDanmaku.Text == string.Empty)
                {
                    currentDanmaku.Text = content;
                    setButtons(true);
                }
                else
                {
                    AuditList.Items.Add(content);

                }
            }));
        }

        private void getADanmaku()
        {
            if (AuditList.Items.Count == 0)
            {
                currentDanmaku.Text = "";
                setButtons(false);
            }
            else
            {
                currentDanmaku.Text = AuditList.Items[0].ToString();
                AuditList.Items.RemoveAt(0);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void passButton_Click(object sender, RoutedEventArgs e)
        {
            mw.sendDanmaku(currentDanmaku.Text);
            getADanmaku();
        }

        private void setButtons(bool on)
        {
            passButton.IsEnabled = on;
            dropButton.IsEnabled = on;
        }

        private void dropButton_Click(object sender, RoutedEventArgs e)
        {
            getADanmaku();
        }
    }
}
