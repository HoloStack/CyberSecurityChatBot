using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CybersecurityChatbot
{
    public partial class UserSetupDialog : Window
    {
        public UserProfile UserProfile { get; private set; } = new UserProfile();

        public UserSetupDialog()
        {
            InitializeComponent();
            
            // Clear existing content and set up proper input for name field
            NameRichTextBox.Document.Blocks.Clear();
            NameRichTextBox.Document.Blocks.Add(new Paragraph(new Run("")));
            
            // Set focus to name field and add keyboard support
            NameRichTextBox.Focus();
            NameRichTextBox.KeyDown += NameRichTextBox_KeyDown;
            
            // Make Next button the default button
            NextButton.IsDefault = true;
        }
        
        private void NameRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                NextButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var nameText = GetRichTextBoxText(NameRichTextBox);
            if (string.IsNullOrWhiteSpace(nameText))
            {
                MessageBox.Show("Please enter your name to continue.", "Name Required", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NameRichTextBox.Focus();
                return;
            }

            UserProfile.Name = nameText.Trim();
            
            // Determine favorite subject from radio buttons
            if (PasswordsRadio.IsChecked == true)
                UserProfile.FavoriteSubject = "passwords";
            else if (PhishingRadio.IsChecked == true)
                UserProfile.FavoriteSubject = "phishing";
            else if (MalwareRadio.IsChecked == true)
                UserProfile.FavoriteSubject = "malware";
            else if (TwoFARadio.IsChecked == true)
                UserProfile.FavoriteSubject = "2fa";
            else if (VPNRadio.IsChecked == true)
                UserProfile.FavoriteSubject = "vpn";
            else if (EncryptionRadio.IsChecked == true)
                UserProfile.FavoriteSubject = "encryption";
            else if (IncidentRadio.IsChecked == true)
                UserProfile.FavoriteSubject = "incident response";

            UserProfile.SaveProfile();

            DialogResult = true;
            Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Legacy method - redirect to NextButton_Click
            NextButton_Click(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Create minimal profile
            UserProfile.Name = "User";
            UserProfile.FavoriteSubject = "general security";
            
            DialogResult = false;
            Close();
        }

        private void CreateUserLink_Click(object sender, MouseButtonEventArgs e)
        {
            // Call the same logic as SaveButton_Click
            SaveButton_Click(sender, new RoutedEventArgs());
        }

        private void CreateUserLink_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Parent is Border border)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(0x1E, 0x8B, 0x44)); // Darker green
            }
        }

        private void CreateUserLink_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.Parent is Border border)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(0x27, 0xAE, 0x60)); // Original green
            }
        }

        private string GetRichTextBoxText(RichTextBox richTextBox)
        {
            return new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text.Trim();
        }
    }
}
