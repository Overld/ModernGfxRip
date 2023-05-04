using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ModernGfxRip
{
    /// <summary>
    /// Interaction logic for NumberEntryDialog.xaml
    /// </summary>
    public partial class NumberEntryDialog : Window
    {
        public NumberEntryDialog(string question, long offset)
        {
            InitializeComponent();

            Title = question;
            txtNumber.Text = offset.ToString();
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
