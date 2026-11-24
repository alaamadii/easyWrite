## Summary

<!-- What changed and why? Keep this factual. -->

## Requirements / stories

<!-- Example: FR-03, NFR-05, US-02 -->

## Validation

- [ ] `dotnet restore EasyWrite.slnx --locked-mode`
- [ ] `dotnet format EasyWrite.slnx --verify-no-changes --no-restore`
- [ ] `dotnet build EasyWrite.slnx --configuration Release --no-restore`
- [ ] `dotnet test EasyWrite.slnx --configuration Release --no-build`

Actual results:

```text
Paste concise command results here.
```

## Manual checks

<!-- List actual results. Write "Not run" with a reason when appropriate. -->

- [ ] UI / keyboard / DPI (if affected)
- [ ] Text-to-speech on real audio hardware (if affected)
- [ ] Speech-to-text on a real microphone (if affected)
- [ ] Resource cleanup / form closing (if affected)

## Evidence

<!-- Add authentic screenshots for visible UI changes; never fabricate evidence. -->

## Risks, limitations, and follow-up

<!-- What remains, and what should reviewers pay particular attention to? -->

## Checklist

- [ ] Scope is focused and contains no unrelated/generated files.
- [ ] Tests and documentation match the behavior changed.
- [ ] No secrets, personal transcript content, or machine-specific paths are added.
- [ ] No feature is marked complete without implementation and evidence.
