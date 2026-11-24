# User Stories

## Text-to-speech user

### US-01 — Hear entered text

As a user, I want to enter or paste text and hear it spoken so that I can consume
written content through audio.

**Acceptance:** blank input is rejected clearly; valid input starts asynchronously;
the status becomes Speaking; completion returns the application to Idle.

### US-02 — Control playback

As a user, I want to pause, resume, and stop speech so that I control playback.

**Acceptance:** only valid controls are enabled; Pause changes to Resume while
paused; Stop cancels all queued prompts and returns to Idle.

### US-03 — Choose how speech sounds

As a user, I want to choose an installed voice, rate, and volume so that speech is
comfortable to hear.

**Acceptance:** only usable installed voices appear; values remain within supported
ranges; unavailable choices cause actionable feedback.

### US-04 — Clear and start again

As a user, I want one clear action that empties the editor so that I can begin a new
speech task quickly.

**Acceptance:** clearing does not leave hidden speech running; focus returns to the
text editor.

## Speech-to-text user

### US-05 — Dictate through the microphone

As a user, I want to explicitly start listening and see finalized speech appear as
text so that I can dictate a short transcript.

**Acceptance:** the microphone starts only after a click; Listening is prominent;
results append without replacing earlier results; Stop ends capture safely.

### US-06 — Understand recognition status

As a user, I want clear microphone and recognition feedback so that I know whether
the application is actually listening.

**Acceptance:** status is conveyed by text and shape, not color alone; errors name
the missing capability and suggest a recovery action.

### US-07 — Review when words were recognized

As a user, I want each result timestamped so that I can follow the order and timing
of my dictation.

**Acceptance:** timestamps use local time consistently and remain associated with
their text when exported.

### US-08 — Save a transcript

As a user, I want to export recognized text to a UTF-8 TXT file so that I can use it
outside EasyWrite.

**Acceptance:** I choose the destination; cancel is harmless; success reports the
path; access and I/O errors do not destroy the on-screen transcript.

## Safety and failure stories

### US-09 — Recover from unavailable capabilities

As a user without a compatible voice, recognizer, microphone, or permission, I want
a clear explanation so that I can correct Windows setup rather than believe the
application is working.

### US-10 — Exit safely

As a user, I want to close EasyWrite during speech or listening without a hang so
that the application always remains under my control.

## Developer and evaluator stories

### US-11 — Verify behavior

As an evaluator, I want documented requirements and repeatable checks so that I can
confirm what works and distinguish automated evidence from hardware testing.

### US-12 — Explain the design

As the student, I want small classes with named responsibilities so that I can
explain OOP concepts, event flow, exceptions, and resource ownership during review.
