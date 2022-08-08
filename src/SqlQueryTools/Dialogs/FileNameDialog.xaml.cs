using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SqlQueryTools.Dialogs
{
    /// <summary>
    /// Interaction logic for FileNameDialog.xaml
    /// </summary>
    public partial class FileNameDialog : Window
    {
        public FileNameDialog()
        {
            InitializeComponent();

            tError.Visibility = Visibility.Collapsed;
            tFileName.Text = FileName;
            tRemark.Foreground = lFileName.Foreground;
            tRemark.FontSize = lFileName.FontSize - 2;

            Loaded += FileNameDialog_Loaded;
            Unloaded += FileNameDialog_Unloaded;
        }

        private void FileNameDialog_Loaded(object sender, RoutedEventArgs e)
        {
            tFileName.Focus();
        }

        private void FileNameDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= FileNameDialog_Loaded;
            Unloaded -= FileNameDialog_Unloaded;
        }

        public string SqlFileSuffix { get; set; }
        public string FileName { get; set; }

        private void FileName_TextChanged(object sender, RoutedEventArgs e)
        {
            bAddFile.IsEnabled = true;
            tError.Text = string.Empty;
            tError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrWhiteSpace(tFileName.Text))
            {
                bAddFile.IsEnabled = false;
            }
            else
            {
                if (!char.IsLetter(tFileName.Text[0]))
                {
                    if (!string.IsNullOrWhiteSpace(tError.Text)) tError.Text += "\n";
                    tError.Text += "The filename should start with a letter.";
                    tError.Visibility = Visibility.Visible;
                    bAddFile.IsEnabled = false;
                }
            }
        }

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            FileName = tFileName.Text;

            if (char.IsLower(FileName[0]))
            {
                FileName = char.ToUpper(FileName[0]) + FileName.Remove(0, 1);
            }

            if (!FileName.EndsWith(SqlFileSuffix))
            {
                if (FileName.EndsWith(".sql"))
                {
                    FileName = FileName.Remove(FileName.Length - 4, 4);
                }

                FileName = $"{FileName}{SqlFileSuffix}";
            }

            DialogResult = true;
            Close();
        }

        public async static Task<string> GetFileNameAsync(string sqlFileSuffix)
        {
            var dialog = new FileNameDialog();
            dialog.SqlFileSuffix = sqlFileSuffix;

            var dialogResult = await VS.Windows.ShowDialogAsync(dialog);

            if (dialogResult.HasValue == false || dialogResult.Value == false) return string.Empty;

            return dialog.FileName;
        }
    }
}
