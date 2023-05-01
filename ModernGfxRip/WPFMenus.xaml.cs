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
        public WPFMenus()
        {
            InitializeComponent();
            this.DataContext = new KeyCommandsContext();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CommandBindingNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("New MenuItem Clicked.");
        }

        private void CommandBindingOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Open MenuItem Clicked.");
        }

        private void CommandBindingSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Save MenuItem Clicked.");
        }

        private void CommandBindingSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Save As MenuItem Clicked.");
        }

        protected void ToolsExit_Click(object sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
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
