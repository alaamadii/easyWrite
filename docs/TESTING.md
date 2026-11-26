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

The Phase 02 suite adds an Infrastructure test project with a fake synthesizer
adapter. It verifies enabled voice filtering/mapping, empty discovery, validation,
settings forwarding, state sequences, re-entry, cancellation, synthesizer errors,
invalid voices, late callbacks, event unsubscription, disposal, and post-dispose
commands without requiring a real speaker or installed voice.

The Phase 03 suite extends Infrastructure coverage with a fake recognition-engine
factory and session adapters. It verifies culture discovery, grammar/default-input
configuration, finalized results and ordering, explicit stop, cancellation,
startup/runtime failures, re-entry, late callbacks, and deterministic disposal
without requiring an installed recognizer or microphone.

The Phase 04 suite adds shared timestamp/document formatting, authoritative
transcript-session behavior, export-control state, and real temporary-directory
file checks. It verifies UTF-8 without a byte-order mark, Unicode, stable ordering,
approved replacement, pre-cancellation, mid-write cancellation, write/path
failures, temporary-file cleanup, released handles, and sequential exports.

The Phase 05 suite turns the existing seams into deterministic stress checks. It
runs ten TTS start/stop cycles, ten complete STT sessions with one result each, and
ten sequential exports. It also covers disposal while Paused/Stopping, late
callbacks after disposal, per-session event unsubscription, and a successful
export retry after a partial-write failure.

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

## Phase 02 text-to-speech evidence — 2026-11-24

Environment: Windows 11, .NET SDK 10.0.402, branch `feat/text-to-speech`.
The real Windows service was used only for voice discovery and startup. No audible
result is claimed because the assistant cannot hear the development machine.

Automated results:

- Restore after adding the test project: passed; lock file generated.
- Release build: passed with 0 warnings and 0 errors.
- Tests: 37 passed (7 Core, 13 App presentation, 17 Infrastructure).
- Deterministic cases include Speak/Pause/Resume/Stop, cancellation, failure,
  re-entry, late callbacks, settings forwarding, and deterministic disposal.

Real Windows smoke results:

- Enabled voice discovery: passed; count 3.
- `Microsoft Zira Desktop` — English (United States).
- `Microsoft Tolga` — Turkish (Turkey).
- `Microsoft Naayf` — Arabic (Saudi Arabia).
- Release executable startup: passed; the process remained healthy for the
  three-second startup observation.
- Normal interactive form close: not run. The hidden startup probe had no main
  window handle and was force-cleaned; a follow-up check found no remaining
  `EasyWrite.App` process.

Audible/manual matrix:

- Short sentence and multi-sentence playback: not run — no human listener.
- Pause and Resume audible continuity: not run — no human listener.
- Stop before completion and repeated Speak/Stop: not run — no human listener.
- Voice selection with multiple voices: discovery passed; audible distinction not run.
- Rate -10, 0, and 10: forwarding is automated; audible effect not run.
- Low volume and volume 100: forwarding is automated; audibility not run.
- Repeated Speak while active: automated rejection passed; interactive check not run.
- Clear after playback: Phase 01 clear behavior passed; after-audio path not run.
- Close while Speaking or Paused: not run — would require starting real audio.
- No-voice/no-output behavior: deterministic failure paths passed; destructive
  machine reconfiguration was not attempted.
- UI responsiveness during audio: not run — real audio was not started.

## Phase 03 speech-to-text evidence — 2026-11-24

Environment: Windows 11, .NET SDK 10.0.402, branch `feat/speech-to-text`.
The real Windows API was used for recognizer discovery only. It reported zero
installed recognizers, so no microphone was activated and no spoken transcript is claimed.

Automated results:

- Release build: passed with 0 warnings and 0 errors.
- Tests: 60 passed (7 Core, 13 App presentation, 40 Infrastructure).
- Deterministic STT cases cover discovery, selection, grammar/input setup,
  finalized-result mapping/order, Start/Stop, cancellation, failures, re-entry,
  late events, unsubscription, disposal, and post-dispose commands.
- Existing TTS coverage remains passing.

Real Windows smoke results:

- Installed recognizer discovery: passed; count 0.
- Release executable startup with zero recognizers: passed; process remained healthy
  for a three-second hidden observation and no process remained after exact-PID cleanup.
