using Microsoft.Win32;
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
using static ModernGfxRip.KeyHandlers;

namespace ModernGfxRip
{
    /// <summary>
    /// Interaction logic for WPFMenus.xaml
    /// </summary>
    public partial class WPFMenus : Window
    {
        // Configuration Settings Filename
        private string gfxRipCfgName;

        // Graphics Ripper Variables Configured
        public bool GRActive { set; get; }

        // Binary Data to view is loaded
        public bool BinLoaded { set; get; }

        public WPFMenus()
        {
            InitializeComponent();

            // Initialize variables before sharing them
            GRActive = false;
            BinLoaded = false;

            // Set up the name of the configuration file
            gfxRipCfgName = "undefined";

            this.DataContext = new KeyCommandsContext(this);
        }

        public void ExitProgram()
        {
            Application.Current.Shutdown();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            ExitProgram();
        }

        private void CommandBindingNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("New MenuItem Clicked.");

            // GfxRip variables are now initialized
            GRActive = true;

            gfxRipCfgName = "undefined";
        }

        private void CommandBindingOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Title = string.Format("Select GfxRip Config File"),
                Filter = "GfxRip Config files (*.cfg)|*.cfg|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                gfxRipCfgName = openFileDialog.FileName;

                // GfxRip variables are now initialized
                GRActive = true;

                MessageBox.Show("Open GfxRip Config file from " + openFileDialog.FileName);
            }
        }

        private void CommandBindingSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GRActive;
        }

        private void CommandBindingSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // No save filename specified so call SaveAs instead
            if (gfxRipCfgName == "undefined")
            {
                CommandBindingSaveAs_Executed(sender, e);
            }
            else
            {
                MessageBox.Show("Save MenuItem Clicked.");
            }
        }

        private void CommandBindingSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GRActive;
        }

        private void CommandBindingSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Title = string.Format("Save GfxRip Config File"),
                DefaultExt = string.Format(".cfg"),
                Filter = "GfxRip Config files (*.cfg)|*.cfg|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AddExtension = true,
                FileName = string.Format("GfxRip")
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                gfxRipCfgName = saveFileDialog.FileName;

                MessageBox.Show("Save As MenuItem Clicked.  Save to " + gfxRipCfgName);
            }
        }

        protected void ToolsExit_Click(object sender, RoutedEventArgs args)
        {
            ExitProgram();
        }

        protected void ToolsSpellingHints_Click(object sender, RoutedEventArgs args)
        {
        }

        protected void MouseEnterExitArea(object sender, RoutedEventArgs args)
        {
            statBarText.Text = "Exit the Application";
        }

        protected void MouseEnterToolsHintsArea(object sender, RoutedEventArgs args)
        {
            statBarText.Text = "Spelling Suggestions";
        }
        protected void MouseLeaveArea(object sender, RoutedEventArgs args)
        {
            statBarText.Text = "Ready";
        }

    }
}
