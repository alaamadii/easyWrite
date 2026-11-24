# Project Brief

## Background

EasyWrite is the fourth of five YMH219 Object-Oriented Programming assignments,
following Flappy Bird, Key Event Logger, and a client-server chat application. The
course project requires an event-driven C# Windows Forms program that demonstrates
OOP, external library use, resource management, error handling, and user feedback.

## Problem

Users need a simple desktop tool for two opposite tasks: hearing written text and
turning live speech into readable text. Generic operating-system settings expose
speech capabilities, but they do not provide a focused course-sized workspace with
clear controls, visible state, timestamps, and a simple export flow.

## Proposed solution

Create a single Windows application with two tabs:

1. **Text to Speech** accepts text and uses an installed Windows voice to speak it.
2. **Speech to Text** listens only after explicit user action and appends finalized
   recognition results to a readable transcript.

The initial implementation uses local Windows speech engines through
`System.Speech`. It requires no API key, subscription, or cloud service.

## Target users

- A student demonstrating Windows Forms, events, OOP, and resource handling.
- A desktop user who wants written content read aloud.
- A desktop user who wants a short live dictation transcribed and exported.

## Value proposition

- One uncluttered place for both speech directions.
- Local-first behavior with no account or paid service.
- Clear active-state feedback so microphone and audio activity are never hidden.
- Code organized so responsibilities and OOP concepts are easy to explain.

## Goals

- Satisfy every mandatory professor requirement reliably.
- Provide a modern, keyboard-friendly Windows Forms experience.
- Handle unavailable hardware, permissions, languages, and engines gracefully.
- Demonstrate encapsulation, abstraction, interfaces, events, exceptions, and
  deterministic disposal.
- Maintain a traceable Git and documentation history.

## Non-goals

- Cloud transcription, translation, speaker identification, or voice cloning.
- Audio-file transcription or audio editing in the first release.
- Cross-platform support.
- Permanent transcript storage or a database.
- Claiming human-level transcription accuracy.

## Success measures

- All mandatory acceptance criteria in `REQUIREMENTS.md` pass on a Windows test
  machine with compatible installed engines.
- The user can always tell whether the application is idle, speaking, paused,
  listening, stopping, or in error.
- All owned disposable resources are released when stopped or when the form closes.
- Automated build and core tests pass from a clean checkout.
- The final README contains accurate setup instructions and at least two authentic
  screenshots of the completed application.

## Constraints and assumptions

- Deadline: November 30, 2026 at 23:59 (Asia/Riyadh).
- Target runtime: .NET 10 on Windows 10/11.
- Speech availability is determined by the current Windows installation.
- CI agents generally cannot validate real speakers or microphones.
- Student name and number must be added before submission.
