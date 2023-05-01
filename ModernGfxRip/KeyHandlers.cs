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
                Application.Current.Shutdown();
            }
        }

        public class LoadBinary : ICommand
        {
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
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = string.Format("Select Binary File");
                openFileDialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    MessageBox.Show("Load Binary Data from " + openFileDialog.FileName);
                }
            }
        }

        public class OffsetKey : ICommand
        {
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
                NumberEntryDialog dialog = new NumberEntryDialog("Go to BYTE:");
                if (dialog.ShowDialog() == true)
                {
                    MessageBox.Show("Go to Byte = " + dialog.txtNumber.Text);
                }
            }
        }
        public class SkipValueKey : ICommand
        {
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
                NumberEntryDialog dialog = new NumberEntryDialog("Skip Value:");
                if (dialog.ShowDialog() == true)
                {
                    MessageBox.Show("Set Skip Value = " + dialog.txtNumber.Text);
                }
            }
        }

        public class ZoomWindowKey : ICommand
        {
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
                MessageBox.Show("Toggle Zoom Wimdow");
            }
        }

        public class GetColorKey : ICommand
        {
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
                return true;
            }

            public void Execute(Object? parameter)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = string.Format("Select BMP File");
                openFileDialog.Filter = "BMP graphic files (*.bmp)|*.bmp|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    MessageBox.Show("Get Color Palette from " + openFileDialog.FileName);
                }
            }
        }

        public class KeyCommandsContext
        {
            public ICommand ExitCommand
            {
                get
                {
                    return new ExitKey();
                }
            }

            public ICommand LoadCommand
            {
                get
                {
                    return new LoadBinary();
                }
            }

            public ICommand OffsetCommand
            {
                get
                {
                    return new OffsetKey();
                }
            }

            public ICommand SkipValueCommand
            {
                get
                {
                    return new SkipValueKey();
                }
            }
            public ICommand GetColorCommand
            {
                get
                {
                    return new GetColorKey();
                }
            }


            public ICommand ZoomWindowCommand
            {
                get
                {
                    return new ZoomWindowKey();
                }
            }
        }
    }
}
