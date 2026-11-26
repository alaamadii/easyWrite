# Project Plan and Work Breakdown

## Delivery strategy

Development is divided into nine reviewable phases. Mandatory requirements are
completed before optional features and visual polish. Each phase must update docs,
build, test, commit, push, and open a pull request; no PR is merged without explicit
owner approval.

## Phase overview

| Phase / PR | Branch | Goal | Exit criteria |
| --- | --- | --- | --- |
| 00 | `chore/initialize-project` | Repository, architecture, contracts, docs, tests, CI | Clean restore/build/test; docs agree; no speech behavior claimed |
| 01 | `feat/speech-ui` | Complete responsive Windows Forms UI and view state | Both tabs and all specified controls render; keyboard/resize checks pass |
| 02 | `feat/text-to-speech` | Working local TTS and playback lifecycle | Speak, pause/resume, stop, clear, status, and missing-voice errors pass |
| 03 | `feat/speech-to-text` | Working microphone recognition | Explicit start/stop, appended results, status, and unavailable-device/engine errors pass |
| 04 | `feat/speech-settings` | Optional enhancements | Voice/rate/volume, timestamps, TXT export pass |
| 05 | `test/reliability-validation` | Reliability, disposal, and deeper tests | Repeated cycles/close/failure cases are stable; validation suite passes |
| 06 | `docs/final-submission` | Submission evidence and documentation | Authentic screenshots, final README, checklist, clean-clone validation |
| 07 | `docs/submission-closure` | Manual evidence and release closure | Verified identity, hardware/accessibility evidence, authentic screenshots, final green PR |
| 08 | `docs/final-manual-evidence` | Owner-attested validation and authentic evidence | Verified identity, TTS/accessibility evidence, honest STT blocker, final review PR |

## Phase 00 — Initialization (complete)

- [x] Confirm .NET 10 and Windows Desktop runtime are installed.
- [x] Confirm `System.Speech` has a .NET 10-compatible Windows package.
- [x] Create App, Core, Infrastructure, and Core.Tests projects.
- [x] Define foundational models, events, and service interfaces.
- [x] Add initial settings validation tests.
- [x] Document product, requirements, stories, UI, architecture, plan, testing, and
  decisions.
- [x] Add repository instructions, ignore rules, PR template, and Windows CI.
- [x] Restore locked dependencies, verify formatting, build Release, and run tests.
- [x] Push baseline `main`, push the feature branch, and open PR 00.

## Phase 01 — Interface

- [x] Build shared window header, navigation tabs, and persistent status strip.
- [x] Build TTS view with editor, count, voice/rate/volume, and action row.
- [x] Build STT view with language, microphone state, transcript, and actions.
- [x] Add a centralized state-to-controls renderer with deterministic tests.
- [x] Add input/empty/capability states without claiming real engine operations.
- [ ] Verify resizing at minimum/default sizes and Windows DPI 100/125/150%.
- [ ] Verify tab order, access keys, focus, labels, contrast, and no color-only state.
- [x] Add view/state tests where practical and record manual UI checks.

Phase 01 has been visually and interactively checked at 125% DPI. The remaining two
verification items stay open until 100% and 150% DPI and a complete keyboard-only
pass are available; see `TESTING.md` for the recorded scope.

## Phase 02 — Text to Speech

- [x] Implement `TextToSpeechService` over `SpeechSynthesizer`.
- [x] Enumerate enabled installed voices and choose a safe default.
- [x] Apply validated voice, rate, and volume without freezing the UI.
- [x] Implement Speak, Pause, Resume, Stop, completion, and cancellation.
- [x] Publish consistent state changes and protect against re-entry.
- [x] Handle blank text, missing voices, invalid voice, and audio output failures.
- [x] Dispose the synthesizer and detach events on shutdown.
- [x] Add automated adapter/state tests using seams where possible.
- [ ] Manually verify audible output and repeated playback on actual hardware.

Implementation and deterministic lifecycle coverage are complete on 2026-11-24.
Real Windows discovery found three enabled voices. Audible playback, pause/resume,
rate/volume perception, repeated cycles, and close-during-playback remain open
because no human listener validated the machine's audio output.

## Phase 03 — Speech to Text

- [x] Implement `SpeechToTextService` over `SpeechRecognitionEngine`.
- [x] Discover installed recognizers and expose real culture choices only.
- [x] Load a dictation grammar and set the default audio input.
- [x] Implement explicit asynchronous Start Listening and Stop Listening.
- [x] Map finalized results to text, timestamp, and confidence.
- [x] Marshal recognition/state events safely to the UI thread.
- [x] Handle no recognizer, wrong culture, missing/disabled microphone, permissions,
  recognizer/audio errors, cancellation, and repeated start/stop.
- [x] Dispose engine, grammar, events, and cancellation resources deterministically.
- [ ] Manually verify real microphone recognition in each installed target language.

Implementation and deterministic lifecycle coverage are complete on 2026-11-24.
The real Windows discovery probe found zero installed recognizers. Application
startup in that unavailable state passed, but spoken recognition and microphone
lifecycle checks remain blocked until a compatible Windows speech language is installed.

## Phase 04 — Enhancements

- [x] Complete voice selection behavior and fallback rules.
- [x] Complete rate and volume value labels and persistence decision.
- [x] Format timestamped transcript entries consistently.
- [x] Implement UTF-8 `TextExportService` and `SaveFileDialog` integration.
- [x] Report export success path; handle cancel, invalid path, access, and I/O errors.
- [x] Add export and transcript-formatting unit tests.

