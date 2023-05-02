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
    /// Interaction logic for SavePictureDialog.xaml
    /// </summary>
    public partial class SavePictureDialog : Window
    {
        public SavePictureDialog(Boolean autoIncrement)
        {
            string baseFileName = "p.bmp";

            InitializeComponent();

            if (autoIncrement)
            {
                baseFileName = "p000.bmp";
            }

            fileName.Text = baseFileName;
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            RadioButton? rb = sender as RadioButton;
        }
    }
}
