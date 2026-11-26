# Requirements and Traceability

## Priority definitions

- **Must:** required by the assignment or necessary for safe operation.
- **Should:** professor-suggested enhancement planned after all Must items.
- **Could:** useful polish only if time and reliability permit.

## Functional requirements

| ID | Priority | Requirement | Acceptance criteria | Phase |
| --- | --- | --- | --- | --- |
| FR-01 | Must | Convert entered text to spoken audio. | Given non-empty text and an installed voice, Speak produces audible output without freezing the UI. | 2 |
| FR-02 | Must | Start text-to-speech. | Speak validates input, starts one operation, changes status to Speaking, and disables conflicting actions. | 2 |
| FR-03 | Must | Pause/resume and stop speech. | Pause halts active output, Resume continues it, and Stop cancels queued output and returns to Idle. | 2 |
| FR-04 | Must | Clear text. | Clear empties the relevant editor after required state handling and focuses it. | 1–3 |
| FR-05 | Must | Convert microphone speech to text. | With a compatible recognizer and microphone, Start Listening appends finalized recognized speech. | 3 |
| FR-06 | Must | Handle microphone and speech failures. | Missing engines/devices, denied access, invalid state, and service errors produce friendly feedback and recoverable state. | 3, 5 |
| FR-07 | Must | Display active operation. | Persistent status and contextual controls distinguish Idle, Speaking, Paused, Listening, Stopping, and Error. | 1–3 |
| FR-08 | Must | Provide a Windows Forms desktop UI. | The app starts as a .NET 10 Windows Forms executable and provides all mandatory controls. | 1 |
| FR-09 | Must | Prevent conflicting speech operations. | Starting one direction safely stops or rejects the other; microphone and synthesis never run accidentally together. | 2–3 |
| FR-10 | Must | Shut down safely. | Closing cancels active work, unsubscribes events, and disposes speech resources without hanging. | 5 |
| FR-11 | Should | Select an installed voice. | The voice list shows enabled installed voices and the selection is used for subsequent speech. | 4 |
| FR-12 | Should | Adjust speech rate and volume. | Supported rate (-10–10) and volume (0–100) values are applied and visible. | 4 |
| FR-13 | Should | Export transcript to TXT. | The user chooses a path; UTF-8 text is saved, cancellation changes nothing, and I/O failures are explained. | 4 |
| FR-14 | Should | Timestamp recognition results. | Each finalized result shows the local completion time in a consistent format. | 4 |
| FR-15 | Could | Show recognition confidence. | Low-confidence results can be visually identified without hiding recognized text. | 6 |

## Non-functional requirements

| ID | Category | Requirement / measurable target |
| --- | --- | --- |
| NFR-01 | Usability | Primary actions are visible without menus and labels use consistent, understandable English. |
| NFR-02 | Responsiveness | No speech or file operation blocks the UI thread; controls continue repainting during work. |
| NFR-03 | Accessibility | Logical tab order, keyboard access, text labels in addition to color, and adequate contrast are required. |
| NFR-04 | Privacy | Microphone capture starts only after an explicit click and Listening remains visibly indicated. |
| NFR-05 | Reliability | Repeated start/stop cycles and form closing release resources and do not leave an invalid UI state. |
| NFR-06 | Maintainability | Core has no Windows Forms or System.Speech dependency; behavior is expressed through focused interfaces. |
| NFR-07 | Testability | Core validation/state rules receive automated tests; hardware behavior has a documented manual matrix. |
| NFR-08 | Portability | The product supports Windows only; the limitation is explicit in setup documentation. |
| NFR-09 | Security | No microphone data is intentionally persisted or transmitted; no secrets/API keys are required. |
| NFR-10 | Diagnostics | User-facing errors include a recovery action; development diagnostics retain underlying exceptions. |

## Course-concept traceability

| Course concept | Evidence planned |
| --- | --- |
| Encapsulation | Speech engine instances stay private inside infrastructure services. |
| Abstraction | Core service interfaces isolate forms from `System.Speech`. |
| Inheritance | Windows Forms views derive from `Form`/`UserControl`; unnecessary custom inheritance is avoided. |
| Polymorphism | UI consumes the text-to-speech, speech-to-text, and export interfaces. |
| Events | Controls trigger commands; services report results and state changes through .NET events. |
| Exception handling | Engine, audio, permission, state, and file exceptions map to actionable feedback. |
| Resource management | Engines, cancellation sources, event subscriptions, and streams have explicit ownership/disposal. |

## Verification legend

The implementation PR for each requirement must mark evidence as `Automated`,
`Manual passed`, `Manual failed`, `Blocked`, or `Not run`. A planned test is not
evidence of completion.

## Phase 02 evidence — 2026-11-24

