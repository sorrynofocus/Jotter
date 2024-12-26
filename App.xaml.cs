using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System;
using System.Runtime.InteropServices;


namespace Jotter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex mutex;

        public static class NativeMethods
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetForegroundWindow(IntPtr hWnd);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            string mutexName = $"JotterAppMutex_{Environment.UserName}";
            //calling process created the mutex or an existing one was found?
            //if false: another instance of the application is running
            bool createdNew;

            mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                //MessageBox.Show("Another instance of Jotter is already running.", "Jotter", MessageBoxButton.OK, MessageBoxImage.Information);
                //Application.Current.Shutdown();

                //Do interprocess communication to find the instance, and set it to focus
                System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                foreach (var process in System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName))
                {
                    if (process.Id != currentProcess.Id)
                    {
                        // Bring the existing process window to the foreground
                        NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                        break;
                    }
                }

                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.ReleaseMutex();
            base.OnExit(e);
        }

    }

}
