using System.Collections.Generic;
using System.Windows;

namespace CybersecurityChatbot
{
    public partial class ActivityLogWindow : Window
    {
        public ActivityLogWindow(List<ActivityLogEntry> logs)
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
