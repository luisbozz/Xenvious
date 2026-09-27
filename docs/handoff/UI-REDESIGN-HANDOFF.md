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

## Design decisions (binding, from the user; apply them without asking again)

- Dark Discord-like grey. Cards: DashCard + DashCardHeader + DashCardTitle (17 px bold); sub titles CardSub.
  Not everything bold: labels normal or semi-bold, only what matters is big.
- Edit pages follow the page concept: free entry bar without a box (left "N … placed", right an action like
  "→ Dynamic", then ‹ number › as one field with numbers only, trash icon button, light "+ Add" =
  FormButtonPrimary), all 32 px high. Below: 340 px card columns in a WrapPanel: Model | Placement (not
  "Position", next to the model) | Look | Rules | Advanced (folded CardExpander, raw values with FieldLabelRaw).
- Labels above fields (FieldLabel), no watermark in the field. X/Y/Z in one row with FieldAxis labels and a
  square FieldIconButton (crosshair = cursor position, chain = keep rotation). Switch rows: FormRow +
  FormLabel + FormToggle.
- Buttons that jump to another Xenvious page get GoToPageIcon; external links the "opens outside" icon.
- Global ComboBox style; model card variant C (change button with pencil, clickable picture, empty states).
- Breadcrumb never doubled. Options that do not fit the loaded job type: hidden, with a section heading per
  job type (like the dashboard ambient).
- Choices shown as picture buttons where that helps (zone shapes), long lists grouped by topic (zone types, variant B).
- No markers for "not offered by the creator": no " *", no "also in the public creator" notes.
- Every control keeps its x:Name and handler; old controls still used by code are hidden, not deleted.
- Work without asking for mockups when the pattern is known; mockups only as a list to review afterwards.
- Overview map on the dashboard: shelved. Copy Jobs "missing models" check: not wanted.

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

## Done on 2026-09-27 (not tested in game yet)

- Vehicles, zones, weapons and doors in the card layout (`docs/handoff/vehicle_page.py`,
  `zone_page.py`, `weapon_page.py`, `doors_page.py`; `.gitignore` excludes `*.py`, so add them with `git add -f`).
  Vehicles and weapons have a model card with catalog (no pictures: the only sources found serve WebP).
- Zones: zone type picker with names and help (`Creator Classes/ZoneTypes.cs`), taken from the zone
  type switch in `fm_mission_controller`. Rockstar's names for types above 7 are in no public label dump.
- Actor catalog: categories and model names from `fm_lts_creator` (`Creator Classes/ActorCategories.cs`).
- Entry number dropdown stays numbers only (user picked variant A).
- Copy Jobs "missing models" check: dropped, the user does not need it.

## Open todos

1. Card layout for the remaining edit pages: the Mission/Capture/Race/DM/Survival sub-pages, fixtures,
   modded, interior. Page concept has templates for Capture objects/general, Race checkpoints, Mission team settings.
2. Dashboard: ambient sections for DM and Survival (check in the scripts which options their
   controllers read). The overview map is shelved for now (user decision, mockup
   https://claude.ai/artifact/C6nbeVWAam5wJZwdApWxtr).
3. Zone type names above 7: need the DLC text labels (FMMC_ZN_TY*, MC_H_ZN_TY*), e.g. exported with OpenIV.
   The public Mission Creator (Enhanced `public_mission_creator.c`, func_3152) offers only
   0, 9, 12, 23, 37, 40 and 89, so the user can read those names in game.
4. Optional: entry bar into the header row; catalog pictures for vehicles/weapons/actors (WebP).
5. Later: Race handoff, converter and bit handling, new offsets optbs / musmustr / WSBS via ysc-global-updater.
6. PR for `feature/ui-redesign` once the user asks (state what was tested in game).

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
