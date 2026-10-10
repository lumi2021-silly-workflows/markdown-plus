# MD+

A new way of enchanting your github overview

<!-- Unknown node ThematicBreakNode -->

MD+ (Markdown plus) is a github tool that provides extra resources for enchanting
someone's github overview profile.

Those resources includes:

- Third-party apps and services, such as shields.io badges
- Custom apps and widgets provided by MD+
- Dynamic profile components
- Automated content generation and updates
- Scheduled daily or weekly updates
- Integrations with external services (steam, wakatime, etc.)
- Customizable resources that can be embedded directly into your

GitHub profile README or any markdown file

This tool works by compiling a template written in an extended version of the
github's markdown + html language into a verbose and automated version of that
same template.

## Services

### Badges

Integration with [shields.io](https://shields.io/) made straight forward.

The `badge` tag is an inline tag that can be used to create static shields.io
badges inside the code:

```html
<badge color="202020" icon="python">python</badge>
<badge color="303030" style="flat" icon="c">C</badge> \
<badge color="404040" style="flat-square" icon="c++">C++</badge>
<badge color="505050" style="plastic" icon="dotnet">C#</badge> \
<badge color="606060" style="for-the-badge" icon="zig">Zig</badge> \
<badge color="707070" style="social" icon="lua">Lua</badge>
<br/>
<badge color="282b30" style="for-the-badge" icon="discord" href="https://discordapp.com/users/632992487375634432">Discord</badge>
<badge color="ffffff" style="for-the-badge" icon="linkedin" labelColor="0077B5" href="https://www.linkedin.com/in/leoaraujodev">Linkedin</badge>
<badge color="000000" style="for-the-badge" icon="threads" href="https://www.threads.com/@42batata42">Threads</badge>
```

![python](https://img.shields.io/badge/python-202020?logo=python)![C](https://img.shields.io/badge/C-303030?logo=c&style=flat)![C++](https://img.shields.io/badge/C%2B%2B-404040?logo=c%2B%2B&style=flat-square)![C#](https://img.shields.io/badge/C%23-505050?logo=dotnet&style=plastic)![Zig](https://img.shields.io/badge/Zig-606060?logo=zig&style=for-the-badge)![Lua](https://img.shields.io/badge/Lua-202020?logo=lua&style=social)

<br />

[![Discord](https://img.shields.io/badge/Discord-282b30?logo=discord&style=for-the-badge)](https://discordapp.com/users/632992487375634432)[![Linkedin](https://img.shields.io/badge/Linkedin-ffffff?logo=linkedin&style=for-the-badge)](https://www.linkedin.com/in/leoaraujodev)[![Threads](https://img.shields.io/badge/Threads-000000?logo=threads&style=for-the-badge)](https://www.threads.com/@42batata42)

The following attributes can be applied to a `badge` tag:

- `icon`: The icon shown in the tag. Icons provided by [Simple Icons](https://simpleicons.org/).
- `href`: Link to redirect when clicking in the badge.
- `style`: Badge style. Options are  [`flat`, `flat-square`, `plastic`, `for-the-badge`, `social`] (default is `flat`).
- `color`: The tag's background color.
- `icon-color`: The tag's icon color.
- `label-color`: The tag's label color.

### Typing

Integration with [readme-typing-svg](https://readme-typing-svg.herokuapp.com/demo/) made straight forward.

The `typing` tag is a block tag that can be used to create typing animations.

```html
<typing
      font="Rock Salt" size="20" duration="3000" pause="300"
      width="500" height="60" repeat="off"
>
Look at me!
I'm typing!
</typing>
```

<picture>
<source media="(prefers-color-scheme: dark)" srcset="https://readme-typing-svg.herokuapp.com?width=500&height=60&center=true&vCenter=true&multiline=true&repeat=false&lines=Look+at+me!%3BI%27m+typing!&color=cfcfcf" />
<source media="(prefers-color-scheme: light)" srcset="https://readme-typing-svg.herokuapp.com?width=500&height=60&center=true&vCenter=true&multiline=true&repeat=false&lines=Look+at+me!%3BI%27m+typing!&color=000000" />
<img draggable="false" width="100%" />
</picture>

The `typing` tag only supports raw text content.

The following attributes can be applied to the `typing` tag:

- `font-family`: Rendered text's font family. Fonts provided by [Google Fonts](https://fonts.google.com/).
- `font-size`: Rendered text's font size.
- `font-weight`: Rendered text's font weight.
- `letter-spacing`: Rendered text's letter spacing.
- `char-duration`: Wait time between each character typing.
- `line-duration`: Wait time between each line typing.
- `width`: Rendered image's width, in pixels. Default is 400.
- `height`: Rendered image's height, in pixels. Default is 100.
- `repeat`: If `on`, the animation repeats. Default is `on`.

### Wakatime

Integrations with the [Wakatime](https://wakatime.com) service.

The `wakatime` block tag provides different information that can be
chosen though the `option` attribute.

For allowing this service to work, the tool needs the following
variables:

- `wakatime_api_key`: The API key provided by the wakatime service.

Some options are:

- `weekly-langs`: Time spent in each language this week.

```html
<wakatime-weekly-langs />
<wakatime-weekly-langs max="10" />
<wakatime-weekly-langs style="display: code;" levels="⣿⣷⣶⣦⣤⣄⣀" />
```

<p align="center">
<img src="actions/cache/wakatime_weekly-langs-5_000000006acb7302.svg" />
</p>

<p align="center">
<img src="actions/cache/wakatime_weekly-langs-10_000000006acb7303.svg" />
</p>

```rust
Total Time: 12 hrs 6 mins

- "C#"            ⣿⣿⣿⣿⣿⣿⣶⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀ 2 hrs 41 mins
- "Swift"         ⣿⣿⣿⣿⣿⣿⣶⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀ 2 hrs 40 mins
- "Go"            ⣿⣿⣿⣿⣿⣿⣤⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀ 2 hrs 35 mins
- "AsciiDoc"      ⣿⣿⣿⣿⣿⣷⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀ 2 hrs 22 mins
- "TypeScript"    ⣿⣷⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀ 44 mins
```

### Last.fm

Integrations with the [last.fm](https://www.last.fm/home) service.

The `last-fm` block tag provides the list of your mostly listened
songs, as tracked by last.fm.

For allowing this service to work, the tool needs the following
variables:

- `lastfm_api_key`: The API key provided by the last.fm service.
- `lastfm_username`: The username of the targeted last.fm profile.

```html
<last-fm-recent />
```

<p>
<div style="clear: both; padding: 10px 0;">
<img src="https://is1-ssl.mzstatic.com/image/thumb/Music211/v4/a0/93/33/a0933384-61e2-ec73-796f-2c77fbd59ea0/artwork.jpg/60x60bb.jpg" width="60" align="left" />
<p>
<strong>
<a href="https://www.last.fm/music/Jamie+Paige/_/Machine+Love">Machine Love</a>
</strong>
 • 
<a href="https://www.last.fm/music/Jamie+Paige">Jamie Paige</a>
</p>
<strong clear="left">3:36</strong>
</div>
<div style="clear: both; padding: 10px 0;">
<img src="https://is1-ssl.mzstatic.com/image/thumb/Features115/v4/93/38/44/9338444f-8003-2c1c-4f61-c72c62c9850c/dj.uprjxxxf.jpg/60x60bb.jpg" width="60" align="left" />
<p>
<strong>
<a href="https://www.last.fm/music/we+are+the+dirt/_/counting+the+days">counting the days</a>
</strong>
 • 
<a href="https://www.last.fm/music/we+are+the+dirt">we are the dirt</a>
</p>
<strong clear="left">3:22</strong>
</div>
<div style="clear: both; padding: 10px 0;">
<img src="https://is1-ssl.mzstatic.com/image/thumb/Music211/v4/25/9c/a5/259ca5e1-c365-8b72-b12e-660aae6ff21d/25UMGIM87679.rgb.jpg/60x60bb.jpg" width="60" align="left" />
<p>
<strong>
<a href="https://www.last.fm/music/elio+mei/_/One+Man+Circus">One Man Circus</a>
</strong>
 • 
<a href="https://www.last.fm/music/elio+mei">elio mei</a>
</p>
<strong clear="left">5:49</strong>
</div>
<div style="clear: both; padding: 10px 0;">
<img src="https://is1-ssl.mzstatic.com/image/thumb/Music211/v4/25/9c/a5/259ca5e1-c365-8b72-b12e-660aae6ff21d/25UMGIM87679.rgb.jpg/60x60bb.jpg" width="60" align="left" />
<p>
<strong>
<a href="https://www.last.fm/music/Elio+Mei/_/Playing+Dead">Playing Dead</a>
</strong>
 • 
<a href="https://www.last.fm/music/Elio+Mei">Elio Mei</a>
</p>
<strong clear="left">4:47</strong>
</div>
<div style="clear: both; padding: 10px 0;">
<img src="https://is1-ssl.mzstatic.com/image/thumb/Music211/v4/25/9c/a5/259ca5e1-c365-8b72-b12e-660aae6ff21d/25UMGIM87679.rgb.jpg/60x60bb.jpg" width="60" align="left" />
<p>
<strong>
<a href="https://www.last.fm/music/Elio+Mei/_/Velcro">Velcro</a>
</strong>
 • 
<a href="https://www.last.fm/music/Elio+Mei">Elio Mei</a>
</p>
<strong clear="left">1:53</strong>
</div>
</p>

### Github

Integrations with the [Github](https://www.github.com) services.

#### Profile

The `github-profile` block tag provides data about someone's profile.

For allowing this service to work, the tool needs the following
variables:

- `github_api_token`: A token from github's API. No private access needed.
- `github_username`: The username of the targeted github user.

```html
<github-profile width="49.5%" />
<github-profile user="Anthragon" width="49.5%" />
```

<p align="center">
<img src="actions/cache/github_github-profile-lumi2021_000000006acb7578.svg" width="49.5%" />
</p>

<p align="center">
<img src="actions/cache/github_github-profile-Anthragon_000000006acb7cd4.svg" width="49.5%" />
</p>

#### Repositories

The `github-repo` block tag provides data about someone's activity, history and contributions.

For allowing this service to work, the tool needs the following
variables:[README.md](README.md)

- `github_api_token`: A token from github's API. No private access needed.[README.md](README.md)
- `github_username`: The username of the targeted github user.

```html
<github-repo path="lumi2021-silly-workflows/markdown-plus" width="49.5%" />
<github-repo path="Anthragon/Distribution" width="49.5%" />
```

<a href="https://github.com/lumi2021-silly-workflows/markdown-plus">
<p align="center">
<img src="actions/cache/github_github-repository-lumi2021-silly-workflows-markdown-plus_000000006acb79f6.svg" alt="lumi2021-silly-workflows/markdown-plus - " width="49%" />
</p>
</a>

<a href="https://github.com/Anthragon/Distribution">
<p align="center">
<img src="actions/cache/github_github-repository-Anthragon-Distribution_000000006acb7cd5.svg" alt="Anthragon/Distribution - Operating system built in zig" width="49%" />
</p>
</a>

#### Contributions

The `github-activity` block tag provides data about someone's activity, history and contributions.

For allowing this service to work, the tool needs the following
variables:[README.md](README.md)

- `github_api_token`: A token from github's API. No private access needed.[README.md](README.md)
- `github_username`: The username of the targeted github user.

```html
<github-activity />
```

- ✏️ Made 2 commits
- ✏️ Made 8 commits
- ✏️ Made 18 commits
- ✏️ Made 3 commits
- ✏️ Made 2 commits
- ✏️ Made 1 commit
- ✏️ Made 46 commits
- ✏️ Made 3 commits
- ✏️ Made 6 commits
- ✏️ Made 4 commits

### Steam

Integrations with the [Steam](https://store.steampowered.com/) services.

#### Recent

The `steam-lib-recent` block tag provides a list of the user's recent played games.

For allowing this service to work, the tool needs the following
variables:

- `steam_api_key`: A steam API key.
- `steam_user_id`: The user id of the targeted steam account.

```html
<steam-lib-recent />
```

<p>
<a href="https://store.steampowered.com/app/1353300" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="actions/cache/steam_gamecard-1353300-000001c7-thin_000000006ad62ba9.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="actions/cache/steam_gamecard-1353300-000001c7-wide_000000006ad62ba9.svg" />
<img style="max-width: 100%;" alt="Idle Slayer – Incremental RPG" />
</picture>
</a>
<a href="https://store.steampowered.com/app/1454400" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="actions/cache/steam_gamecard-1454400-0000022e-thin_000000006ad62ba9.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="actions/cache/steam_gamecard-1454400-0000022e-wide_000000006ad62ba9.svg" />
<img style="max-width: 100%;" alt="Cookie Clicker" />
</picture>
</a>
<a href="https://store.steampowered.com/app/1919460" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="actions/cache/steam_gamecard-1919460-00000001-thin_000000006ad62ba9.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="actions/cache/steam_gamecard-1919460-00000001-wide_000000006ad62ba9.svg" />
<img style="max-width: 100%;" alt="Seraph's Last Stand" />
</picture>
</a>
<a href="https://store.steampowered.com/app/433340" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="actions/cache/steam_gamecard-433340-00000039-thin_000000006ad62ba9.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="actions/cache/steam_gamecard-433340-00000039-wide_000000006ad62ba9.svg" />
<img style="max-width: 100%;" alt="Slime Rancher" />
</picture>
</a>
</p>
<p align="center">
<sub>
<i>Disclaimer: All game titles, arts, logos, and trademarks belong to Steam (Valve Corporation) and their respective developers.</i>
</sub>
</p>

#### Perfected

The `steam-lib-perfected` block tag provides a list of the user's perfected (100% achievements) games.

For allowing this service to work, the tool needs the following
variables:

- `steam_api_key`: A steam API key.
- `steam_user_id`: The user id of the targeted steam account.

```html
<steam-lib-perfected />
```

<p>
<a href="https://store.steampowered.com/app/1289310" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="actions/cache/steam_gamecard-1289310-0000000a-thin_000000006ad62ba9.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="actions/cache/steam_gamecard-1289310-0000000a-wide_000000006ad62ba9.svg" />
<img style="max-width: 100%;" alt="Helltaker" />
</picture>
</a>
<a href="https://store.steampowered.com/app/433340" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="actions/cache/steam_gamecard-433340-00000039-thin_000000006ad62ba9.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="actions/cache/steam_gamecard-433340-00000039-wide_000000006ad62ba9.svg" />
<img style="max-width: 100%;" alt="Slime Rancher" />
</picture>
</a>
<a href="https://store.steampowered.com/app/255520" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="actions/cache/steam_gamecard-255520-00000002-thin_000000006ad62ba9.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="actions/cache/steam_gamecard-255520-00000002-wide_000000006ad62ba9.svg" />
<img style="max-width: 100%;" alt="Viscera Cleanup Detail: Shadow Warrior" />
</picture>
</a>
<a href="https://store.steampowered.com/app/1997680" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="actions/cache/steam_gamecard-1997680-0000003a-thin_000000006ad62ba9.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="actions/cache/steam_gamecard-1997680-0000003a-wide_000000006ad62ba9.svg" />
<img style="max-width: 100%;" alt="REFLEXIA Prototype ver." />
</picture>
</a>
</p>
<p align="center">
<sub>
<i>Disclaimer: All game titles, arts, logos, and trademarks belong to Steam (Valve Corporation) and their respective developers.</i>
</sub>
</p>
