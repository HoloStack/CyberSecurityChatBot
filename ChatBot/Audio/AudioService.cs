using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace CybersecurityChatbot.Audio
{
    public static class AudioService
    {
        private static readonly string AudioDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Audio");
        
        public static async Task PlayGreetingAsync()
        {
            try
            {
                var greetingPath = Path.Combine(AudioDirectory, "greetings.wav");
                
                if (File.Exists(greetingPath))
                {
                    await Task.Run(() =>
                    {
                        using (var player = new SoundPlayer(greetingPath))
                        {
                            player.LoadAsync();
                            player.Play();
                        }
                    });
                }
                else
                {
                    // If greetings.wav doesn't exist, play system sound as fallback
                    await Task.Run(() => SystemSounds.Asterisk.Play());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing greeting sound: {ex.Message}");
                // Fallback to system sound on error
                try
                {
                    SystemSounds.Asterisk.Play();
                }
                catch
                {
                    // Silently fail if even system sounds don't work
                }
            }
        }
        
        public static void PlayGreeting()
        {
            try
            {
                var greetingPath = Path.Combine(AudioDirectory, "greetings.wav");
                
                if (File.Exists(greetingPath))
                {
                    using (var player = new SoundPlayer(greetingPath))
                    {
                        player.Load();
                        player.Play();
                    }
                }
                else
                {
                    // If greetings.wav doesn't exist, play system sound as fallback
                    SystemSounds.Asterisk.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing greeting sound: {ex.Message}");
                // Fallback to system sound on error
                try
                {
                    SystemSounds.Asterisk.Play();
                }
                catch
                {
                    // Silently fail if even system sounds don't work
                }
            }
        }
        
        public static void PlaySuccessSound()
        {
            try
            {
                SystemSounds.Exclamation.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing success sound: {ex.Message}");
            }
        }
        
        public static void PlayErrorSound()
        {
            try
            {
                SystemSounds.Hand.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing error sound: {ex.Message}");
            }
        }
        
        public static void PlayNotificationSound()
        {
            try
            {
                SystemSounds.Beep.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing notification sound: {ex.Message}");
            }
        }
    }
}
