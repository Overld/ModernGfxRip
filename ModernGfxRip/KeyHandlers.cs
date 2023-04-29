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
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                Application.Current.Shutdown();
            }
        }

        public class OffsetKey : ICommand
        {
            public event EventHandler? CanExecuteChanged;

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
            public event EventHandler? CanExecuteChanged;

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
            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                MessageBox.Show("Toggle Zoom Wimdow");
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
