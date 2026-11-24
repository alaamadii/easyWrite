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
