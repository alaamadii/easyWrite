# Development Guide

## Environment setup

1. Use Windows 10/11 with .NET SDK 10.0.402 or a later 10.0 patch.
2. Clone the repository and work on a branch; never implement directly on `main`.
3. Restore once, then build and test from the repository root:

```powershell
dotnet restore EasyWrite.slnx
dotnet build EasyWrite.slnx --configuration Release --no-restore
dotnet test EasyWrite.slnx --configuration Release --no-build
```

Run the current desktop application with:

```powershell
dotnet run --project src/EasyWrite.App/EasyWrite.App.csproj
```

## Project conventions

- File-scoped namespaces, nullable annotations, implicit usings, and warnings as
  errors are enabled repository-wide.
- One public type per file unless small private/nested types are clearer.
- Use meaningful behavior names (`StartListeningAsync`, not `DoSpeech`).
- Prefer immutable records for values and narrow interfaces for boundaries.
- Validate at the boundary closest to the invalid input.
- Async methods accept `CancellationToken` when cancellation is meaningful.
- Do not add a framework or pattern solely to make the architecture look complex.

## Adding behavior

1. Identify the requirement/user story and its acceptance criteria.
2. Decide which layer owns the behavior using `ARCHITECTURE.md`.
3. Add/update a core contract or model only when the use case requires it.
4. Implement the infrastructure adapter without leaking platform types into Core.
5. Connect it through the App composition root and render service-reported state.
6. Add automated tests for deterministic behavior and manual checks for hardware.
7. Update requirement evidence, decisions, limitations, and README status.

## UI rules

- UI event handlers validate, call a service, and render state; they do not own a
  speech engine or contain file-format logic.
- Touch controls only from the UI thread.
- Disable invalid/re-entrant actions based on application state.
- Explain unavailable features inline and keep the rest of the app usable.
- Form closing must stop accepting input before asynchronous cleanup begins.

## Speech adapter rules

- Query installed voices/recognizers; do not assume names or cultures.
- Use asynchronous recognition/synthesis APIs for long-running work.
- Subscribe and unsubscribe events in the same owning class.
- Preserve original exception details while returning a friendly recovery message.
- Make `Dispose` safe to call more than once.
- Never claim success when an engine or device is absent.

## Git and pull requests

```powershell
git switch main
git pull --ff-only
git switch -c feat/example
# implement and validate
git add -- <intentional files>
git diff --cached --check
git commit -m "feat(scope): describe behavior"
git push -u origin feat/example
gh pr create --base main --fill
```

Recommended commit types are `feat`, `fix`, `test`, `docs`, `refactor`, and `chore`.
PR descriptions must include requirement IDs, actual validation output, manual checks
not run, risks, and screenshots for visible UI changes.

## Updating packages

Versions are centralized in `Directory.Packages.props`. Package updates require a
successful clean restore/build/test and review of Windows/.NET compatibility. Do
not add cloud speech SDKs unless local recognition is proven unsuitable and the
project owner explicitly approves the architectural change.

## Troubleshooting

- **SDK mismatch:** run `dotnet --info`; install the pinned .NET 10 SDK or adjust
  `global.json` only in a reviewed maintenance change.
- **No voice/recognizer:** install the relevant Windows speech/language capability;
  the SDK package alone does not install an engine.
- **Microphone failure:** verify input device, desktop-app microphone permission,
  exclusive-device conflicts, and selected recognizer culture.
- **CI cannot test audio:** expected; CI proves code-level checks only. Run the
  documented manual matrix on a physical Windows machine.
