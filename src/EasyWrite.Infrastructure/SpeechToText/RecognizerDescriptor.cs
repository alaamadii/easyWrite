using System.Globalization;

namespace EasyWrite.Infrastructure.SpeechToText;

internal sealed record RecognizerDescriptor(
    string Id,
    string Name,
    CultureInfo Culture,
    string Description);
