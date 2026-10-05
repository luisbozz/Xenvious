# Releases and updates

Releases are made by [release-please](https://github.com/googleapis/release-please)
from the Conventional Commits on `main`. Nobody edits the version or the
changelog by hand.

## How a release happens

1. Commits land on `main` (`feat(...)`, `fix(...)`, ...). release-please keeps a
   pull request "chore(main): release X.Y.Z" open that collects them, updates
   `CHANGELOG.md` and the version in `Xenvious/Properties/AssemblyInfo.cs`
   (the lines marked `// x-release-please-version`).
2. The version is `1.<GTA>.<n>`: the middle number follows the GTA V update
   (73 for GTA 1.73), the last one counts Xenvious releases for it. So every
   release, `feat` or `fix`, only raises the last number
   (`"versioning": "always-bump-patch"` in `release-please-config.json`).
   `docs`, `refactor`, `chore` and the like do not appear in the changelog, so
   the subject line of a `feat` or `fix` commit is what users read.
3. Merging that pull request tags `vX.Y.Z` and creates the GitHub release with
   the same notes. The second job of `.github/workflows/release.yml` builds
   that tag and attaches `Xenvious.exe` and `Xenvious.exe.sha256`.
4. Xenvious finds the release on its next start (`Helper Classes/Updater.cs`),
   shows the notes, downloads the exe, checks it against the SHA256 and swaps
   itself. The embedded `CHANGELOG.md` provides "New in X.Y.Z" afterwards.

## Nightly builds

`.github/workflows/nightly.yml` builds `main` every night at 01:00 UTC and on
demand (Actions → Nightly → "Run workflow", or `gh workflow run nightly.yml`).
The scheduled run skips the build when `main` has not moved since the last
nightly.

- Each run replaces one pre-release with the tag `nightly`. Its title carries
  the version, its notes the `feat`/`fix` subjects since the last release tag.
  It has the same `Xenvious.exe`, `Xenvious.exe.sha256` and build provenance
  as a release.
- The version is the release version plus the run number as a fourth part,
  `1.73.3.12`. That is newer than release 1.73.3 and older than the next
  release 1.73.4, so a nightly player moves on to that release by itself. The
  fourth part exists only in the built exe; `main`, the changelog and
  release-please do not change.
- Players only get it with Settings → Updates → "Get nightly builds" on.
  Pre-releases never count as `releases/latest`, so everyone else keeps
  getting releases only. Turning the option off on a nightly build offers to
  go back to the latest release.

## The first release

The manifest (`.release-please-manifest.json`) starts at 2.71.11, the last
version before the public repository. To make the first public release
1.73.0, the commit that starts it carries this footer:

```
Release-As: 1.73.0
```

Xenvious only offers a release whose version is higher than its own
(`Helper Classes/Updater.cs`), so versions must only go up. Exes numbered 2.x
or 3.73.0 from before this scheme never offer 1.73.x; replace them by hand
once.

## Repository setting

release-please opens pull requests with the workflow token, which GitHub
refuses unless Settings → Actions → General → "Allow GitHub Actions to create
and approve pull requests" is on.

## After a GTA patch

The first Xenvious release for a new GTA update raises the middle number. Put
the version in the footer of the commit that adds the new offsets, for example
for GTA 1.74:

```
Release-As: 1.74.0
```

1. `python3 update_xenvious.py --variant both --new <build>` in
   ysc-global-updater refreshes `OfflineData`.
2. Build and test in the game.
3. Push `fix(offsets): support GTA build <build>`.
4. Merge the release pull request. Users get the update on their next start.
