# Final Submission Checklist

Status date: 2026-11-26 (Asia/Riyadh)

This checklist separates completed engineering evidence from owner-only or
hardware-dependent submission gates. An unchecked item must not be presented as
passed.

## Identity and course information

- [x] Replace the README student-name placeholder with the verified name `ALAA MADI`.
- [x] Omit the student-number field by the owner's explicit instruction.
- [x] Course, project title, deadline, and repository URL are documented.

## Requirements and automated validation

- [x] Every functional and non-functional requirement has a final evidence status.
- [x] Optional confidence visualization (FR-15) is explicitly Deferred, not claimed.
- [x] Locked dependency restore has passed on the development checkout.
- [x] Formatting verification has passed with no changes.
- [x] Release build has passed with 0 warnings and 0 errors.
- [x] All 90 automated tests pass (12 Core, 21 App, 57 Infrastructure).
- [x] Repeat restore/format/build/test from pushed commit `0b57412` in a clean clone.
- [x] Repeat restore/format/build/test/startup from Phase 07 commit `ccf9014` in a clean clone.

## Real Windows and hardware evidence

- [x] Windows 11 Pro 10.0.22631 and .NET SDK 10.0.402 are recorded.
- [x] Three enabled voices are genuinely enumerated.
- [x] Zero installed recognizers are genuinely recorded.
- [x] Hidden Release startup remained healthy and exact-PID cleanup left no process.
- [x] Have the owner confirm audible short and multi-sentence TTS.
- [x] Have the owner confirm voice, rate, volume, Pause, Resume, and Stop behavior.
- [x] Record the owner's successful ten-cycle and active-audio close report.
- [ ] Install/enable a compatible recognizer with owner approval or use another test PC.
- [ ] Complete real microphone recognition, timestamps, repeated cycles, and close checks.
- [ ] Complete genuine Save-dialog and live transcript export checks.

## Accessibility and layout

- [x] Earlier 125% DPI interface evidence is recorded honestly.
- [x] Record the owner's final keyboard-only navigation and access-key pass.
- [x] Record the owner's visible-focus, accessible-description, and contrast pass.
- [x] Record the owner's successful 100%/125%/150% DPI review.

## Authentic screenshots

- [x] Add `docs/screenshots/text-to-speech.png` from the real Release application.
- [ ] Add `docs/screenshots/speech-to-text.png` with genuinely recognized timestamped text.
- [x] Inspect the available TTS screenshot for cropping, legibility, accurate state, and privacy.
- [x] No mock, generated, injected-transcript, or fabricated screenshot is committed.

## Documentation and repository hygiene

- [x] README setup, build, run, test, status, and limitations are current.
- [x] Requirements, plan, testing evidence, and screenshot status agree.
- [x] All local Markdown links resolve; the repository URL was exercised by Git.
- [x] No tracked build output, test results, logs, temporary probes, or transcript TXT files exist.
- [x] No detected secret pattern, private key, API key, or machine-specific absolute path is tracked.
- [x] No personal speech or transcript content is committed.
- [x] Remove both README identity placeholders; omit the student number as instructed.

## Delivery

- [x] Push `docs/final-submission` and open PR #12 to `main`.
- [x] Confirm GitHub Actions passes on PR #12 (run `45812513776`).
- [x] Obtain owner approval and merge PR #12 (`923372a`).
- [x] Push `docs/submission-closure` and open release-evidence PR #13.
- [x] Confirm GitHub Actions passes on PR #13 (run `45815719014`).
- [x] Obtain owner approval and merge PR #13 (`2c8c8d1`).
- [ ] Push `docs/final-manual-evidence` and open the final evidence PR.
- [ ] Confirm GitHub Actions passes on the final evidence PR.
- [ ] Obtain owner approval and merge the final evidence PR.
- [ ] Create and push `v1.0.0` only after the owner explicitly approves tagging.

## Current blockers

1. This Windows installation reports zero `System.Speech` recognizers, so genuine STT,
   live transcript export, and the required STT screenshot cannot be produced.
2. Windows speech settings were reviewed, but production discovery still returns
   zero compatible recognizers. Voice Access or online speech alone is insufficient.
3. The final tag remains prohibited until the final PR is merged and the owner gives
   separate explicit tag approval.
