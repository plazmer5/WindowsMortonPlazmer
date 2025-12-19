using System.Windows;

namespace MortonPlazmer
{
    public partial class CustomDialog : Window
    {
        public string Message { get; set; }
        public bool Result { get; private set; }

        public CustomDialog(string title, string message)
        {
            InitializeComponent();
            Title = title;
            Message = message;
            DataContext = this;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
