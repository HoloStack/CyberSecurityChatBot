# 🛡️ Cybersecurity Awareness Chatbot

A comprehensive cybersecurity education and task management tool built with .NET 8 and C#. This interactive console application helps users learn cybersecurity best practices while managing their security-related tasks.

## ✨ Features

### 📝 Task Management System
- **Add cybersecurity tasks** with detailed descriptions
- **Set reminders** for important security actions
- **Track progress** with task completion and deletion
- **Smart task suggestions** based on cybersecurity best practices

### 🎯 Interactive Quiz System
- **10 comprehensive cybersecurity questions** covering:
  - Phishing detection and prevention
  - Password security and best practices
  - Two-Factor Authentication (2FA)
  - VPN usage and public Wi-Fi safety
  - Malware and ransomware awareness
  - Social engineering tactics
  - Software updates and security patches
- **Multiple-choice and true/false formats**
- **Immediate feedback** with detailed explanations
- **Score tracking** and performance evaluation

### 🧠 Natural Language Processing (NLP) Simulation
- **Keyword detection** for cybersecurity topics
- **Intent recognition** through string manipulation
- **Flexible input handling** - understands variations in user requests
- **Context-aware responses** based on detected keywords

### 😊 Sentiment Analysis
- **Emotion detection** for worried, frustrated, and positive sentiments
- **Empathetic responses** tailored to user's emotional state
- **Supportive guidance** for cybersecurity anxiety

### 📊 Activity Logging
- **Comprehensive tracking** of all user interactions
- **Timestamped logs** for tasks, quizzes, and conversations
- **Activity review** functionality
- **Log management** with clear option

### 🎨 Modern Console Interface
- **Colorful, emoji-rich design** for enhanced user experience
- **Clear formatting** with professional styling
- **Intuitive command structure** with comprehensive help system
- **Cross-platform compatibility** (Windows, macOS, Linux)

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK or later
- Git (for cloning the repository)

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/HoloStack/CyberSecurityChatBot.git
   cd CyberSecurityChatBot
   ```

2. **Navigate to the ChatBot directory:**
   ```bash
   cd ChatBot
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

## 🎮 Usage Guide

### Basic Commands

#### Task Management
- `add task [description]` - Add a new cybersecurity task
- `show tasks` - View all current tasks
- `complete task` - Mark a task as completed
- `delete task` - Remove a task

#### Quiz System
- `start quiz` - Begin the cybersecurity knowledge quiz
- Quiz includes 5 random questions from a pool of 10

#### Activity Tracking
- `activity log` - View recent activity
- `clear log` - Clear the activity log

#### Smart Conversations
- Ask about **cybersecurity topics**: passwords, phishing, malware, 2FA, VPN, etc.
- Express **feelings**: worried, frustrated, excited, etc.
- Get **contextual responses** and helpful tips

#### General
- `help` - Show all available commands
- `exit` - Close the chatbot

### Example Interactions

```
👤 You: add task enable 2FA on email account
🤖 Bot: ✅ Task added: 'enable 2FA on email account'. Would you like to set a reminder? (Type 'yes' or 'no')

👤 You: start quiz
🎯 CYBERSECURITY KNOWLEDGE QUIZ
This quiz has 10 questions. Let's test your cybersecurity knowledge!

👤 You: I'm worried about online security
🤖 Bot: I understand you're feeling worried, User. Let's take cybersecurity step-by-step to make it less overwhelming. Would you like to start with a simple task?

👤 You: tell me about phishing
🤖 Bot: 💡 Phishing is a key concept in cybersecurity. Always take precautions to avoid phishing related risks.
```

## 🏗️ Project Structure

```
CyberSecurityChatBot/
├── ChatBot/
│   ├── Program.cs              # Main application logic
│   ├── ResponcesList.cs        # Cybersecurity knowledge base
│   ├── ChatBot.csproj          # Project configuration
│   └── bin/Debug/net8.0/       # Build output (ignored by git)
├── .gitignore                  # Git ignore rules
└── README.md                   # This file
```

## 🔧 Technical Details

### Architecture
- **Console Application** built with .NET 8
- **Object-Oriented Design** with clean separation of concerns
- **Cross-platform compatibility** using .NET Core
- **Modular structure** for easy maintenance and extension

### Key Classes
- `Program` - Main application controller
- `TaskData` - Task information container
- `ActivityLogEntry` - Activity logging model
- `Question` - Quiz question structure
- `ResponcesList` - Cybersecurity knowledge database

### Features Implementation
- **Task Management**: List-based storage with CRUD operations
- **Quiz System**: Queue-based question delivery with scoring
- **NLP Simulation**: String manipulation and keyword matching
- **Sentiment Analysis**: Pattern recognition in user input
- **Activity Logging**: Timestamped event tracking

## 🎯 Educational Objectives

This project fulfills all assignment requirements:

### ✅ Task 1: Task Assistant with Reminders
- Integrated task management system
- Reminder functionality
- Task lifecycle management (add, view, complete, delete)

### ✅ Task 2: Cybersecurity Mini-Game (Quiz)
- 10 educational questions covering key security topics
- Immediate feedback with explanations
- Score tracking and performance evaluation

### ✅ Task 3: Natural Language Processing (NLP) Simulation
- Keyword detection and intent recognition
- Flexible input handling
- Context-aware response generation

### ✅ Task 4: Activity Log Feature
- Comprehensive activity tracking
- User action logging with timestamps
- Log viewing and management capabilities

## 🛡️ Cybersecurity Topics Covered

The chatbot provides information and guidance on:

- **Authentication**: Passwords, 2FA, biometrics
- **Email Security**: Phishing, spam, safe practices
- **Network Security**: VPN, firewalls, public Wi-Fi safety
- **Malware Protection**: Antivirus, ransomware, social engineering
- **Privacy**: Data protection, GDPR, PII handling
- **Device Security**: Updates, backups, encryption
- **Web Safety**: Browser security, safe downloads, social media
- **Incident Response**: Breach handling, forensics
- **Compliance**: Security standards and regulations

## 🔮 Future Enhancements

Potential improvements and additions:
- **Persistent data storage** (JSON/database)
- **Advanced NLP** with machine learning
- **Web-based GUI** for broader accessibility
- **Real-time threat intelligence** integration
- **Multi-language support**
- **Advanced quiz analytics**
- **Customizable reminder notifications**

## 🤝 Contributing

This is an educational project, but contributions are welcome:

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is created for educational purposes as part of a cybersecurity awareness curriculum.

## 👨‍💻 Author

**Jethro Hall**
- GitHub: [@HoloStack](https://github.com/HoloStack)
- Project: [CyberSecurityChatBot](https://github.com/HoloStack/CyberSecurityChatBot)

---

**🛡️ Stay Cyber Safe! 🛡️**

*"Cybersecurity is not just a technology problem—it's a human problem."*
