# Project Plan and Work Breakdown

## Delivery strategy

Development is divided into seven reviewable phases. Mandatory requirements are
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
| 05 | `fix/validation-and-errors` | Reliability, disposal, and deeper tests | Repeated cycles/close/failure cases are stable; validation suite passes |
| 06 | `docs/final-submission` | Submission evidence and documentation | Authentic screenshots, final README, checklist, clean-clone validation |

## Phase 00 — Initialization (current)

- [x] Confirm .NET 10 and Windows Desktop runtime are installed.
- [x] Confirm `System.Speech` has a .NET 10-compatible Windows package.
- [x] Create App, Core, Infrastructure, and Core.Tests projects.
- [x] Define foundational models, events, and service interfaces.
- [x] Add initial settings validation tests.
- [x] Document product, requirements, stories, UI, architecture, plan, testing, and
  decisions.
- [x] Add repository instructions, ignore rules, PR template, and Windows CI.
- [x] Restore locked dependencies, verify formatting, build Release, and run tests.
- [ ] Push baseline `main`, push the feature branch, and open PR 00.

## Phase 01 — Interface

- [ ] Build shared window header, navigation tabs, and persistent status strip.
- [ ] Build TTS view with editor, count, voice/rate/volume, and action row.
- [ ] Build STT view with language, microphone state, transcript, and actions.
- [ ] Add a centralized state-to-controls renderer using placeholder/fake services.
- [ ] Add input/empty/capability states without claiming real engine operations.
- [ ] Verify resizing at minimum/default sizes and Windows DPI 100/125/150%.
- [ ] Verify tab order, access keys, focus, labels, contrast, and no color-only state.
- [ ] Add view/state tests where practical and record manual UI checks.

## Phase 02 — Text to Speech

- [ ] Implement `TextToSpeechService` over `SpeechSynthesizer`.
- [ ] Enumerate enabled installed voices and choose a safe default.
- [ ] Apply validated voice, rate, and volume without freezing the UI.
- [ ] Implement Speak, Pause, Resume, Stop, completion, and cancellation.
- [ ] Publish consistent state changes and protect against re-entry.
- [ ] Handle blank text, missing voices, invalid voice, and audio output failures.
- [ ] Dispose the synthesizer and detach events on shutdown.
- [ ] Add automated adapter/state tests using seams where possible.
- [ ] Manually verify audible output and repeated playback on actual hardware.

## Phase 03 — Speech to Text

- [ ] Implement `SpeechToTextService` over `SpeechRecognitionEngine`.
- [ ] Discover installed recognizers and expose real culture choices only.
- [ ] Load a dictation grammar and set the default audio input.
- [ ] Implement explicit asynchronous Start Listening and Stop Listening.
- [ ] Map finalized results to text, timestamp, and confidence.
- [ ] Marshal recognition/state events safely to the UI thread.
- [ ] Handle no recognizer, wrong culture, missing/disabled microphone, permissions,
  recognizer/audio errors, cancellation, and repeated start/stop.
- [ ] Dispose engine, grammar, events, and cancellation resources deterministically.
- [ ] Manually verify real microphone recognition in each installed target language.

## Phase 04 — Enhancements

- [ ] Complete voice selection behavior and fallback rules.
- [ ] Complete rate and volume value labels and persistence decision.
- [ ] Format timestamped transcript entries consistently.
- [ ] Implement UTF-8 `TextExportService` and `SaveFileDialog` integration.
- [ ] Report export success path; handle cancel, invalid path, access, and I/O errors.
- [ ] Add export and transcript-formatting unit tests.

## Phase 05 — Reliability and validation

- [ ] Audit every requirement and update traceability evidence.
- [ ] Test at least ten TTS start/stop and STT start/stop cycles.
- [ ] Test closing while Speaking, Paused, Listening, and Stopping.
- [ ] Test absent engine/device and file failure paths where safely reproducible.
- [ ] Verify no cross-thread UI updates, unobserved exceptions, or hanging processes.
- [ ] Review disposal ownership and event unsubscription.
- [ ] Improve messages without hiding diagnostic causes.
- [ ] Run formatting, Release build, automated tests, and full manual matrix.

## Phase 06 — Final submission

- [ ] Replace README student placeholders with verified details.
- [ ] Capture at least two authentic screenshots from the final running build.
- [ ] Update README checkboxes, limitations, setup, and actual validation results.
- [ ] Validate all links and remove stale/TODO text except genuine limitations.
- [ ] Perform clean-clone restore, Release build, tests, and run.
- [ ] Confirm no secrets, personal transcript data, build output, or fake evidence.
- [ ] Create a final submission checklist and version tag only after approval.

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
