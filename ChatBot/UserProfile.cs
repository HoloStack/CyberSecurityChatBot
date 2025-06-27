using System;
using System.IO;
using System.Text.Json;

namespace CybersecurityChatbot
{
    public class UserProfile
    {
        public string Name { get; set; } = "";
        public string FavoriteSubject { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastActiveDate { get; set; } = DateTime.Now;

        private const string ProfileFileName = "Data/user_profile.json";

        public static UserProfile LoadProfile()
        {
            try
            {
                if (File.Exists(ProfileFileName))
                {
                    var jsonString = File.ReadAllText(ProfileFileName);
                    var profile = JsonSerializer.Deserialize<UserProfile>(jsonString);
                    if (profile != null)
                    {
                        profile.LastActiveDate = DateTime.Now;
                        profile.SaveProfile(); // Update last active
                        return profile;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user profile: {ex.Message}");
            }
            
            return new UserProfile();
        }

        public void SaveProfile()
        {
            try
            {
                EnsureDataDirectory();
                var jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(ProfileFileName, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving user profile: {ex.Message}");
            }
        }

        public bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(FavoriteSubject);
        }

        private static void EnsureDataDirectory()
        {
            var dataDir = Path.GetDirectoryName(ProfileFileName);
            if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
        }

        public string GetGreeting()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "Hello there!";
            
            var timeOfDay = DateTime.Now.Hour switch
            {
                < 12 => "Good morning",
                < 17 => "Good afternoon", 
                _ => "Good evening"
            };
            
            return $"{timeOfDay}, {Name}!";
        }

        public string GetFavoriteSubjectResponse()
        {
            if (string.IsNullOrWhiteSpace(FavoriteSubject))
                return "";

            return FavoriteSubject.ToLower() switch
            {
                "passwords" or "password" => $"🌟 Ah, {Name}! I see you're asking about {FavoriteSubject} - your favorite cybersecurity topic! You have excellent taste. Strong passwords are the foundation of digital security. Here's something special for you:",
                "phishing" => $"🌟 {Name}, you're asking about {FavoriteSubject} - your favorite area! Your interest in phishing awareness shows you really understand the human element of cybersecurity. Here's a personalized tip:",
                "malware" => $"🌟 Hey {Name}! {FavoriteSubject} is such a fascinating area - no wonder it's your favorite! Your interest in malware protection shows you're thinking like a true cybersecurity professional:",
                "2fa" or "two-factor authentication" => $"🌟 {Name}, I love that {FavoriteSubject} is your favorite topic! Two-factor authentication is one of the most effective security measures. Since you're so interested in this area:",
                "vpn" => $"🌟 Perfect choice, {Name}! {FavoriteSubject} technology is absolutely crucial for privacy and security. Your interest in VPNs shows you really value your digital privacy:",
                "encryption" => $"🌟 Excellent choice, {Name}! {FavoriteSubject} is the mathematical foundation of cybersecurity. Your fascination with encryption shows real depth of understanding:",
                "social engineering" => $"🌟 {Name}, your interest in {FavoriteSubject} shows you understand that cybersecurity is as much about psychology as technology. Smart choice for a favorite topic:",
                "network security" => $"🌟 Great choice, {Name}! {FavoriteSubject} is where the real action happens in cybersecurity. Your interest in network security shows you think systematically:",
                "incident response" => $"🌟 {Name}, {FavoriteSubject} is such a critical area! Your interest shows you understand that it's not just about prevention, but also about rapid response:",
                "risk assessment" => $"🌟 {Name}, choosing {FavoriteSubject} as your favorite shows real strategic thinking! Risk assessment is the foundation of any good security program:",
                _ => $"🌟 {Name}, I love that {FavoriteSubject} is your favorite cybersecurity topic! Your passion for this area really shows. Here's something special about {FavoriteSubject}:"
            };
        }
    }
}