| ID | Status | Evidence |
| --- | --- | --- |
| FR-01 | Automated; audible portion not run | Real `SpeechSynthesizer.SpeakAsync` integration and completion task implemented; human audible verification remains required. |
| FR-02 | Automated | Blank input, settings, one-active-operation guard, Speaking state, and UI-state behavior covered by Core/App/Infrastructure tests. |
| FR-03 | Automated; audible portion not run | Pause, Resume, Stop, cancellation, and state sequences covered with a deterministic adapter; audible continuity remains unverified. |
| FR-04 | Automated/UI evidence from Phase 01 | Clear remains available only in valid states and returns focus to the editor. |
| FR-06 | Partial, automated for TTS | Missing/invalid voice, initialization, playback, and disposal failures have recovery messages; microphone cases remain Phase 03/05. |
| FR-07 | Automated for TTS | Speaking, Paused, Stopping, Idle, and Error update the shared status and controls. Listening remains deferred. |
| FR-09 | Automated for TTS | Re-entrant Speak is rejected and TTS locks conflicting controls. Cross-direction behavior awaits STT. |
| FR-10 | Automated plus startup-process check | Form shutdown prevents commands, unsubscribes, stops, and Program disposes the service. Close-during-audio remains not run. |
| FR-11 | Automated plus real discovery probe | Enabled voices are sorted, displayed, selected, and forwarded; three real voices were detected on the development machine. |
| FR-12 | Automated | Rate -10–10 and volume 0–100 validation and adapter forwarding are covered. Audible perception is not run. |
| NFR-02 | Automated design evidence | Only asynchronous synthesis is used; no `.Wait()`, `.Result`, or synchronous `Speak` is used. Human responsiveness during audio is not run. |
| NFR-05 | Partial | Re-entry, cancellation, late callback, repeated disposal, and post-dispose cases pass; repeated audible cycles remain not run. |
| NFR-06 | Automated/build | `System.Speech` remains private to Infrastructure behind Core contracts and an internal adapter. |
| NFR-07 | Automated/manual separation | 37 hardware-independent tests pass; real voice discovery is recorded separately from audible evidence. |
| NFR-10 | Automated/code review | User messages are concise and wrapped failures preserve the original exception as `InnerException`. |

## Phase 03 evidence — 2026-11-24

| ID | Status | Evidence |
| --- | --- | --- |
| FR-05 | Automated; microphone portion blocked | Real `SpeechRecognitionEngine` integration, dictation grammar, finalized-result mapping, and append flow are implemented. The machine has zero installed recognizers, so spoken recognition was not run. |
| FR-06 | Automated for STT failure paths | Missing/invalid recognizer, discovery, grammar/input startup, cancellation, runtime, stop, and disposal failures have actionable messages with preserved inner exceptions. Real permission/device failures were not safely reproducible. |
| FR-07 | Automated for STT | Listening, Stopping, Idle, and Error drive the shared status, microphone presentation, and conflict controls. |
| FR-09 | Automated/UI-state evidence | Listening disables TTS start/playback actions; Speaking/Paused disable recognition start; Stopping blocks both directions. |
| FR-10 | Automated plus unavailable-state startup | Shutdown unsubscribes UI handlers and Program deterministically disposes both services. Close during real Listening/Stopping is blocked by the absence of a recognizer. |
| NFR-02 | Automated design evidence | Recognition uses `RecognizeAsync` in multiple mode; no blocking waits or synchronous recognition are used. Real microphone responsiveness is blocked. |
| NFR-04 | Automated/code review | Microphone setup occurs only after Start Listening; discovery and startup do not silently activate recording. |
| NFR-05 | Partial | Re-entry, repeated stop, cancellation, late callbacks, repeated disposal, and post-dispose cases pass; repeated hardware cycles remain blocked. |
| NFR-06 | Automated/build | `System.Speech` recognition types remain private to Infrastructure behind Core contracts and internal adapters. |
| NFR-07 | Automated/manual separation | 60 hardware-independent tests pass; a separate real discovery probe reported zero recognizers. |
| NFR-09 | Automated/code review | Recognition remains local and no audio or transcript is persisted or transmitted. |
| NFR-10 | Automated | Friendly recovery messages wrap engine failures while preserving source exceptions for diagnostics. |

## Phase 04 evidence — 2026-11-25

