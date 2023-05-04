using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModernGfxRip
{
    internal class KeyHandlers
    {
        public class ExitKey : ICommand
        {
            private readonly WPFMenus menu;

            public ExitKey(WPFMenus menu)
            {
                this.menu = menu;
            }

            public event EventHandler? CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                menu.ExitProgram();
            }
        }

        public class LoadBinary : ICommand
        {
            private readonly WPFMenus menu;

            public LoadBinary(WPFMenus menu)
            {
                this.menu = menu;
            }

            public event EventHandler? CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(object? parameter)
            {
                return menu.GRActive;
            }

            public void Execute(object? parameter)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = string.Format("Select Binary File"),
                    Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*"
                    // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    if (menu.LoadBinaryData(openFileDialog.FileName))
                    {
                        // Binary Data has been loaded so enable all other menu options
                        menu.BinLoaded = true;
                    }
                }
            }
        }

        public class OffsetKey : ICommand
        {
            private readonly WPFMenus menu;

            public OffsetKey(WPFMenus menu)
            {
                this.menu = menu;
            }

            public event EventHandler? CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(object? parameter)
            {
                return menu.BinLoaded;
            }

            public void Execute(object? parameter)
            {
                NumberEntryDialog dialog = new("Go to BYTE:", menu.ConfigOffset);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        menu.ConfigOffset = Int32.Parse(dialog.txtNumber.Text);
                    }
                    catch (FormatException e)
                    {
                        throw e;
                    }
                }
            }
        }

        public class SkipValueKey : ICommand
        {
            private readonly WPFMenus menu;

            public SkipValueKey(WPFMenus menu)
            {
                this.menu = menu;
            }

            public event EventHandler? CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(object? parameter)
            {
                return menu.BinLoaded;
            }

            public void Execute(object? parameter)
            {
                NumberEntryDialog dialog = new("Skip Value:", menu.ConfigSkip);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        menu.ConfigSkip = Int32.Parse(dialog.txtNumber.Text);
                    }
                    catch (FormatException e)
                    {
                        throw e;
                    }
                }
            }
        }

        public class SavePictureKey : ICommand
        {
            private readonly WPFMenus menu;

            public SavePictureKey(WPFMenus menu)
            {
                this.menu = menu;
            }

            public event EventHandler? CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(object? parameter)
            {
                return menu.BinLoaded;
            }

            public void Execute(object? parameter)
            {
                SavePictureDialog dialog = new(false);
                if (dialog.ShowDialog() == true)
                {
                    string? radioButton;

                    if (dialog.rb1.IsChecked== true)
                    {
                        radioButton = dialog.rb1.Content.ToString();
                    } else
                    {
                        radioButton = dialog.rb2.Content.ToString();
                    }

                    MessageBox.Show("Save Picture Values: filename=" + dialog.fileName.Text + ", X Pics=" + dialog.xPicText.Text +
                        ", Y Pics=" + dialog.xPicText.Text + "\nborder=" + dialog.borderText.Text + ", direction=" + radioButton +
                        ", Color=" + dialog.colorText.Text);
                }
            }
        }

        public class SavePictureAutoIncKey : ICommand
        {
            private readonly WPFMenus menu;

            public SavePictureAutoIncKey(WPFMenus menu)
            {
                this.menu = menu;
            }

            public event EventHandler? CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(object? parameter)
            {
                return menu.BinLoaded;
            }

            public void Execute(object? parameter)
            {
                SavePictureDialog dialog = new(true);
                if (dialog.ShowDialog() == true)
                {
                    string? radioButton;

                    if (dialog.rb1.IsChecked == true)
                    {
                        radioButton = dialog.rb1.Content.ToString();
                    }
                    else
                    {
                        radioButton = dialog.rb2.Content.ToString();
                    }

                    MessageBox.Show("Save Picture Values: filename=" + dialog.fileName.Text + ", X Pics=" + dialog.xPicText.Text +
                        ", Y Pics=" + dialog.xPicText.Text + "\nborder=" + dialog.borderText.Text + ", direction=" + radioButton +
                        ", Color=" + dialog.colorText.Text);
                }
            }
        }

        public class ZoomWindowKey : ICommand
        {
            // Get Menus Status
            private readonly WPFMenus menu;
            // See if Zoom window is Active
            private bool zoomActive;

            public ZoomWindowKey(WPFMenus menu)
            {
                this.menu = menu;
                this.zoomActive = false;
            }

            public event EventHandler? CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(object? parameter)
            {
                return menu.BinLoaded;
            }

            public void Execute(object? parameter)
            {
                this.zoomActive = !this.zoomActive;

                MessageBox.Show("Toggle Zoom Wimdow: visible=" + this.zoomActive);
            }
        }

        public class GetColorKey : ICommand
        {
            private readonly WPFMenus menu;

            public GetColorKey(WPFMenus menu)
            {
                this.menu = menu;
            }

            public event EventHandler? CanExecuteChanged {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(Object? parameter)
            {
                return menu.BinLoaded;
            }

            public void Execute(Object? parameter)
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = string.Format("Select BMP File"),
                    Filter = "BMP graphic files (*.bmp)|*.bmp|All files (*.*)|*.*"
                    // InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    MessageBox.Show("Get Color Palette from " + openFileDialog.FileName);
                }
            }
        }

        public class ImageSize : ICommand
        {
            private readonly WPFMenus menu;

            public ImageSize(WPFMenus menu)
            {
                this.menu = menu;
            }

            public event EventHandler? CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(object? parameter)
            {
                return menu.BinLoaded;
            }

            public void Execute(object? parameter)
            {
                menu.ModifyImageSize((string?) parameter);
            }
        }
        public class PictureSize : ICommand
        {
            private readonly WPFMenus menu;

            public PictureSize(WPFMenus menu)
            {
                this.menu = menu;
            }

            public event EventHandler? CanExecuteChanged
            {
                add
                {
                    CommandManager.RequerySuggested += value;
                }
                remove
                {
                    CommandManager.RequerySuggested -= value;
                }
            }

            public bool CanExecute(object? parameter)
            {
                return menu.BinLoaded;
            }

            public void Execute(object? parameter)
            {
                menu.ModifyPictureSize((string?) parameter);
            }
        }

        public class KeyCommandsContext
        {
            // Access to menus
            private readonly WPFMenus menus;

            public KeyCommandsContext(WPFMenus menus)
            {
                this.menus = menus;
            }

            public ICommand ExitCommand
            {
                get
                {
                    return new ExitKey(menus);
                }
            }

            public ICommand LoadCommand
            {
                get
                {
                    return new LoadBinary(menus);
                }
            }

            public ICommand OffsetCommand
            {
                get
                {
                    return new OffsetKey(menus);
                }
            }

            public ICommand SkipValueCommand
            {
                get
                {
                    return new SkipValueKey(menus);
                }
            }

            public ICommand GetColorCommand
            {
                get
                {
                    return new GetColorKey(menus);
                }
            }

            public ICommand SavePictureCommand
            {
                get
                {
                    return new SavePictureKey(menus);
                }
            }

            public ICommand SavePictureAutoIncCommand
            {
                get
                {
                    return new SavePictureAutoIncKey(menus);
                }
            }

            public ICommand ZoomWindowCommand
            {
                get
                {
                    return new ZoomWindowKey(menus);
                }
            }

            public ICommand ImageSizeCommand
            {
                get
                {
                    return new ImageSize(menus);
                }
            }

            public ICommand PictureSizeCommand
            {
                get
                {
                    return new PictureSize(menus);
                }
            }
        }
    }
}
