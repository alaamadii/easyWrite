namespace EasyWrite.Core.Models;

/// <summary>Describes an installed local text-to-speech voice.</summary>
public sealed record SpeechVoice(string Name, string Culture, string Description, bool IsEnabled);
