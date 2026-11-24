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

The Phase 01 suite adds deterministic presentation-state coverage for every
`SpeechOperationState`, unavailable and available capability contexts, conflicting
operation prevention, playback control labels, transcript actions, and accessible
status text.

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

## Phase 01 interface evidence — 2026-11-24

Environment: Windows 11, .NET SDK 10.0.402, branch `feat/speech-ui`, 125% DPI
(120 DPI). No microphone, speaker, recognizer, or speech engine operation was
started during this phase.

Automated results:

- Locked restore: passed.
- Formatting verification: passed.
- Release build: passed with 0 warnings and 0 errors.
- Tests: 20 passed (7 Core and 13 App presentation tests).

Manual / UI Automation results:

- Release executable launched and closed cleanly: passed.
- Default/enforced minimum layout at 125% DPI: passed for both tabs with no core
  control clipping.
- Text and color both communicate the Ready/microphone-off states: passed.
- Tab selection and visible keyboard focus: passed.
- Character count changed from 0 to 15 for `Hello EasyWrite`: passed.
- Rate changed from 0 to 5 and volume from 100 to 95 through keyboard input;
  visible values updated: passed.
- Clear Text emptied the editor and preserved a usable UI: passed.
- Start Listening, Stop Listening, Speak, playback controls, and Export remained
  disabled while services are unavailable: passed.
- Real TTS, microphone recognition, and TXT export: not run because those features
  intentionally belong to Phases 02–04.
- DPI 100% and 150%: not run because the current Windows session is configured at
  125% and changing the user's display configuration was out of scope.
- Full keyboard-only traversal/access-key audit: not run; scheduled before Phase 01
  is considered fully verified.

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
