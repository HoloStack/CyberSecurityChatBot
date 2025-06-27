Add-Type -AssemblyName System.Speech
$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer
$synth.SetOutputToWaveFile("Audio/greetings.wav")
$synth.Speak("Welcome to your Cybersecurity Assistant! Stay safe online.")
$synth.Dispose()
Write-Host "Greeting audio file created successfully!"
