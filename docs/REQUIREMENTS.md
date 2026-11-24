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
`Manual passed`, `Manual failed`, or `Not run`. A planned test is not evidence of
completion.

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
