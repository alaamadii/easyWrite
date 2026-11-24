# EasyWrite

EasyWrite is a Windows desktop application that converts written text into spoken
audio and microphone speech into written text. It is Project 04 for the YMH219
Object-Oriented Programming course.

> **Current status:** Phase 02 text-to-speech implementation complete and awaiting
> audible hardware sign-off. EasyWrite discovers enabled Windows voices and provides
> asynchronous Speak, Pause/Resume, Stop, rate, volume, cancellation, status, and
> safe shutdown behavior. Microphone recognition and TXT export remain deferred. See
> [Project Plan](docs/PROJECT_PLAN.md).

## Course information

| Field | Value |
| --- | --- |
| Course | YMH219 — Object-Oriented Programming |
| Project | 04 — Speech-to-Text and Text-to-Speech Application |
| Student | **TODO: add student full name** |
| Student number | **TODO: add student number** |
| Deadline | November 30, 2026 at 23:59 (Asia/Riyadh) |
| Repository | <https://github.com/alaamadii/easyWrite> |

## Product goal

EasyWrite provides an understandable, local-first speech workspace with two
focused experiences:

- **Text to Speech:** type or paste text, choose an installed voice, adjust rate
  and volume, then speak, pause/resume, stop, or clear.
- **Speech to Text:** explicitly start the microphone, watch the listening state,
  receive timestamped recognition results, stop safely, clear, or export to TXT.

Mandatory functionality is prioritized. Voice settings, TXT export, and
timestamps are planned after the basic workflows work reliably.

## Technology

| Technology | Role |
| --- | --- |
| C# / .NET 10 | Language and application runtime |
| Windows Forms | Windows desktop interface and event-driven controls |
| `System.Speech` 11 | Local Windows speech synthesis and recognition |
| xUnit | Unit testing of platform-independent behavior |
| GitHub Actions | Restore, build, and automated test validation |

`System.Speech` uses speech engines installed in Windows. It is Windows-only and
does not guarantee that a recognizer or voice exists for every language. EasyWrite
must detect available engines at runtime and show actionable errors instead of
simulating success.

## Architecture

```text
EasyWrite.App (WinForms views and event handlers)
        │
        ├──────► EasyWrite.Core (models, events, service contracts)
        │
        └──────► EasyWrite.Infrastructure (System.Speech and file adapters)
                         │
                         └──────► EasyWrite.Core

EasyWrite.Core.Tests ───────────► EasyWrite.Core
```

This separation keeps form code focused on presentation and permits core rules to
be tested without a microphone, speaker, or Windows speech engine. See the full
[architecture description](docs/ARCHITECTURE.md).

## Repository layout

```text
src/
  EasyWrite.App/             Windows Forms executable
  EasyWrite.Core/            Models, events, and service abstractions
  EasyWrite.Infrastructure/  Windows speech and file integrations
tests/
  EasyWrite.App.Tests/       Deterministic presentation-state tests
  EasyWrite.Core.Tests/      Fast unit tests
  EasyWrite.Infrastructure.Tests/ Deterministic TTS lifecycle tests
docs/                        Product and engineering documentation
.github/                     CI and pull-request guidance
```

## Prerequisites

- Windows 10 or Windows 11
- .NET SDK 10.0.402 or a later 10.0 patch
- A working audio output device for text-to-speech
- A working microphone with Windows permission enabled for speech-to-text
- At least one installed Windows voice and one recognizer for the desired language

Check installed voices under Windows **Settings > Time & language > Speech**.
Recognition capabilities vary by Windows edition, installed language packs, and
system configuration.

## Build and run

```powershell
git clone https://github.com/alaamadii/easyWrite.git
cd easyWrite
dotnet restore EasyWrite.slnx
dotnet build EasyWrite.slnx --configuration Release --no-restore
dotnet run --project src/EasyWrite.App/EasyWrite.App.csproj
```

The application opens the two-tab workspace. Text-to-speech controls use enabled
voices discovered from Windows at runtime. If initialization or discovery fails,
the application still opens and explains how to recover. Speech-to-text and export
controls remain disabled until their later phases.

## Test

```powershell
dotnet test EasyWrite.slnx --configuration Release
```

Manual speech tests require real Windows audio hardware and installed engines;
they are tracked separately in [Testing](docs/TESTING.md).

## Planned features

- [x] Complete two-tab Windows Forms interface
- [x] Local text-to-speech with speak, pause/resume, and stop
- [ ] Microphone speech-to-text with explicit start and stop
- [x] Clear actions and visible operation status
- [x] Voice, rate, and volume controls
- [ ] Timestamped recognition results
- [ ] TXT export
- [ ] Graceful microphone, recognition-engine, and file errors
- [x] Graceful TTS voice, state, initialization, and audio errors
- [ ] Final accessibility pass and authentic screenshots

## Known limitations

- Windows is the only supported operating system.
- Speech quality and supported languages depend on locally installed engines.
- `System.Speech` is a compatibility-focused Windows library and is not a cloud AI
  transcription service.
- Automated CI can validate code but cannot prove microphone or speaker behavior.
- Audible TTS output still needs a human hardware check; the assistant-run Phase 02
  validation confirmed startup and real voice discovery but did not claim hearing audio.
- Final submission screenshots are not included yet because the speech workflows
  are not complete. Phase 01 UI evidence must not be presented as final behavior.

## Documentation

- [Project brief](docs/PROJECT_BRIEF.md)
- [Requirements and traceability](docs/REQUIREMENTS.md)
- [User stories](docs/USER_STORIES.md)
- [Architecture](docs/ARCHITECTURE.md)
- [UI specification](docs/UI_SPECIFICATION.md)
- [Project plan and task list](docs/PROJECT_PLAN.md)
- [Development guide](docs/DEVELOPMENT.md)
- [Testing strategy](docs/TESTING.md)
- [Architecture decisions](docs/DECISIONS.md)

## Academic integrity and evidence

Documentation must describe the repository as it actually exists. Do not claim a
feature is complete until it has been implemented and verified. Do not add mock or
AI-generated images as application screenshots; final evidence must be captured
from the running EasyWrite application.

