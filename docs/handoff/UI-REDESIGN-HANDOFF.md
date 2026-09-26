# Handoff: UI redesign (branch `feature/ui-redesign`)

Status as of 2026-09-26. For a new Claude session: read this file, `AGENTS.md` and
`docs/wiki/` first. The user writes in German; answer in German.

## Working rules (from the user)

- Every change goes to `feature/ui-redesign`, never `claude/...` branches.
- The user tests in game on Windows and builds with xbuild in WSL.
  GitHub Actions `build.yml` must stay green. It only proves that the code compiles.
  A missing StaticResource in XAML only shows up at startup, so ask for the log after a crash.
- The user likes to see mockups first (HTML artifacts) and then picks a variant.
- Be sparing with tokens.
- Conventional Commits with a scope. Every new UI text goes into all 6
  dictionaries in `Translation.cs`. Insert new keys after the `"dash_team"` line
  in each dictionary (order ger, eng, ru, pl, fr, zh_cn).

## What is done (all pushed, CI green up to 42f5565 / 061c3bf)

- **Dashboard.**
  - Layout B with two columns, a compact job card and the Social Club button.
  - The job ID is a link and teams are toggle buttons.
  - Ambient section with "Mission" and "Race lobby" subsections, shown per job type.
  - Description: byte counter (8 × 63 = 504 bytes) and the write bug fixed (`setDescribtionNew`).
- **Global styles.**
  - ComboBox: 14 px, list exactly as wide as the box, light hover.
  - Textboxes (Watermark) and checkboxes restyled.
  - FormRow 34 px and FormLabel 15 px.
- **Edit navigation** (`MainWindow.EditNav.cs`).
  - Side list with groups, search and collapse.
  - Script-features status in the bottom left with a popup.
  - Breadcrumb header; the old inner switchers are hidden (`HideInnerSwitchers`).
- **Misc.**
  - Side list with groups Jobs / Creator / Development. "Map Backup" is renamed "Job-Backup".
  - The SC-Jobs page is removed and nrcid moved to Copy Jobs.
  - The Menu Switcher page was redone.
- **Copy Jobs.**
  - Link or nrcid mode.
  - Job infos by job type, meta data (creator, plays, likes), map (`Controls/JobMap.cs`) and the "Does the job fit?" checks.
- **Model catalog.**
  - `Controls/ModelCatalog.xaml` is the overlay: categories, favourites, recently used, search.
  - `Controls/ModelCard.xaml` is variant C: a "Change model" button with a pencil, and a clickable picture.
  - Empty states "Nothing placed yet" and "No entry selected".
  - Picture cache: `Creator Classes/ModelImageCache.cs`, images from `cdn.rage.mp/public/odb/imgs/{native}-{uint}.jpg`.
  - Favourites and recently used: `ModelCatalogStore.cs` (config `[MODELCATALOG]`). The default favourites are the old premium props (UFO etc.).
- **Settings** in the dashboard card style, with a picture-cache block.
- **Props, Dynamic Props and Actor** in the card layout from the page concept.
  - Entry bar: count, `‹ number ›` (`EntryStep_Click` in `MainWindow.EntryBar.cs`), move/duplicate, delete, add.
  - Cards: Model | Placement | Look | Rules | Advanced (folded, raw values).
  - Every control kept its name and handler; the old model controls are hidden.
  - Generated with `docs/handoff/props_page.py` and `actor_page.py` (they extract elements by `x:Name`). They are useful as a template for further pages, but the pages are already converted, so do not run them again.
  - New global styles:
    - in `MainWindow.xaml` Window.Resources: FieldLabel, FieldAxis, FieldLabelRaw, CardSub, CardExpander, DashCard*
    - in `Controls/NavBar.xaml`: FormButtonPrimary, EntryStepButton, FieldIconButton, GoToPageIcon (arrow for buttons that jump to another page).
- **Map Mover calibration** measured in game and set as the default.

## Open todos

1. Rebuild the other edit pages in the card layout: vehicles, weapons, zones, doors, the Mission/Capture/Race/DM/Survival sub-pages, fixtures, modded, interior. Use the same pattern, styles and generator approach as for props.
2. Use FieldIconButton (crosshair, link) and GoToPageIcon (arrow for page jumps) on those pages too.
3. Optional: move the entry bar into the header row next to the title (as in the page concept).
4. Dashboard: overview map (the user wants it) and ambient sections for DM and Survival.
5. Actor list with model names and categories from the scripts (`fm_lts_creator` func_5751/func_952, 10 categories), so actors get pictures and categories in the catalog.
6. Copy Jobs: check for missing models.
7. Read the Race HANDOFF.md (the user has the file and will attach it again).
8. Later:
   - extend the converter and switch the bit handling
   - new offsets optbs / musmustr / WSBS via ysc-global-updater
9. Create a PR for `feature/ui-redesign` once the user asks (state what was tested in game).
10. Check the latest builds `5bc4df8`, `1550dc4` and `792289b` in Actions.

## Mockups (claude.ai artifacts, private to the old account)

They can only be opened if the old account shares them:

| Mockup | Link |
|---|---|
| Page concept | https://claude.ai/artifact/J3ZacvVcEhMZJGSztGznPX |
| Model card variants | https://claude.ai/artifact/RuWrE15dSttbyD1jxkUfYw |
| Edit navigation | https://claude.ai/artifact/TVNSB5bmYYKa5DCyq65G3h |
| Dashboard v3 | https://claude.ai/artifact/BmhFr7ebhDYeRcKq4M72Me |
| Copy Jobs | https://claude.ai/artifact/4n7tg1JWhpJquREjdaZSD4 |
| Misc | https://claude.ai/artifact/JNEtZzh8RFjeipuPe4h5zF |