- Unavailable-state behavior: code path and startup passed; visual text was not
  interactively inspected during the hidden launch.

Microphone/manual matrix:

- Start Listening and default microphone activation: blocked — no installed recognizer.
- Short and multi-sentence spoken recognition: blocked — no installed recognizer.
- Ordered finalized transcript results: automated only; real speech blocked.
- Stop Listening and repeated Start/Stop: automated only; hardware path blocked.
- TTS/STT conflict prevention: automated UI-state coverage passed; interactive
  microphone check blocked.
- Closing while Listening or Stopping: blocked — no recognition session can start.
- Missing-recognizer startup: passed with a genuine zero-recognizer environment.
- Missing microphone and denied permission: not run — recognizer absence prevents
  reaching microphone setup, and machine permissions were not modified.
- Per-culture checks: blocked — no recognizer cultures are installed.
- UI responsiveness during recognition: blocked — no real recognition session.
- Normal interactive form close: not run during the hidden startup probe.

## Phase 04 timestamped transcript and TXT export evidence — 2026-11-25

Environment: Windows 11 Pro, .NET SDK 10.0.402, branch
`feat/speech-settings`. The real Windows API was queried for recognizer discovery;
it again reported zero installed recognizers. No microphone session or spoken
transcript is claimed.

Automated results:

- Release build: passed with 0 warnings and 0 errors.
- Formatting verification: passed with no changes.
- Tests: 84 passed (12 Core, 21 App presentation, 51 Infrastructure).
- Core coverage verifies local `[HH:mm:ss]` entries, complete document formatting,
  Unicode/order, blank-text policy, and no mutation of source results.
- App coverage verifies authoritative result count/order/snapshot behavior and
  clear/export availability during Idle, Listening, Stopping, and export-busy states.
- Infrastructure coverage performs actual asynchronous file writes in unique
  temporary directories. UTF-8 without BOM, replacement, cancellation/failure
  cleanup, preserved existing content, released handles, and repeated exports passed.

Real Windows smoke results:

- Installed recognizer discovery: passed; count 0.
- Release executable startup: passed; the process remained healthy for a
  three-second hidden observation and no process remained after exact-PID cleanup.
- Timestamped transcript and Save dialog export in the live UI: blocked because no
  installed recognizer can produce a genuine finalized result on this machine.

Manual/export matrix:

- Actual generated TXT content and ordering: passed through real file-system tests;
  no test output was written inside the repository.
- Save dialog cancellation and visible success/error feedback: not run — they
  require an interactive desktop session.
- Genuine recognized timestamps compared with exported timestamps: blocked — zero
  installed recognizers.
- Access-denied/in-use destination through the UI: not run; deterministic service
  failure and transcript-preservation behavior passed automatically.
- Audible TTS, microphone capture, speech accuracy, final screenshots, and final
  accessibility/layout review: not run; no unsupported claim is made.

## Phase 05 reliability and validation evidence — 2026-11-26

Environment: Windows 11 Pro 10.0.22631, .NET SDK 10.0.402, branch
`test/reliability-validation`, 125% display scaling (120 DPI). No audio playback or
microphone capture was started during this audit.

Automated results:

- Locked restore: passed.
- Formatting verification: passed with no changes.
- Release build: passed with 0 warnings and 0 errors.
- Tests: 90 passed (12 Core, 21 App presentation, 57 Infrastructure).
- Ten TTS start/stop cycles returned to Idle without overlap.
- Ten STT start/stop sessions created and disposed ten separate adapters, produced
  exactly ten results, and unsubscribed every session handler once.
- Ten sequential real file-system exports completed without leaked temporary files.
- Disposal while Paused and Stopping cancels pending work and ignores late callbacks.
- A partial-write failure removes its temporary file and the same export service
  succeeds on the next request.
- Source audit found no production `.Wait()`, `.Result`, synchronous speech or
  recognition call, or synchronous long-running export path.
- View/content/tab and service events have explicit shutdown unsubscription where
  ownership crosses the form boundary. User-facing failures remain concise while
  caught exceptions are retained in Debug diagnostics.

Real Windows capability and process results:

- Enabled voice discovery: passed; count 3 (`Microsoft Zira Desktop`,
  `Microsoft Tolga`, and `Microsoft Naayf`).
