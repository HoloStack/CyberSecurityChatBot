using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CybersecurityChatbot
{
    public partial class ReminderPopupWindow : Window
    {
        private readonly ReminderData _reminder;
        private DispatcherTimer? _autoCloseTimer;
        private const int AUTO_CLOSE_SECONDS = 30;

        public ReminderPopupWindow(ReminderData reminder)
        {
            InitializeComponent();
            _reminder = reminder;
            InitializeReminder();
            SetupAutoClose();
            PositionWindow();
        }

        private void InitializeReminder()
        {
            TaskTitleText.Text = _reminder.TaskTitle;
            ReminderTextBlock.Text = string.IsNullOrEmpty(_reminder.ReminderText) 
                ? "It's time to work on this cybersecurity task!" 
                : _reminder.ReminderText;
            
            TimeStampText.Text = $"Reminder set for: {_reminder.ReminderTime:MMM dd, yyyy h:mm tt}";
        }

        private void SetupAutoClose()
        {
            _autoCloseTimer = new DispatcherTimer();
            _autoCloseTimer.Interval = TimeSpan.FromSeconds(AUTO_CLOSE_SECONDS);
            _autoCloseTimer.Tick += AutoCloseTimer_Tick;
            _autoCloseTimer.Start();
        }

        private void PositionWindow()
        {
            // Position in the bottom-right corner
            var workingArea = SystemParameters.WorkArea;
            this.Left = workingArea.Right - this.Width - 20;
            this.Top = workingArea.Bottom - this.Height - 20;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Play slide-in animation
            var slideInStoryboard = (Storyboard)FindResource("SlideInAnimation");
            slideInStoryboard.Begin(MainBorder);

            // Play notification sound
            try
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch
            {
                // Ignore sound errors
            }
        }

        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            _autoCloseTimer.Stop();
            CloseWithAnimation();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWithAnimation();
        }

        private void DismissButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWithAnimation();
        }

        private void SnoozeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Snooze for 10 minutes
                var snoozeTime = DateTime.Now.AddMinutes(10);
                ReminderService.AddReminder(
                    _reminder.TaskTitle,
                    _reminder.ReminderText + " (Snoozed)",
                    snoozeTime
                );

                MessageBox.Show("Reminder snoozed for 10 minutes!", "Snoozed", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error snoozing reminder: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CloseWithAnimation();
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show completion message
                MessageBox.Show($"Great job completing '{_reminder.TaskTitle}'! Your cybersecurity is stronger now.", 
                    "Task Completed", MessageBoxButton.OK, MessageBoxImage.Information);

                // Log the completion
                var chatbotEngine = new ChatbotEngine(UserProfile.LoadProfile());
                chatbotEngine.LogActivity($"Completed reminder task: {_reminder.TaskTitle}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking task complete: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CloseWithAnimation();
        }

        private void CloseWithAnimation()
        {
            _autoCloseTimer?.Stop();

            var slideOutStoryboard = (Storyboard)FindResource("SlideOutAnimation");
            slideOutStoryboard.Completed += (s, e) => this.Close();
            slideOutStoryboard.Begin(MainBorder);
        }

        protected override void OnClosed(EventArgs e)
        {
            _autoCloseTimer?.Stop();
            base.OnClosed(e);
        }

        // Prevent window from stealing focus when it appears
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            var source = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            if (source != null)
            {
                source.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_MOUSEACTIVATE = 0x0021;
            const int MA_NOACTIVATE = 3;

            if (msg == WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(MA_NOACTIVATE);
            }

            return IntPtr.Zero;
        }
    }
}