The Phase 04 code and deterministic file evidence are complete on 2026-11-25.
Voice/rate/volume behavior was audited and remains correctly covered by Phase 02;
settings persistence is intentionally not required. Live timestamp/export UI checks
remain blocked because the development machine still has no installed recognizer.

## Phase 05 — Reliability and validation

- [x] Audit every requirement and update traceability evidence.
- [x] Test at least ten deterministic TTS start/stop and STT start/stop cycles.
- [x] Test deterministic disposal while Speaking, Paused, Listening, and Stopping.
- [x] Test absent engine/device and file failure paths where safely reproducible.
- [x] Verify marshaling guards, observed background tasks, and process cleanup.
- [x] Review disposal ownership and event unsubscription.
- [x] Improve messages without hiding diagnostic causes.
- [ ] Complete the physical-hardware, interactive accessibility, and DPI matrix.

The deterministic Phase 05 audit is complete on 2026-11-26 with 90 passing tests.
It adds ten-cycle TTS/STT/export checks, paused/stopping disposal coverage, late
callback checks, export recovery, explicit view-event unsubscription, saved-path
success feedback, and retained Debug diagnostics. The current machine has three
enabled voices but still has zero recognizers. Audible TTS, real microphone work,
active-state interactive close, keyboard-only review, and DPI 100%/150% therefore
remain manual submission gates rather than claimed passes.

## Phase 06 — Final submission

- [x] Replace the README name placeholder with verified details; completed in Phase 08.
- [ ] Capture at least two authentic screenshots from the final running build.
- [x] Update README checkboxes, limitations, setup, and actual validation results.
- [x] Validate all links and remove stale placeholder text; identity was finalized in Phase 08.
- [x] Perform clean-clone restore, Release build, tests, and run.
- [x] Confirm no secrets, personal transcript data, build output, or fake evidence.
- [x] Create the final submission checklist.
- [ ] Create a version tag only after the final PR is merged and the owner approves it.

Phase 06 preparation on 2026-11-26 completed the documentation, local-link, and
repository-hygiene audits. The final submission remains intentionally incomplete:
the owner has not supplied verified student details, this machine still has zero
recognizers, no human has confirmed audible TTS, and no authentic final screenshots
exist. These are submission gates, not software successes that can be simulated.

## Phase 07 — Manual evidence and release closure

- [x] Confirm PR #12 merged into `main` with successful CI.
- [x] Repeat the locked restore, format, Release build, and 90-test baseline.
- [x] Repeat real voice/recognizer discovery without starting audio or microphone input.
- [x] Replace the README identity placeholder with the verified name; omit the student number by owner instruction.
- [x] Complete the owner-reported TTS hardware matrix.
- [ ] Complete genuine STT, timestamp, export, and physical lifecycle checks.
- [x] Complete owner-reported keyboard-only and DPI 100%/150% verification.
- [ ] Capture and inspect authentic TTS and STT screenshots.
- [x] Complete final clean-clone validation.
- [x] Open release-evidence PR #13 and confirm green CI.
- [x] Merge PR #13 into `main` at `2c8c8d1` with owner approval.
- [ ] Create `v1.0.0` only after the final PR is merged and the owner separately approves tagging.

The Phase 07 baseline on 2026-11-26 passed with 0 build warnings/errors and all
90 tests. Read-only discovery again found Microsoft Zira Desktop, Microsoft Tolga,
and Microsoft Naayf, but zero recognizers. Identity, human-observed audio,
interactive accessibility/DPI, and authentic screenshot gates remain open.

## Phase 08 — Owner-assisted final evidence

- [x] Record the verified student name `ALAA MADI` and omit the student number by explicit owner instruction.
- [x] Run locked restore, formatting verification, Release build, and all 90 tests.
- [x] Record the owner's successful TTS, keyboard, accessibility, contrast, and DPI report.
- [x] Capture and visually inspect an authentic paused-TTS screenshot from the real Release app.
- [x] Repeat production recognizer discovery after the owner reviewed Windows speech settings.
- [ ] Produce genuine STT, live export, lifecycle, and timestamp evidence.
- [ ] Capture an authentic timestamped STT screenshot.
- [ ] Complete final clean-clone validation and open the final evidence PR.
- [ ] Create `v1.0.0` only after the PR is merged and the owner separately approves tagging.

The Phase 08 automated baseline passed with 0 warnings/errors and 90 tests. The
owner reported the requested TTS and interactive accessibility/DPI matrix successful.
The production service still enumerated zero recognizers, so STT-dependent evidence
remains blocked and is not simulated. One authentic 1182×799 TTS screenshot was
captured in the real Paused state and inspected for privacy and legibility.

## Risk register

| Risk | Probability / impact | Mitigation |
| --- | --- | --- |
| No compatible local recognizer language | Medium / High | Discover early, document language-pack setup, test with installed culture, never simulate. |
| Microphone permission/device failure | Medium / High | Explicit preflight/error messages and actual-hardware tests. |
| `System.Speech` lifecycle quirks | Medium / High | Isolate adapter, use async APIs/events, test repeated start/stop and disposal. |
| UI-thread cross-thread exception | Medium / High | Centralize marshaling and test callbacks during shutdown. |
| Deadline pressure encourages optional work first | Medium / High | Phase gates keep all Must requirements before Should/Could polish. |
| CI passes without proving audio | High / Medium | Maintain a separate signed-off manual hardware matrix. |
| Repository authentication unavailable | Medium / Medium | Keep clean local commits; re-authenticate GitHub before push/PR. |

## Definition of project complete

The project is complete only when all Must requirements have evidence, Release
build/tests pass, the Windows hardware matrix has been run, authentic screenshots
are committed, student metadata is present, and the final documentation describes
the actual build without unsupported claims.
