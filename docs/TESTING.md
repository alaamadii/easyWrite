# Testing Strategy

## Principles

Testing evidence must match what was actually exercised. Automated tests validate
deterministic code; they do not prove audible output, microphone capture, speech
accuracy, installed languages, or final visual quality.

## Test layers

| Layer | Scope | Environment |
| --- | --- | --- |
| Unit | Settings validation, state transitions, transcript formatting, error mapping, export formatting | Any .NET Windows build agent; no hardware |
| Component | Views/presenters with fake services; concrete file export using temporary directories | Windows test process |
| Adapter smoke | Engine discovery, construction, cancellation, and disposal | Windows with known installed capabilities |
| Manual end-to-end | Real voice, speaker, microphone, languages, permissions, UI/accessibility | Physical Windows machine |

## Automated commands

```powershell
dotnet restore EasyWrite.slnx
dotnet build EasyWrite.slnx --configuration Release --no-restore
dotnet test EasyWrite.slnx --configuration Release --no-build --logger "trx"
dotnet format EasyWrite.slnx --verify-no-changes --no-restore
```

The initialization test suite covers the `SpeechSettings` supported ranges. Tests
for state coordination, adapters, errors, transcript formatting, and export are
added with the feature that introduces each behavior.

## Core test catalog

| Area | Required cases |
| --- | --- |
| Settings | Boundary and typical rate/volume values; below/above range |
| State coordinator | Every valid transition; rejected conflicting/repeated commands; error recovery |
| Transcript | Append order, local timestamp format, empty text policy, confidence boundaries |
| Export | UTF-8 content, timestamps/order, cancellation, access/I/O failures, no result loss |
| Error mapping | Missing voice/recognizer/device, permissions, invalid state, unknown exception |
| Disposal | Repeated disposal, post-dispose commands, late events/callbacks |

## Manual Windows test matrix

Record tester, date/time, Windows version, installed voice/recognizer cultures, and
result (`Pass`, `Fail`, `Blocked`, `Not run`) when executing this matrix.

### Application and UI

- [ ] Launches from a clean Release build without a debugger.
- [ ] Both tabs, every label, and every action render at default/minimum window size.
- [ ] Resize and DPI at 100%, 125%, and 150% do not clip core controls.
- [ ] Keyboard-only navigation follows a logical order with visible focus.
- [ ] Status is understandable without relying on color.

### Text to Speech

- [ ] Empty text produces inline validation and no operation.
- [ ] Short and multi-sentence text are audible through the default output device.
- [ ] Correct installed voice, minimum/default/maximum rate, and volume are applied.
- [ ] Pause halts and Resume continues the same operation.
- [ ] Stop cancels output promptly; Clear returns focus appropriately.
- [ ] Ten start/stop cycles do not hang or overlap audio.
- [ ] Closing while Speaking and Paused exits cleanly.
- [ ] Missing/disabled output or unavailable voice produces actionable feedback.

### Speech to Text

- [ ] Microphone remains off until Start Listening is clicked.
- [ ] Listening status remains visible for the full capture period.
- [ ] Speech appends as finalized text without replacing previous results.
- [ ] Result timestamps and ordering are correct.
- [ ] Stop Listening ends capture and returns to Idle.
- [ ] Ten start/stop cycles do not hang or duplicate event callbacks.
- [ ] Closing while Listening/Stopping exits cleanly.
- [ ] Missing microphone, denied permission, and unavailable recognizer are handled.
- [ ] Each advertised installed culture is checked with representative speech.

### Export and recovery

- [ ] Exported file is readable UTF-8 and matches visible ordering/timestamps.
- [ ] Canceling the save dialog has no side effect and shows no error.
- [ ] Access-denied/in-use destination preserves the transcript and offers recovery.
- [ ] After a recoverable error, the user can complete a new valid operation.

## CI scope

GitHub Actions uses a Windows runner because the projects target
`net10.0-windows`. It restores, verifies formatting, builds Release with warnings as
errors, and runs tests. Hardware-dependent tests must not be labeled as ordinary CI
tests or made flaky through assumptions about hosted-runner audio devices.

## Evidence record template

```text
Date/time (Asia/Riyadh):
Commit SHA:
Tester:
Windows version:
.NET SDK:
Installed voices:
Installed recognizer cultures:
Audio output / microphone:
Automated result:
Manual matrix result:
Known failures or blocked checks:
```