| ID | Status | Evidence |
| --- | --- | --- |
| FR-04 | Automated | Clear removes both visible transcript content and the authoritative in-memory result collection, immediately disabling export. |
| FR-06 | Automated for export failures | Invalid destinations, missing directories, cancellation, and I/O failures preserve transcript state and provide recovery messages with inner exceptions. |
| FR-07 | Automated/UI-state evidence | Export busy state prevents repeated export, transcript clearing, and recognition start without changing the speech-operation vocabulary. |
| FR-10 | Automated/code review | Form closing cancels an active export; temporary files are cleaned and UI continuations check shutdown/disposal. |
| FR-11 | Audited | Real enabled voice selection and fallback behavior remain implemented and covered by Phase 02 tests. |
| FR-12 | Audited | Rate -10–10 and volume 0–100 labels, validation, and forwarding remain covered; persistence is intentionally not required. |
| FR-13 | Automated; live UI path blocked | `TextExportService` writes ordered UTF-8 without BOM through an atomic temporary-file replacement. SaveFileDialog uses an approved TXT path. Genuine UI export is blocked by zero recognizers. |
| FR-14 | Automated; live recognition blocked | UI and export share `TranscriptFormatter`, using each result's local timestamp in `[HH:mm:ss]` format. Genuine recognition display is blocked by zero recognizers. |
| NFR-02 | Automated | Export uses asynchronous stream/writer APIs and the UI awaits without blocking. |
| NFR-05 | Automated | Repeated sequential exports, busy-state prevention, cancellation cleanup, failure cleanup, and file-resource release pass. |
| NFR-06 | Automated/build | Formatting remains in Core, file I/O in Infrastructure, and dialog coordination in App. |
| NFR-07 | Automated/manual separation | 84 hardware-independent tests pass, including real temporary-directory UTF-8 output; live UI export is recorded separately as blocked. |
| NFR-09 | Automated/code review | Files are created only after a user-approved path; there is no automatic persistence, cloud storage, or transcript logging. |
| NFR-10 | Automated | Wrapped file failures retain original exceptions while UI messages remain concise. |

## Phase 05 reliability-audit evidence — 2026-11-26

| ID | Status | Evidence |
| --- | --- | --- |
| FR-01 | Automated; audible portion not run | Asynchronous synthesis and completion remain covered. No human listener verified audio during Phase 05. |
| FR-02 | Automated | Input/settings validation, one-active-operation guards, Speaking state, and ten deterministic start/stop cycles pass. |
| FR-03 | Automated; audible portion not run | Pause, Resume, Stop, cancellation, and disposal while Paused/Stopping pass; audible continuity was not run. |
| FR-04 | Automated plus prior UI evidence | Text/transcript clearing and focus behavior remain implemented; transcript clear updates both presentation and stored results. |
| FR-05 | Automated; microphone portion blocked | Ten fake-engine sessions and finalized-result ordering pass. The genuine recognizer count is 0, so microphone recognition is blocked. |
| FR-06 | Automated; destructive machine changes not run | Discovery, startup, runtime, stop, cancellation, and file failures retain causes and provide recovery guidance. Permissions/devices were not reconfigured. |
| FR-07 | Automated plus prior UI evidence | All operation states map to text, accessible descriptions, and valid controls; no color-only state is required. |
| FR-08 | Startup passed; interactive review not run | The Release WinForms executable remained healthy for three seconds. The hidden process had no window handle, so interactive rendering was not re-claimed. |
| FR-09 | Automated | The centralized mapper continues to reject conflicting TTS/STT actions and export re-entry. |
| FR-10 | Automated; active hardware close not run/blocked | Disposal while speech is Speaking/Paused/Stopping and recognition is Listening/Stopping passes with late callbacks ignored. Real listening close is blocked. |
| FR-11 | Automated plus real discovery | Three enabled voices were genuinely enumerated: Microsoft Zira Desktop, Microsoft Tolga, and Microsoft Naayf. Audible selection was not run. |
| FR-12 | Automated; audible portion not run | Rate/volume ranges, labels, and adapter forwarding pass; audible perception was not run. |
| FR-13 | Automated; Save dialog not run | Ten sequential exports, recovery after failure, cancellation, UTF-8, replacement, cleanup, and released handles pass. Success feedback now includes the chosen path. |
| FR-14 | Automated; live recognition blocked | Shared local `[HH:mm:ss]` formatting and ordering pass; genuine recognized timestamps remain blocked by zero recognizers. |
| FR-15 | Deferred | Recognition confidence remains in the model but has no optional visual treatment; this Could requirement is not claimed. |
| NFR-01 | Automated/code review plus prior UI evidence | Primary controls and consistent English labels remain visible; final interactive review is outstanding. |
| NFR-02 | Automated/code review | No production `.Wait()`, `.Result`, synchronous speech/recognition, or synchronous long-running export path was found. |
| NFR-03 | Partial manual/code review | Text accompanies state color and controls have tab indices/accessibility names. Prior 125% evidence exists; final keyboard and 100%/150% checks are not run. |
| NFR-04 | Automated/code review | Microphone setup still begins only after explicit Start Listening and the Listening state remains textual. |
| NFR-05 | Automated; hardware cycles not run/blocked | Ten deterministic TTS/STT/export cycles, paused/stopping disposal, late callbacks, and process cleanup pass. Real STT cycles are blocked. |
| NFR-06 | Automated/build | Layer boundaries remain intact; no Windows Forms or `System.Speech` reference was added to Core. |
| NFR-07 | Automated/manual separation | 90 hardware-independent tests pass. Hardware, DPI, keyboard, and audio evidence remains explicitly separate. |
| NFR-08 | Documented | README prerequisites and limitations continue to state Windows-only support. |
| NFR-09 | Automated/code review | No audio is persisted or transmitted; transcript export remains explicitly user initiated; no secrets were added. |
| NFR-10 | Automated/code review | Wrapped infrastructure errors retain inner exceptions, and caught UI failures are now written to Debug without logging transcript content. |