- Installed recognizer discovery: passed; count 0.
- Release executable startup: passed; the hidden process remained healthy for a
  three-second observation.
- Normal close: not run. The hidden launch exposed no main-window handle, so only
  its exact process ID was force-cleaned; the remaining `EasyWrite.App` count was 0.

Manual/hardware matrix:

- Audible short/multi-sentence TTS, pause/resume continuity, rate/volume perception,
  and ten physical start/stop cycles: not run — no human listener participated.
- Close while real Speaking or Paused: not run; deterministic service disposal for
  Speaking, Paused, and Stopping passed.
- Start Listening, real results, timestamps, ten physical STT cycles, and close
  while Listening/Stopping: blocked — the genuine recognizer count is 0.
- Microphone absence/permission changes: not run — machine settings were not changed.
- Save-dialog cancellation, successful UI export, and visible recovery feedback:
  not run in an interactive desktop. Real temporary-directory content, replacement,
  cancellation, failure, retry, cleanup, and handle-release checks passed.
- Default/minimum layout at 125% DPI retains the earlier Phase 01 evidence but was
  not re-run interactively in Phase 05. DPI 100%/150% and the full keyboard-only
  accessibility pass remain not run.
- Final screenshots: not run; authentic screenshots remain a Phase 06 deliverable.

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

## Phase 06 final-submission audit — 2026-11-26

Environment: Windows 11 Pro 10.0.22631, .NET SDK 10.0.402, branch
`docs/final-submission`, 125% display scaling (120 DPI). The capability probe and
validation commands did not start audio playback or microphone capture.

Final automated results:

- `dotnet restore EasyWrite.slnx --locked-mode`: passed.
- `dotnet format EasyWrite.slnx --verify-no-changes --no-restore`: passed.
- `dotnet build EasyWrite.slnx --configuration Release --no-restore`: passed with
  0 warnings and 0 errors.
- `dotnet test EasyWrite.slnx --configuration Release --no-build`: 90 passed
  (12 Core, 21 App presentation, and 57 Infrastructure).
- Pushed commit `0b57412a4bdb913f27016259ecfc6a1cf359ab9e` was cloned into a
  new temporary directory. Locked restore, formatting verification, Release build,
  all 90 tests, and the three-second startup check passed there; the verified
  temporary clone was then removed.
- All repository-local Markdown links resolve.
- Tracked-file hygiene checks found no `bin`, `obj`, `TestResults`, binary/image
  artifacts, log/test-result files, machine-specific paths, private keys, or API-key
  patterns.
- At the Phase 06 audit, the README identity fields were deliberately unresolved.
  Phase 08 later supplied the verified name and omitted the number by owner instruction.

Real Windows capability evidence:

- Enabled voices: 3 (`Microsoft Zira Desktop`, `Microsoft Tolga`, and
  `Microsoft Naayf`).
- Installed recognizers: 0. A genuine recognized transcript, STT export flow, and
  populated STT screenshot are therefore blocked on this machine.
- Release executable startup: passed; the process remained healthy for a
  three-second hidden observation, its exact process ID was stopped, and zero
  `EasyWrite.App` processes remained. The hidden launch exposed no window handle,
  so this is not evidence that speech, recognition, or visible layout worked.

Manual and delivery evidence:

- Audible TTS, pause/resume continuity, rate/volume perception, microphone input,
  real recognition, and close during active hardware operations: not run or blocked.
- Keyboard-only navigation, 100%/150% DPI, visible Save dialog feedback, and final
  screenshot capture: not run in an interactive desktop session.
- No screenshot is committed because no genuine final screenshot has been captured;
  an unavailable-state or typed transcript is not substituted for STT evidence.
- Final CI, PR review, and optional release tagging are recorded in
  `SUBMISSION_CHECKLIST.md` and must not be marked complete before they actually
  occur.

## Phase 07 closure baseline — 2026-11-26

Branch: `docs/submission-closure`, based on merged PR #12 commit
`923372ab54a9b59269d97ce5b2e217ccba23d5c7`.

Automated baseline:

