# EasyWrite Agent Guide

These instructions apply to the entire repository.

## Mission

Build a clear, reliable university-level Windows Forms speech application while
keeping the code easy for the student to explain. Implement mandatory assignment
requirements before optional enhancements.

## Engineering rules

- Target .NET 10 and Windows; keep nullable reference types and warnings-as-errors
  enabled.
- Keep UI code in `EasyWrite.App`, contracts and models in `EasyWrite.Core`, and
  Windows/file integrations in `EasyWrite.Infrastructure`.
- Do not reference Windows Forms or `System.Speech` from `EasyWrite.Core`.
- Put speech-engine lifecycle and disposal inside infrastructure services.
- Keep event handlers small: validate input, call a service, and update view state.
- Never block the UI thread for speech, recognition, file I/O, or shutdown.
- Represent state explicitly and ensure controls match the current operation.
- Never silently simulate a microphone, recognizer, voice, or successful export.
- Show friendly user messages, but retain the original exception for diagnostics.
- Add or update tests and documentation whenever behavior changes.
- Do not add secrets, machine-specific paths, generated build output, or fabricated
  screenshots.

## Definition of done

A change is complete only when:

1. Its acceptance criteria are met.
2. `dotnet build EasyWrite.slnx --configuration Release` succeeds.
3. Relevant automated tests pass.
4. Relevant manual checks are recorded honestly.
5. Requirements, decisions, and README status are updated when applicable.
6. The diff contains no unrelated changes or generated output.

## Git workflow

- Branch from an up-to-date `main`.
- Use `feat/`, `fix/`, `docs/`, `test/`, or `chore/` branch prefixes.
- Prefer focused conventional commits such as `feat(tts): add pause and resume`.
- Open a pull request to `main`; do not merge without explicit owner approval.
- Never force-push, delete remote work, or rewrite shared history without approval.

## Validation boundaries

CI validates restore, build, and unit tests. Audio output, microphone input,
installed-language behavior, and final screenshots require a Windows manual test on
actual hardware. Record unavailable checks as not run, never as passed.
