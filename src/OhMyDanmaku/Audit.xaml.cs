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
        public Audit()
        {
            InitializeComponent();
        }

        public void addToAuditList(string content)
        {
            this.Dispatcher.Invoke(new Action(() => {
                AuditList.Items.Add(content);
            }));
            
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
