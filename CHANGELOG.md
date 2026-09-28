# Changelog

## [1.73.1](https://github.com/luisbozz/Xenvious/compare/v1.73.0...v1.73.1) (2026-09-28)


### Features

* **catalog:** model catalog with pictures, favourites and recently used models ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **copyjobs:** job map, creator info and fit checks ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **creator:** Public Mission Creator support with six script patches ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **dashboard:** redesigned dashboard with creator-aware status and counts ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **map:** HD satellite map ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **mission:** hidden LTS and Capture options, creator value fixes ([#16](https://github.com/luisbozz/Xenvious/issues/16)) ([4608fe8](https://github.com/luisbozz/Xenvious/commit/4608fe8ca304b016eb4bea6b154e7ced911d8054))
* **moddedprops:** redesigned modded props page, remember changes for all creators ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **patches:** game and script patches on one page, creator camera without collision, ignore budget ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **playarea:** one page for play areas 1 and 2 with per-rule mode, timer and wanted level ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **rules:** rules page with jumps, flow, objective texts, drop-off zones and entity links ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **ui:** redesigned pages, themes, grouped navigation with icons, search, reorderable favourites ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **vehicles:** vehicles page with map, rules card, lifecycle and team locks ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))


### Bug Fixes

* **mission:** police dropdown, team and round count ranges, one team in LTS ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **moddedprops:** no writes into wrong slots of other creators ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))
* **props:** moving a prop to dynamic no longer bricks the creator ([5fa87db](https://github.com/luisbozz/Xenvious/commit/5fa87db4f9c07ca3e371862a187a7c21fd28a08c))
* **props:** moving a prop to dynamic no longer bricks the creator ([b497d37](https://github.com/luisbozz/Xenvious/commit/b497d374658b656d304c3ec2c23a016522f11513))
* **ui:** mouse wheel scrolls lists and text boxes, page scroll no longer changes dropdowns ([08949e4](https://github.com/luisbozz/Xenvious/commit/08949e49c84a17132c84ba20d2c6cc2eac4fd0d7))

## [1.73.0](https://github.com/luisbozz/Xenvious/releases/tag/v1.73.0) (2026-09-25)

First public release of Xenvious. What's new since 2.71.11:

### Highlights

- **GTA V Enhanced** support next to Legacy, updated for GTA 1.73
- **Free, no server:** no login, no accounts, every feature unlocked, all game data built in
- **Self-updating:** Xenvious finds new releases on GitHub, updates itself and shows what's new
- **One single exe**, nothing else to install

### Creator

- **Copy Jobs** has its own page: paste a link or job ID, load the complete job (vehicles, actors and zones included) or only selected parts, then just copy, save or publish. Saving creates your own new job. Loading a complete job can skip the backup of the open map.
- **Advanced prop placement** rebuilt: quick-start wizard for straights, curves, loops, spirals, corkscrews and wallrides, a free-flying 3D preview, and the full 300-prop limit on Enhanced
- **Precise templates** on both editions
- **Map backup:** save props, dynamic props, checkpoints and templates to a file and load them back
- Move props between the static and the dynamic list
- Map rebuilds keep the creator's menu and camera
- **Dashboard:** launch or leave a creator with one click, even a stuck one, plus a live status strip
- **Script patches page:** every patch as a card with a description, filterable by script
- 47 new vehicles from the 1.73 race creator, prop lists refreshed for 1.73

### Under the hood

- Faster start and smoother UI (cached pattern scans, GPU rendering)
- Dialogs inside Xenvious instead of pop-ups that pull you out of the game