- Locked restore: passed.
- Formatting verification: passed with no changes.
- Release build: passed with 0 warnings and 0 errors.
- Tests: 90 passed (12 Core, 21 App presentation, and 57 Infrastructure).
- Hidden Release startup: passed; the process remained healthy for three seconds,
  its exact PID was stopped, and zero `EasyWrite.App` processes remained. No window
  handle was exposed, so this is process evidence rather than a visible UI check.
- Pushed commit `ccf9014c07d5604bf385c2632969dbfd3b29e3a1` was cloned into a
  new temporary directory. Locked restore, formatting verification, Release build,
  all 90 tests, and the three-second startup smoke check passed; exact-PID cleanup
  left zero application processes, and the verified temporary clone was removed.
- Release-evidence PR #13 Windows CI run `45815719014` passed restore, formatting,
  Release build, all tests, and test-result upload.

A temporary read-only test used the production discovery services, printed the
results, and was removed immediately afterward. It did not request synthesis or
recognition and did not start audio playback or microphone input.

- Enabled voices: 3 (`Microsoft Zira Desktop`, `Microsoft Tolga`, and
  `Microsoft Naayf`).
- Installed recognizers: 0.

Consequently, genuine STT, recognized timestamps, live transcript export, physical
recognition lifecycle checks, and the required STT screenshot remain blocked on
this machine. Audible TTS, keyboard/DPI review, and screenshots remain not run
because no human-observed interactive session has been completed. Student identity
also remains unverified and is not inferred from repository or account data.

## Phase 08 owner-assisted final evidence — 2026-11-26

Environment: Windows 11 Pro 10.0.22631, .NET SDK 10.0.402, branch
`docs/final-manual-evidence`. Verified student name: `ALAA MADI`. The student-number
field is intentionally omitted by the owner's explicit instruction.

Final automated baseline:

- Locked restore: passed.
- Formatting verification: passed with no changes.
- Release build: passed with 0 warnings and 0 errors.
- Tests: 90 passed (12 Core, 21 App presentation, and 57 Infrastructure).
- Hidden Release startup: passed; the process remained healthy for three seconds,
  exact-PID cleanup left zero `EasyWrite.App` processes, and no visible/manual claim
  is inferred from the hidden launch.
- Pushed commit `d9353886eec980b1d61fc793ad09dbf18f84b33b` passed locked
  restore, formatting verification, Release build, all 90 tests, and a three-second
  startup smoke check from a new temporary clone. Exact-PID cleanup left zero app
  processes, and the verified temporary clone was removed.
- Final evidence PR #14 Windows CI run `45823762169` passed restore, formatting,
  Release build, all tests, and test-result upload.
- The temporary capability probe was removed and is not part of the repository.

Owner-reported manual matrix:

- Audible short and multi-sentence TTS: passed.
- Available voice selection plus rate and volume behavior: passed.
- Pause, Resume, Stop, ten physical cycles, active-audio close, and UI
  responsiveness: passed.
- Keyboard-only navigation, access keys, focus, accessible descriptions, textual
  status, contrast, and default/minimum layout: passed.
- 100%, 125%, and 150% DPI layout review: passed.

The statements above are explicitly owner-reported manual evidence. The assistant
did not infer audible perception or DPI changes from automated state alone.

Directly observed capture evidence:

- The real Release app accepted non-personal text, started actual TTS, entered
  Paused, exposed Resume/Stop, and displayed matching inline/global Paused status.
- One rapid repeat attempt exposed `Audio playback could not start...`; the error
  was not hidden. After an exact-process restart, TTS again reached Paused and the
  final image was captured successfully.
- `docs/screenshots/text-to-speech.png` is 1182×799, window-only, legible, and free
  of usernames, machine paths, notifications, transcripts, and unrelated windows.
- Its SHA-256 is
  `CAEDD651F6193CB77A58F0656DE63A590BA489A3CF5DCA2A38915E75A9BE259D`.

Remaining blocked evidence:

- After Windows speech settings were manually reviewed, a fresh read-only probe
  through the production services still enumerated 3 voices and 0 compatible
  recognizers.
- Genuine microphone recognition, finalized results, timestamps, STT repeated
  cycles/active-close behavior, live recognized-transcript export, and
  `speech-to-text.png` remain blocked.
- No typed/injected transcript, Voice Access result, unavailable-state image,
  mockup, or generated image is substituted for those requirements.
