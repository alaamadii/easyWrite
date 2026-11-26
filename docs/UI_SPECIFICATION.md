# Frontend / UI Specification

## Design direction

EasyWrite should look calm and purposeful rather than like a default control dump.
Use a desktop-first layout, Segoe UI, generous spacing, a restrained blue accent,
and clear status text. Icons may support labels but never replace them.

## Window shell

| Property | Specification |
| --- | --- |
| Title | `EasyWrite — Speech Studio` |
| Initial size | Approximately 1180 × 760 logical pixels |
| Minimum size | 960 × 640 |
| Start position | Center screen |
| Resize behavior | Editors expand; action rows remain usable; no overlapping controls |

### Global layout

```text
┌──────────────────────────────────────────────────────────────────┐
│ EasyWrite                                      Speech Studio     │
│ Write it. Hear it. Say it. See it.                              │
├──────────────────────────────────────────────────────────────────┤
│ [ Text to Speech ] [ Speech to Text ]                            │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│                    selected module content                       │
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│ ● Ready                    Local Windows speech • Privacy first  │
└──────────────────────────────────────────────────────────────────┘
```

The bottom status strip is persistent. The state indicator combines shape/color
with text: gray Ready, blue Speaking, amber Paused, red Listening, muted Stopping,
and red Error. Exact colors must meet readable contrast targets.

## Text-to-Speech tab

```text
┌─ Text to Speech ─────────────────────────────────────────────────┐
│ Turn written text into audio using an installed Windows voice.  │
│                                                                  │
│ Text to speak                                      0 characters  │
│ ┌──────────────────────────────────────────────────────────────┐ │
│ │ Type or paste text here…                                    │ │
│ │                                                              │ │
│ └──────────────────────────────────────────────────────────────┘ │
│                                                                  │
│ Voice [ System default ▼ ] Rate [────●────] 0 Volume [──●─] 100 │
│                                                                  │
│ [▶ Speak] [⏸ Pause / ▶ Resume] [■ Stop]              [Clear]    │
│                                                                  │
│ Tip / validation / engine availability message                  │
└──────────────────────────────────────────────────────────────────┘
```

### Elements

| Element | Control | Initial behavior |
| --- | --- | --- |
| Text label and character count | `Label` | Count updates as text changes. |
| Text editor | `RichTextBox` | Multiline, accepts return/tab as appropriate, anchored on all sides. |
| Voice | `ComboBox` | Drop-down list; shows enabled installed voices or an unavailable message. |
| Rate | `TrackBar` + value label | Range -10 to 10, default 0. |
| Volume | `TrackBar` + value label | Range 0 to 100, default 100. |
| Speak | `Button` | Default action when valid and Idle. |
| Pause/Resume | `Button` | Disabled until Speaking; label/action changes in Paused. |
| Stop | `Button` | Enabled only while Speaking or Paused. |
| Clear | `Button` | Enabled when editor is non-empty; safely resolves active speech. |
| Inline message | `Label` | Input validation and capability guidance; screen-reader meaningful. |

## Speech-to-Text tab

```text
┌─ Speech to Text ─────────────────────────────────────────────────┐
│ Dictate through your microphone using an installed recognizer.  │
│                                                                  │
│ Language [ English (United States) ▼ ]     ● Microphone is off   │
│                                                                  │
│ [🎙 Start listening] [■ Stop listening]                          │
│                                                                  │
│ Transcript                                          0 results    │
│ ┌──────────────────────────────────────────────────────────────┐ │
│ │ [14:32:08] Recognized words appear here.                    │ │
│ │                                                              │ │
│ └──────────────────────────────────────────────────────────────┘ │
│                                                                  │
│ [Clear transcript]                              [Export TXT…]    │
│ Recognition/capability guidance                                  │
└──────────────────────────────────────────────────────────────────┘
```

### Elements

| Element | Control | Initial behavior |
| --- | --- | --- |
| Language | `ComboBox` | Lists installed recognizer cultures; no unsupported static list. |
| Microphone indicator | custom panel/labels | Text and visual state always agree with engine state. |
| Start listening | `Button` | Explicit opt-in; enabled only with a valid recognizer and Idle state. |
| Stop listening | `Button` | Enabled only while Listening; disabled while Stopping. |
| Transcript | read-only `RichTextBox` or structured panel | Appends finalized results and timestamps; supports selection/copy. |
| Result count | `Label` | Counts finalized recognition results. |
| Clear transcript | `Button` | Clears displayed/in-memory results after listening is stopped. |
| Export TXT | `Button` + `SaveFileDialog` | Enabled with results; suggests `.txt`; UTF-8 output. |
| Inline message | `Label` | Explains setup/permission failures with a recovery action. |

## Interaction and control-state matrix

| State | Speak | Pause/Resume | Stop TTS | Start STT | Stop STT | Editors/settings |
| --- | --- | --- | --- | --- | --- | --- |
| Idle | Valid text only | Disabled | Disabled | Valid recognizer only | Disabled | Enabled |
| Speaking | Disabled | Pause enabled | Enabled | Disabled | Disabled | TTS text/settings locked |
| Paused | Disabled | Resume enabled | Enabled | Disabled | Disabled | TTS text/settings locked |
| Listening | Disabled | Disabled | Disabled | Disabled | Enabled | STT language locked |
| Stopping | Disabled | Disabled | Disabled | Disabled | Disabled | Locked until settled |
| Error | Depends on recovery | Disabled | Disabled | Depends on recovery | Disabled | Safe controls enabled |

## Keyboard and accessibility

- Logical tab order follows tabs → editor/settings → primary actions → secondary
  actions → status.
- Buttons include text and access keys where natural (`&Speak`, `S&top`).
- `Ctrl+L` may start listening only while the STT tab is selected and capable;
  Escape stops the active operation after the shortcut is documented in UI.
- Status changes are reflected in accessible control names/descriptions.
- Do not use color alone. Provide visible focus cues and avoid tiny icon-only hit
  targets.
- Respect Windows DPI scaling and test at 100%, 125%, and 150%.

## Responsive Windows Forms layout

Use nested `TableLayoutPanel` and `FlowLayoutPanel` containers with docking and
anchoring. Avoid absolute positioning for the final interface except inside small,
fixed decorative components. At minimum size, action groups may wrap but must not
clip.

## Error and empty states

- Empty TTS: prompt the user to enter text; do not open an error dialog.
- No voices: disable Speak and explain how to install a Windows voice.
- No recognizers: disable listening and explain Windows language prerequisites.
- Runtime hardware/file errors: show concise inline feedback; reserve modal dialogs
  for failures that require acknowledgement.
- Empty transcript export: keep Export disabled rather than reporting an avoidable
  error.

## Screenshot plan

Final authentic screenshots must be captured from the running app and stored in
`docs/screenshots/`:

1. `text-to-speech.png` — populated TTS tab with settings and Ready/Speaking status.
2. `speech-to-text.png` — populated transcript with timestamped results and visible
   microphone state.

Optional screenshots may document error handling. Do not capture personal content,
machine usernames, unrelated windows, or simulated results presented as real.

### Final screenshot status — 2026-11-26

Neither planned screenshot is committed yet. The TTS capture requires an
interactive Release session. The STT capture is blocked by the genuine installed
recognizer count of zero and must not be replaced with typed or injected transcript
content. See [the screenshot evidence record](screenshots/README.md).
