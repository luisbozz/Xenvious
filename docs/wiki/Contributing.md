# Contributing

## Before you start

- Read [Game memory](Game-Memory.md). Most bugs here come from a wrong offset
  or a write at the wrong time, not from C#.
- Open an issue for anything bigger than a fix, so the approach can be agreed
  first.

## Commits

[Conventional Commits](https://www.conventionalcommits.org/) with a scope when
it helps:

```text
feat(copyjob): copy only, copy + save, copy + publish
fix(offsets): dynamic prop count is Global_4980736.f_51387
refactor(ui): split MainWindow into partial files per page
```

- Subject in English, imperative or descriptive, under ~72 characters.
- The subject says what changes for the user or the code, not how you
  found it.
- One concern per commit. Offsets changes go in their own commit.

## Code style

- C# 9 on .NET Framework 4.8. Four-space indentation, braces on their own
  lines, `PascalCase` for types and members, `camelCase` for locals.
- Comments explain **why**: what the game does, what was measured, what goes
  wrong otherwise. The class comments in `CreatorMap`, `PreciseTemplates`,
  `GameVariant` and `AobCache` are good examples.
- Name things after what they do (`GameConnection`, not `Stuff`).
- Delete dead code instead of commenting it out; git keeps the history.
- New logic without UI goes into `Creator Classes/`; new pages into a
  `MainWindow/MainWindow.<Page>.cs` partial file.
- Every new UI text needs a key in all six languages, see
  [UI and translations](UI-and-Translations.md).

## Testing

There is no automated test suite for the app. Test in the game:

0. Restart GTA before testing, so bytecode injected by an earlier run is gone,
   and close Xenvious before rebuilding (the running exe is locked).
1. The creator(s) your change affects.
2. The edition(s) your change affects (Legacy, Enhanced, or both).
3. For anything that writes job data: save the job, reload it, and check the
   change survived.

Write in the pull request which creator and which edition you tested, and on
which game build.

## Scope

Xenvious is a tool for building and testing creator jobs. Changes that aim at
GTA Online gameplay, anti-cheat or other players are out of scope and will not
be merged.

## Pull requests

- Target the default branch, keep PRs focused.
- Describe what a user sees before and after.
- Include screenshots for UI changes.
- Say what you tested (see above) and what you could not test.
