# Screenshots

This folder contains authentic application evidence captured from the real Release
build on 2026-11-26. Images must not be mockups, generated images, injected
transcripts, or fabricated evidence.

Evidence files:

- `text-to-speech.png` — passed visual inspection. The 1182×799 window-only capture
  shows non-personal text, Microsoft Zira Desktop, rate 0, volume 100, enabled
  Resume/Stop controls, inline paused feedback, and the global Paused status. The
  application was genuinely paused during capture and playback was stopped afterward.
- `speech-to-text.png` — blocked because the current machine has zero installed
  recognizers and cannot produce a genuine timestamped transcript.

The TTS image SHA-256 is
`CAEDD651F6193CB77A58F0656DE63A590BA489A3CF5DCA2A38915E75A9BE259D`.

An unavailable-recognizer screen is valid error-state evidence but is not a
replacement for the required genuine speech-to-text screenshot. Install/enable a
compatible recognizer with owner approval or use another Windows test machine,
then capture short non-personal recognized text.

Before committing, crop unrelated windows and verify that no personal paths,
notifications, or private transcript text are visible.
