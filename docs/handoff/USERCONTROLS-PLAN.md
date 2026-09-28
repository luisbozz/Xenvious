# Plan: one UserControl per page

Goal: `MainWindow.xaml` (about 13,000 lines) becomes a shell; every page lives in
`Pages/<Page>.xaml` with its code in `Pages/<Page>.xaml.cs`. The XAML designer can open a page
again, merge conflicts get rare, and a page is one place to read.

## What is in the way

1. **Styles live in `Window.Resources`.** FormRow, FormLabel, FormToggle, DashCard*, FieldLabel,
   FieldAxis, FieldLabelRaw, CardSub, CardExpander are `BasedOn` the window's implicit TextBlock /
   CheckBox styles. A UserControl compiles its StaticResources before it sits in the window, so it
   cannot see them. They (and the implicit Label / Button / TextBox / TextBlock / ScrollViewer styles
   they build on) have to move together into `Controls/FormStyles.xaml`, merged in `App.xaml`.
   Moving only the keyed styles changes their look (their BasedOn would fall back to the theme).
2. **The worker reads and writes the controls directly.** `MainWindow.Worker.cs` refreshes 44 pages
   by control name. Each page needs a `Refresh()` that the worker calls instead.
3. **Shared helpers sit on MainWindow** (`IsValidInt`, `ChangeValuePlusMinus`, `TranslateOr`,
   `displayScreenMessage`, `creatorRefresh`, `m`). They move to a static `PageHelpers` class, or the
   page gets a reference to the window.
4. **Cross-page access.** Only a few places reach into another page (Edit navigation, dashboard counts,
   script status). Those go through small public methods on the page.

## Steps (each one builds and can be tested on its own)

1. Move all window styles (implicit and keyed) into `Controls/FormStyles.xaml`, merge it in
   `App.xaml`, delete them from `Window.Resources`. No page changes. Test: every page looks the same.
2. `PageHelpers` static class for the shared helpers; MainWindow forwards to it.
3. Pilot: **Teleport** (79 lines, no worker refresh). `Pages/TeleportPage.xaml`, code moved from
   `MainWindow.Teleport*.cs`. In MainWindow only `<pages:TeleportPage/>`.
4. **Zones** (small, card layout, own Refresh from the worker).
5. Then page by page: Doors, Weapons, Vehicles, Fixtures, Actor, Props, Dynamic, Capture, Race,
   Mission sub-pages (largest last), Dashboard, Settings, Misc.

Each step is one PR-sized commit; after each, the user clicks through the moved page in game.
