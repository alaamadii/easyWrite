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
| `Program` | Composition root and service lifetime | Concrete TTS and STT services |
| `MainForm` | Window lifecycle, navigation, command/event coordination | Child views and UI event subscriptions |
| `TextToSpeechView` | Text editor, settings controls, playback commands, status rendering | UI controls only |
| `SpeechToTextView` | Listen commands, transcript presentation, export commands | UI controls only |
| `ITextToSpeechService` | Installed voices and playback lifecycle | Implementing service owns synthesizer |
| `ISpeechToTextService` | Recognizer discovery, microphone lifecycle, finalized results | Implementing service owns recognizer |
| `ITextExportService` | Validate destination and write UTF-8 transcript | File stream/writer per operation |
| `SpeechSettings` | Voice/rate/volume value and validation | Nothing disposable |
| `SpeechResult` | Recognized text, timestamp, confidence | Nothing disposable |
| `SpeechOperationState` | Shared vocabulary for visible state | Nothing disposable |
| `SpeechControlStateMapper` | Derive enabled actions, labels, and accessible status from one state/capability context | Nothing disposable |

`TextToSpeechView`, `SpeechToTextView`, `OperationStatusControl`, and the centralized
state mapper were implemented in Phase 01. Phase 02 injects TTS, and Phase 03 now
injects STT. Each capability is exposed only when its real Windows engines are
discovered; one unavailable service does not disable the other.

## Runtime composition

`Program` initializes Windows Forms, independently constructs
`TextToSpeechService` and `SpeechToTextService`, injects their Core interfaces into
`MainForm`, and disposes both after the message loop exits. If either service fails
to initialize, the form still opens with an unavailable-capability message while
the other service remains usable. A dependency-injection package is unnecessary.

## Text-to-speech flow

```text
User clicks Speak
  → view exposes text, selected voice, rate, and volume through its interaction contract
  → SpeechSettings.Validate()
  → ITextToSpeechService.SpeakAsync(...)
  → TextToSpeechService validates the installed voice and active-operation guard
  → internal adapter configures installed voice/rate/volume/default output
  → SpeechSynthesizer.SpeakAsync(...)
  → service publishes Speaking / Idle or Error
  → MainForm marshals callbacks to the UI thread
  → centralized mapper updates status and enabled controls
```

Pause and Resume are valid only for their corresponding states. Stop cancels queued
prompts. Completion and cancellation both converge on a stable Idle state; an
unexpected failure passes through Error with a friendly message and then permits
recovery.

`ISpeechSynthesizerAdapter` is internal to Infrastructure. Its production
implementation is the only type that touches `SpeechSynthesizer`; the narrow seam
lets tests drive completion, cancellation, errors, and late callbacks without
installed voices or speakers. Each prompt's opaque operation token prevents a late
completion from changing the state of a newer operation.

## Speech-to-text flow

```text
User clicks Start Listening
  → view confirms a selected installed recognizer culture
  → ISpeechToTextService.StartListeningAsync(...)
  → service resolves culture to a real internal recognizer ID
  → factory creates a session-scoped recognition adapter
  → adapter loads DictationGrammar and selects the default audio input
  → SpeechRecognitionEngine.RecognizeAsync(Multiple)
  → service publishes Listening
  → finalized SpeechRecognized becomes text/timestamp/confidence SpeechResult
  → UI marshals event to UI thread and appends it
User clicks Stop / form closes
  → service publishes Stopping and requests asynchronous cancellation
  → RecognizeCompleted detaches handlers and disposes the session engine
  → state returns to Idle
```

`ISpeechRecognitionEngineFactory` and `ISpeechRecognitionEngineAdapter` are internal
to Infrastructure. A new engine is created for the selected recognizer on each
session and disposed after completion. The active adapter reference is the session
identity, so late results/completions from an older adapter are ignored.

Recognition callbacks may occur away from the UI thread. `MainForm` uses
`InvokeRequired`/`BeginInvoke`, rechecks shutdown/disposal inside queued callbacks,
and ignores callbacks after shutdown begins.

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
- It subscribes once, cancels active speech during disposal, unsubscribes, then
  disposes the adapter; repeated disposal is harmless.
- The concrete STT service owns one session-scoped recognition adapter at a time,
  including its engine, dictation grammar, input, events, and cancellation registration.
- Recognition completion unsubscribes and disposes that adapter before a later
  session can become active; repeated disposal is harmless.
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
- TTS state events may originate on a synthesizer thread. `MainForm` checks shutdown
  and disposal flags and uses `BeginInvoke`; queued callbacks recheck those flags.
- STT state and result events follow the same marshaling rule. Recognition text is
  appended only after a finalized event and never from the engine thread directly.

## Testing seams

Core interfaces let view/state behavior use fakes without a Windows engine.
Infrastructure tests receive an internal fake adapter through `InternalsVisibleTo`
and cover voice filtering, settings forwarding, all playback transitions,
cancellation, errors, re-entry, late events, and disposal. STT tests use a fake
factory/adapter to cover discovery, grammar/input setup, ordered finalized results,
stop/cancellation, failures, late callbacks, and disposal. Real engine discovery is
a separate Windows smoke check; speakers and microphones remain manual checks.

## Security and privacy

- Microphone listening is opt-in per session and continuously visible.
- The local approach does not intentionally transmit voice or text.
- Transcripts persist only when the user explicitly exports.
- No API keys, credentials, telemetry, or background startup are required.
