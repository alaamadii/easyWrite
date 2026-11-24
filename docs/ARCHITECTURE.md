# Architecture

## Architectural goals

The design should be easy to explain, testable without audio hardware where
possible, safe around unmanaged Windows speech resources, and small enough for a
course assignment. It uses a pragmatic layered architecture rather than a large
framework or full dependency-injection container.

## System context

```text
┌──────────────┐       clicks, text, speech       ┌──────────────────┐
│ Desktop user │ ───────────────────────────────► │ EasyWrite        │
│              │ ◄─────────────────────────────── │ WinForms app     │
└──────────────┘       audio, transcript, status  └───────┬──────────┘
                                                          │
                                           ┌──────────────┼──────────────┐
                                           ▼              ▼              ▼
                                  Windows Speech API  Audio devices  Local TXT file
```

No cloud service, account, database, or API secret is part of the initial system.

## Project dependency rule

```text
┌───────────────────────┐
│ EasyWrite.App         │  Forms, controls, view-state mapping, composition
└───────────┬───────────┘
            │ references
       ┌────▼───────────┐       ┌───────────────────────────┐
       │ EasyWrite.Core │ ◄──── │ EasyWrite.Infrastructure  │
       │ models/contracts│       │ System.Speech + file I/O │
       └────▲───────────┘       └───────────────────────────┘
            │
┌───────────┴────────────┐
│ EasyWrite.Core.Tests   │
└────────────────────────┘
```

Allowed dependencies:

- App → Core and Infrastructure
- Infrastructure → Core and `System.Speech`
- Core.Tests → Core

Forbidden dependencies:

- Core → Windows Forms, `System.Speech`, App, or Infrastructure
- Infrastructure → App
- Core tests → real audio hardware

## Responsibility map

| Component | Responsibility | Owns/disposes |
| --- | --- | --- |
| `MainForm` | Window lifecycle, navigation, service composition | Child views and application services |
| `TextToSpeechView` | Text editor, settings controls, playback commands, status rendering | UI controls only |
| `SpeechToTextView` | Listen commands, transcript presentation, export commands | UI controls only |
| `ITextToSpeechService` | Installed voices and playback lifecycle | Implementing service owns synthesizer |
| `ISpeechToTextService` | Recognizer discovery, microphone lifecycle, finalized results | Implementing service owns recognizer |
| `ITextExportService` | Validate destination and write UTF-8 transcript | File stream/writer per operation |
| `SpeechSettings` | Voice/rate/volume value and validation | Nothing disposable |
| `SpeechResult` | Recognized text, timestamp, confidence | Nothing disposable |
| `SpeechOperationState` | Shared vocabulary for visible state | Nothing disposable |

The `TextToSpeechView` and `SpeechToTextView` are planned components; they are not
part of the initialization implementation.

## Runtime composition

`Program` initializes Windows Forms and constructs `MainForm`. In the feature
phases, the composition root will create concrete infrastructure services and pass
them into the form or views. Constructor injection is preferred; a dependency
injection package is unnecessary unless composition becomes materially complex.

## Text-to-speech flow

```text
User clicks Speak
  → view trims and validates text
  → SpeechSettings.Validate()
  → ITextToSpeechService.SpeakAsync(...)
  → infrastructure configures installed voice/rate/volume
  → SpeechSynthesizer.SpeakAsync(...)
  → service publishes Speaking / Idle or Error
  → view maps state to status and enabled controls
```

Pause and Resume are valid only for their corresponding states. Stop cancels queued
prompts. Completion and cancellation both converge on a stable Idle state; an
unexpected failure passes through Error with a friendly message and then permits
recovery.

## Speech-to-text flow

```text
User clicks Start Listening
  → view confirms a selected installed recognizer culture
  → ISpeechToTextService.StartListeningAsync(...)
  → infrastructure creates/configures recognition engine
  → default audio device becomes input
  → service publishes Listening
  → finalized SpeechRecognized event becomes SpeechResult
  → UI marshals event to UI thread and appends it
User clicks Stop / form closes
  → service requests asynchronous stop
  → event handlers detach and engine is disposed
  → state returns to Idle
```

Recognition callbacks may occur away from the UI thread. Views must use
`InvokeRequired`/`BeginInvoke` (or captured synchronization context) before touching
controls and must ignore callbacks after disposal begins.

## State model

| State | Meaning | Valid primary actions |
| --- | --- | --- |
| Idle | No engine operation is active. | Speak or Start Listening |
| Speaking | Synthesizer is producing/queuing audio. | Pause, Stop |
| Paused | Speech is intentionally paused. | Resume, Stop |
| Listening | Microphone recognition is active. | Stop Listening |
| Stopping | Asynchronous cleanup is in progress. | None; wait |
| Error | Latest operation failed and feedback is visible. | Clear/retry after recovery |

Only one speech operation can own the active state at a time. Feature PRs should
centralize state transitions rather than scatter Boolean flags across controls.

## Error strategy

Infrastructure preserves exception context and maps known failure categories to a
small application-facing result/message vocabulary:

| Failure | User guidance |
| --- | --- |
| No installed voice | Install a Windows speech voice and restart EasyWrite. |
| No recognizer for culture | Install the matching Windows speech language or choose another. |
| No/default microphone unavailable | Connect or enable a microphone and check Windows Sound settings. |
| Microphone permission denied | Enable desktop microphone access in Windows Privacy settings. |
| Audio output unavailable | Choose or connect an output device and retry. |
| File access denied/in use | Choose a writable file/location and retry. |
| Invalid state/repeated command | Return controls to the service-reported state; do not crash. |

The UI must not expose a stack trace. Development diagnostics may log exception
type/message, but no microphone audio or recognized content should be logged by
default.

## Resource ownership

- The concrete TTS service owns exactly one `SpeechSynthesizer` during its lifetime.
- The concrete STT service owns its `SpeechRecognitionEngine`, loaded grammars,
  event subscriptions, and cancellation coordination.
- A component that subscribes to a longer-lived publisher must unsubscribe during
  disposal.
- Form closing first prevents new work, then requests stop, then disposes services.
- Export uses `await using` for file resources and never overwrites without the
  path already approved through the save dialog.
- `Dispose` must be idempotent; finalizers are not introduced for managed wrappers.

## Threading rules

- Use asynchronous engine APIs and file APIs where available.
- Never call `.Wait()`, `.Result`, or long synchronous speech methods on the UI
  thread.
- Guard against re-entry by state and disabled commands.
- Cancellation is expected control flow and should not appear as a user error.
- Event callbacks verify the view is not disposed before marshaling to controls.

## Testing seams

Core interfaces let view/state behavior use fakes without a Windows engine. Unit
tests cover value validation, transition rules, transcript formatting, and error
mapping as those classes are introduced. Real engine discovery, speakers, and
microphones remain manual/integration checks on Windows.

## Security and privacy

- Microphone listening is opt-in per session and continuously visible.
- The local approach does not intentionally transmit voice or text.
- Transcripts persist only when the user explicitly exports.
- No API keys, credentials, telemetry, or background startup are required.