## Phase 06 final-submission matrix — 2026-11-26

This matrix is the final documentation audit. It does not replace the detailed
phase evidence above or turn unavailable hardware checks into passes.

| ID | Final status | Final evidence or remaining check |
| --- | --- | --- |
| FR-01 | Automated; audible portion not run | Asynchronous synthesis and completion pass; a human listener has not verified sound. |
| FR-02 | Automated | Validation, settings, operation guards, and Speaking-state behavior pass. |
| FR-03 | Automated; audible portion not run | Pause, resume, stop, cancellation, and disposal pass; audible continuity remains unverified. |
| FR-04 | Automated plus prior UI evidence | Editor and transcript clearing, stored-result clearing, and focus behavior are implemented. |
| FR-05 | Automated; real recognition blocked | Recognition lifecycle/result mapping pass with test adapters; the final machine has zero installed recognizers. |
| FR-06 | Automated; real device/permission checks not run | Failure mappings preserve source exceptions and recovery guidance; machine devices and permissions were not altered. |
| FR-07 | Automated plus prior UI evidence | Explicit textual states and valid control mappings pass without relying only on color. |
| FR-08 | Release startup passed; final interactive review not run | The application launches, but final visible layout and interaction still require an interactive desktop review. |
| FR-09 | Automated | Centralized state mapping prevents conflicting TTS, STT, and export operations. |
| FR-10 | Automated; active hardware close not run/blocked | Deterministic disposal and late-callback protection pass; real listening close is blocked. |
| FR-11 | Automated plus real discovery; audible selection not run | Three enabled voices were genuinely enumerated and selection forwarding passes. |
| FR-12 | Automated; audible portion not run | Rate/volume validation, labels, and forwarding pass; perception remains unverified. |
| FR-13 | Automated; live dialog path not run | Real temporary-file UTF-8 exports, replacement, cleanup, retry, and path feedback pass; interactive Save dialog evidence is pending. |
| FR-14 | Automated; genuine recognized timestamps blocked | Shared local `[HH:mm:ss]` formatting and ordering pass; no real recognizer is installed. |
| FR-15 | Deferred optional requirement | Confidence remains modeled but has no optional UI visualization and is not claimed. |
| NFR-01 | Code review plus prior UI evidence; final interactive review pending | Primary controls and consistent English labels are present. |
| NFR-02 | Automated/code review | Speech, recognition, export, and shutdown avoid blocking waits and synchronous long operations. |
| NFR-03 | Partial manual/code review | Accessible names, tab order, and text state exist; final keyboard-only and 100%/150% DPI checks are not run. |
| NFR-04 | Automated/code review | Microphone setup begins only after the explicit Start Listening action. |
| NFR-05 | Automated; hardware repetition incomplete | Ten deterministic TTS/STT/export cycles and cleanup scenarios pass; real STT cycles are blocked. |
| NFR-06 | Automated/build | App, Core, and Infrastructure boundaries compile cleanly; Core does not reference WinForms or `System.Speech`. |
| NFR-07 | Automated/manual separation | 90 hardware-independent tests pass; unavailable manual checks remain separately identified. |
| NFR-08 | Documented | Windows/.NET prerequisites and hardware limitations are stated in the README. |
| NFR-09 | Automated/code review and hygiene audit | Speech stays local, export is user initiated, and no secrets or generated output are tracked. |
| NFR-10 | Automated/code review | Friendly UI messages retain original exceptions for diagnostics without logging transcript content. |

## Phase 07 closure status — 2026-11-26

PR #12 and its successful Windows CI run are merged into `main` at
`923372ab54a9b59269d97ce5b2e217ccba23d5c7`. Phase 07 repeated locked restore,
format verification, the Release build, and all 90 automated tests successfully.
Read-only Windows discovery again returned three enabled voices and zero installed
recognizers without starting audio or microphone input.

No requirement status in the Phase 06 matrix is promoted by those checks. Audible
FR-01/FR-03/FR-11/FR-12 evidence remains not run; real FR-05/FR-10/FR-13/FR-14
evidence remains blocked by the missing recognizer; and final NFR-01/NFR-03/NFR-05
interactive evidence remains incomplete. FR-15 remains deferred.
