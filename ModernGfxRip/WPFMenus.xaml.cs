using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        // GfxRip Conversion Utilities
        private readonly GfxRip gfxRip;

        // Graphics Ripper Variables Configured
        public bool GRActive { set; get; }

        // Binary Data to view is loaded
        public bool BinLoaded { set; get; }

        // Zoom variable
        public bool ZoomStatus
        {
            set
            {
                gfxRip.Config.Zoom = value;
                ZoomMenu.IsChecked = value;

                // Force Screen to Redraw
                gfxRip.isDirty = true;
                gfxRip.Refresh();
            }

            get
            {
                return gfxRip.Config.Zoom;
            }
        }

        public WPFMenus()
        {
            InitializeComponent();

            gfxRip = new GfxRip();

            // Initialize variables before sharing them
            GRActive = false;
            BinLoaded = false;

            // Set up the name of the configuration file
            gfxRipCfgName = "undefined";

            this.DataContext = new KeyCommandsContext(this);

            // Setup Memory for Drawing Purposes
            gfxRip.SetupDrawingBitmap(graphics, (int)mainCanvas.Width, (int)mainCanvas.Height);

            // Put default text in status bar
            statBarText.Text = "Ready";

            // Start a New Session
            // GfxRip variables are now initialized
            GRActive = true;

            gfxRipCfgName = "undefined";

            gfxRip.NewConfiguration();

            UpdateStatusBar();
        }

        public void ExitProgram()
        {
            // Configuration has not been saved
            // Prompt User to see if they want to save
            if (WarnDialog() == false)
            {
                // Abort exiting
                return;
            }

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
            if (WarnDialog() == false)
            {
                // User has aborted
                return;
            }

            // GfxRip variables are now initialized
            GRActive = true;

            gfxRipCfgName = "undefined";

            gfxRip.NewConfiguration();

            UpdateStatusBar();
        }

        private void CommandBindingOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (WarnDialog() == false)
            {
                // User has aborted
                return;
            }

            OpenFileDialog openFileDialog = new()
            {
                Title = string.Format("Select GfxRip Config File"),
                Filter = "GfxRip Config files (*.cfg)|*.cfg|All files (*.*)|*.*"
                // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                gfxRipCfgName = openFileDialog.FileName;

                // GfxRip variables are now initialized
                GRActive = true;

                gfxRip.LoadConfiguration(openFileDialog.FileName);

                UpdateStatusBar();
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
                // Save the configuration
                gfxRip.SaveConfiguration(gfxRipCfgName);
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
                // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AddExtension = true,
                FileName = string.Format("GfxRip")
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                gfxRipCfgName = saveFileDialog.FileName;

                // Save the configuration
                gfxRip.SaveConfiguration(gfxRipCfgName);
            }
        }
        private void CommandBindingCopy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = BinLoaded;
        }

        private void CommandBindingCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Copy the first square to the copy buffer
            gfxRip.CopyToClipboard();
        }

        public void ModifyImageSize(string? command)
        {
            if (BinLoaded)
            {
                gfxRip.AdjustImageSize(command);

                UpdateStatusBar();
            }
        }

        public void ModifyPictureSize(string? command)
        {
            if (BinLoaded)
            {
                gfxRip.AdjustPictureSize(command);

                UpdateStatusBar();
            }
        }
        public void ModifyPalettes(string? command)
        {
            if (BinLoaded)
            {
                gfxRip.ModifyPalettes(command);

                UpdateStatusBar();
            }
        }


        public void ModifyBitplanes(string? command)
        {
            if (BinLoaded)
            {
                gfxRip.ModifyBitplanes(command);

                UpdateStatusBar();
            }
        }

        /// <summary>
        /// Displays a warning dialog to the user that the configuration file was changed, and do you want to lose your changes?
        /// </summary>
        /// <returns>true if user agrees to lose changes</returns>
        protected bool WarnDialog()
        {
            bool result = true;

            // Configuration is set up so check to see if User wants to override changes
            if (GRActive)
            {
                // Configuration has not been saved so warn User.
                if ((gfxRip != null) && (!gfxRip.IsSaved))
                {
                    // Configuration has not been saved
                    // Prompt User to see if they want to save
                    MessageBoxResult messageBox = MessageBox.Show("Configuration has been changed.\nDo you want to exit and lose your changes?",
                                              "Confirmation",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Question);
                    if (messageBox == MessageBoxResult.No)
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        public bool LoadBinaryData(string filename)
        {
            return gfxRip.ReadBinaryData(filename);
        }

        public void GetBMPInfo(ref BMPFileInfo bmpInfo)
        {
            gfxRip.GetPaletteInfo(ref bmpInfo);
        }

        public long ConfigOffset
        {
            get
            {
                if (gfxRip != null)
                {
                    return gfxRip.Config.Offset;
                }
                return 0;
            }
            set
            { 
                if (gfxRip != null) 
                { 
                    gfxRip.Config.Offset = value;

                    // Update the screen
                    gfxRip.isDirty = true;
                    gfxRip.Refresh();

                    // Update Status Bar
                    UpdateStatusBar();
                }
            }
        }

        public long ConfigSkip
        {
            get
            {
                if (gfxRip != null)
                {
                    return gfxRip.Config.Skip;
                }
                return 0;
            }
            set
            {
                if (gfxRip != null)
                {
                    gfxRip.Config.Skip = value;

                    // Update the screen
                    gfxRip.isDirty = true;
                    gfxRip.Refresh();

                    // Update Status Bar
                    UpdateStatusBar();
                }
            }
        }

        /// <summary>
        /// Place latest Configuration values in the Status Bar
        /// </summary>
        protected void UpdateStatusBar()
        {
            // Update the Status Bar with Latest Settings
            statBarText.Text = gfxRip.Config.DisplayValues();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
