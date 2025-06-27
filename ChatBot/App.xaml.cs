using System;
using System.Windows;

namespace CybersecurityChatbot
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Always start the reminder service
            ReminderService.StartService();
            
            // Check for console mode argument
            foreach (string arg in e.Args)
            {
                if (arg.Equals("--console", StringComparison.OrdinalIgnoreCase) || 
                    arg.Equals("-c", StringComparison.OrdinalIgnoreCase))
                {
                    // Hide the main window and run in console mode
                    this.MainWindow = null;
                    
                    // Start console application
                    var consoleThread = new System.Threading.Thread(() =>
                    {
                        // Allocate console for this application
                        AllocConsole();
                        
                        // Run the console version
                        Program.Main(e.Args);
                        
                        // Exit the application when console closes
                        Environment.Exit(0);
                    });
                    
                    consoleThread.SetApartmentState(System.Threading.ApartmentState.STA);
                    consoleThread.Start();
                    
                    return; // Don't call base.OnStartup to prevent GUI from starting
                }
                else if (arg.Equals("--background", StringComparison.OrdinalIgnoreCase) ||
                         arg.Equals("-b", StringComparison.OrdinalIgnoreCase))
                {
                    // Run in background mode (service only)
                    this.MainWindow = null;
                    this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    
                    // Setup auto-start if not already configured
                    ReminderService.SetupAutoStart();
                    
                    return; // Don't show GUI, just run service
                }
            }
            
            // Default to GUI mode
            base.OnStartup(e);
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            // Stop the reminder service when application exits
            ReminderService.StopService();
            base.OnExit(e);
        }
        
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        static extern bool AllocConsole();
    }
}
