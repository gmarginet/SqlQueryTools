using System.Windows;

namespace SqlQueryTools
{
    public partial class FileNameDialog : Window
    {
        public FileNameDialog()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                txtName.Focus();
            };
        }

        public string Input => txtName.Text.Trim();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
