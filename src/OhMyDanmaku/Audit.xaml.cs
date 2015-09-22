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

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
