# UI and translations

## Window and pages

`MainWindow.xaml` is one large window with all pages. Its code is split into
partial classes under `Xenvious/MainWindow/`, one file per page or concern:

```text
MainWindow.Dashboard.cs / DashboardPage.cs   dashboard, status strip, launch creator
MainWindow.Props.cs / DynamicProps.cs         prop lists
MainWindow.Race*.cs                           race settings, checkpoints, vehicles
MainWindow.Mission.*.cs                       mission (LTS/Capture) rules, teams, actors, zones, ...
MainWindow.Misc.*.cs                          copy jobs, map mover, menu switcher, script patches
MainWindow.Settings.cs                        settings and language
MainWindow.Navigation.cs                      page switching
MainWindow.Dialog.cs / ScreenMessage.cs       in-app dialog and toasts
```

When you add a page, add a new partial file named after it
(`MainWindow.<Page>.cs`) with a first-line comment saying what it covers,
like the existing files. Keep game logic out of it; put that in
`Creator Classes/`.

Reusable controls and styles are in `Controls/` (`ComboBox.xaml`,
`Checkbox.xaml`, `Slider.xaml`, `CustomTB`, `WeaponInventoryControl`, ...).
Use them instead of plain WPF controls so the look stays consistent.

WPF styling pitfalls seen here: an explicit `Style` replaces the implicit one,
so derive with `BasedOn` and define it after its base; a custom control
template needs its `ContentPresenter`; and do not steal focus from the game,
show an in-app dialog (`MainWindow.Dialog.cs`) instead.

## Translations

All UI text goes through `Translation/Translation.cs`. It holds one dictionary
per language:

| Field | Language |
|---|---|
| `_Language.ger` | German |
| `_Language.eng` | English |
| `_Language.ru` | Russian |
| `_Language.pl` | Polish |
| `_Language.fr` | French |
| `_Language.zh_cn` | Chinese (simplified) |

`MainWindow.Translation` is the active dictionary (`_Language.FromCode(...)`,
set in `MainWindow.Settings.cs`). A missing key shows "missing translation".

In XAML:

```xml
<TextBlock Text="{Binding Translation[jobimg], FallbackValue=Job Image}" />
```

In code: `Translation["key"]`. View models get a translate function
(`_t("key", "fallback")`).

**Rule:** a new key goes into **all six** dictionaries in the same commit. If
you cannot translate, use the English text in the other languages and say so
in the PR, so a native speaker can fix it.

## Tone of UI texts

Short, plain words that say what a control does. Status and error lines
describe what happened and what to do next, not internal names.
