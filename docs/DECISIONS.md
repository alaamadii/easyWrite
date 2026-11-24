# Architecture Decision Log

Decisions are append-only. If a decision changes, add a new entry that supersedes
the previous one rather than silently rewriting history.

## ADR-001 — Use .NET 10 Windows Forms

- **Status:** Accepted
- **Date:** 2026-11-24

**Context:** The professor requires a C#/.NET Windows Forms desktop application.
The development machine has .NET SDK 10.0.402 and the matching Windows Desktop
runtime.

**Decision:** Target `net10.0-windows` with Windows Forms and pin the SDK feature
band through `global.json` while allowing later 10.0 patches.

**Consequences:** The project is Windows-only. A Windows GitHub Actions runner is
required. The app can use native Windows desktop controls and audio capabilities.

## ADR-002 — Use System.Speech as the initial speech provider

- **Status:** Accepted with validation gate
- **Date:** 2026-11-24

**Context:** The application needs synthesis and microphone recognition. The
`System.Speech` 11.0.0 package supports .NET 10 and provides Windows Speech API
recognition and synthesis without cloud keys or subscriptions. It is a
compatibility-focused, Windows-only package and depends on installed engines.

**Decision:** Isolate `System.Speech` in `EasyWrite.Infrastructure`. Before claiming
support for a language, enumerate and manually test the installed local engine.

**Consequences:** The app can remain local-first, but availability and quality vary
by machine. The UI needs strong unavailable-capability states. A future provider
can implement the Core contracts only after an explicit decision replaces this one.

## ADR-003 — Separate UI, core contracts, and infrastructure

- **Status:** Accepted
- **Date:** 2026-11-24

**Context:** Directly placing speech engines, file I/O, and all state inside a form
would be difficult to test, dispose, and explain.

**Decision:** Use App, Core, Infrastructure, and Core.Tests projects with inward
dependencies documented in `ARCHITECTURE.md`.

**Consequences:** More project files exist, but the responsibilities and OOP
evidence are explicit. Core remains testable without Windows Forms or speech APIs.

## ADR-004 — One visible application operation state

- **Status:** Accepted
- **Date:** 2026-11-24

**Context:** Running synthesis and microphone recognition together would create
feedback, confusing controls, and difficult resource ownership.

**Decision:** Model one state vocabulary: Idle, Speaking, Paused, Listening,
Stopping, or Error. Reject or safely resolve conflicting commands.

**Consequences:** The UI can be derived from one authoritative state. Simultaneous
TTS/STT is intentionally unsupported.

## ADR-005 — Keep hardware validation manual and explicit

- **Status:** Accepted
- **Date:** 2026-11-24

**Context:** Hosted CI cannot reliably provide speakers, microphones, permissions,
or installed language engines.

**Decision:** CI validates restore, formatting, Release build, and deterministic
tests. A documented physical-Windows matrix provides audio evidence.

**Consequences:** A green CI run is necessary but not sufficient for completion.
Final submission needs recorded manual evidence and authentic screenshots.

## ADR-006 — No cloud, persistence, or DI framework initially

- **Status:** Accepted
- **Date:** 2026-11-24

**Context:** These additions increase setup, privacy, and explanation cost without
supporting a mandatory requirement.

**Decision:** Compose services manually, retain transcripts in memory, export only
on user request, and make no network calls.

**Consequences:** The first version stays understandable and has no API secrets.
Revisit only when a concrete requirement justifies the complexity.
