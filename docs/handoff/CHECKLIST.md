# Checklist for every change

Go through this list for every new page, card, dialog or feature. Tick what applies in the PR text.
Extend the list when something new comes up.

## Look and themes
- [ ] No fixed colour values (`#RRGGBB`) in XAML or code. Use the theme tokens
      (`{DynamicResource TextColor}`, `MutedTextBrush`, `FaintTextBrush`, `SectionBackgroundBrush`,
      `SeactionHeaderBackgroundBrush`, `DeepBrush`, `LineBrush`, `PrimaryButtonBackground/Foreground`,
      `HighlightBrush/Foreground`, `SelectedTileBrush`, `OkBrush`, `WarnBrush`, `BadBrush` …); in code
      `MainWindow.ThemeBrush("Token")`. All tokens: `Helper Classes/Themes.cs`.
- [ ] A new token gets a value in **every** theme in `Themes.cs` and a dark default in `App.xaml`.
- [ ] Looked at in Dark **and** one light theme (Light or Catppuccin Latte).
- [ ] Popups and dialogs too: the in-app dialog (`MainWindow.Dialog.cs`), overlays (catalog, screen
      message), tool tips, the open dropdown list, the context menu.
- [ ] Cards: `DashCard` / `DashCardHeader` / `DashCardTitle`, labels above fields (`FieldLabel`),
      X/Y/Z row with `FieldIconButton`, switches with `FormRow` + `FormToggle`.
- [ ] Buttons that open another page get `GoToPageIcon`.

## Texts
- [ ] Every new text in all six dictionaries (`Translation.cs`: ger, eng, ru, pl, fr, zh_cn), no key twice.
- [ ] XAML binds with `{Binding Translation[key], FallbackValue=...}`.

## Game data
- [ ] Offsets only from ysc-global-updater, in `legacy/` **and** `enhanced/offsets.ini`.
- [ ] Reads and writes check `m.IsProcOpen`; missing offsets (0) are skipped.
- [ ] Per team / per rule values use the team and rule selected on the page.
- [ ] Deleting or moving entities: extra objectives and other index lists follow (`ExtraObjectives.OnEntityDeleted`).
- [ ] After bulk writes the creator is refreshed or rebuilt.

## Pages and navigation
- [ ] New page is in the side list (`MainWindow.EditNav.cs`) with the right creators.
- [ ] Used in the job? Give the sub-page a `Count` so the side list shows the green counter.
- [ ] The page selects its first entry when it opens (entry bar with `Tag="EntryBar"`).
- [ ] Help: `?` button with an explanation where the feature is not obvious (rules, extra objectives).

## Testing
- [ ] Built locally (`~/bin/xrebuild`).
- [ ] Tested in game: which creator, which edition (Enhanced / Legacy), which build.
- [ ] Script patches and offsets: tested on **both** editions.
