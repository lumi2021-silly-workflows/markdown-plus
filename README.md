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

<badge color="202020" icon="python">python</badge>
<badge color="303030" style="flat" icon="c">C</badge> \
<badge color="404040" style="flat-square" icon="c++">C++</badge>
<badge color="505050" style="plastic" icon="dotnet">C#</badge> \
<badge color="606060" style="for-the-badge" icon="zig">Zig</badge> \
<badge color="202020" style="social" icon="lua">Lua</badge>

<br />

<badge color="282b30" style="for-the-badge" icon="discord" href="https://discordapp.com/users/632992487375634432">Discord</badge>
<badge color="ffffff" style="for-the-badge" icon="linkedin" labelColor="0077B5" href="https://www.linkedin.com/in/leoaraujodev">Linkedin</badge>
<badge color="000000" style="for-the-badge" icon="threads" href="https://www.threads.com/@42batata42">Threads</badge>

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

<typing font="Rock Salt " size="20" duration="3000" pause="300" width="500" height="60" repeat="off">Look at me!
I'm typing!</typing>

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
<wakatime-weekly-langs style="display: code;" level="⣿⣷⣶⣦⣤⣄⣀ " />
```

<img src="data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAwIiBoZWlnaHQ9IjE2MCIgdmlld0JveD0iMCAwIDYwMCAxNjAiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+PHN0eWxlPnRleHQgewogICAgY29sb3I6ICM3Nzc7Cn08L3N0eWxlPjx0ZXh0IHg9IjIwIiB5PSIzMCIgZmlsbD0iY3VycmVudENvbG9yIiBmb250LWZhbWlseT0ibW9ub3NwYWNlIiBmb250LXNpemU9IjE2Ij5Ub3RhbCB0aW1lOiA5IGhycyA0OSBtaW5zPC90ZXh0Pjx0ZXh0IHg9IjIwIiB5PSI1NSIgZmlsbD0iY3VycmVudENvbG9yIiBmb250LWZhbWlseT0ibW9ub3NwYWNlIiBmb250LXNpemU9IjE0Ij5DIzwvdGV4dD48cmVjdCB4PSIxNTAiIHk9IjQyIiB3aWR0aD0iMjgwIiBoZWlnaHQ9IjEyIiByeD0iMyIgZmlsbD0iIzMwMzYzZCIgLz48cmVjdCB4PSIxNTAiIHk9IjQyIiB3aWR0aD0iMTY4IiBoZWlnaHQ9IjEyIiByeD0iMyIgZmlsbD0iaHNsKDAsIDcwJSwgNTUlKSIgLz48dGV4dCB4PSI0NDAiIHk9IjU1IiBmaWxsPSJjdXJyZW50Q29sb3IiIGZvbnQtZmFtaWx5PSJtb25vc3BhY2UiIGZvbnQtc2l6ZT0iMTQiPjUgaHJzIDU0IG1pbnM8L3RleHQ+PHRleHQgeD0iMjAiIHk9Ijc1IiBmaWxsPSJjdXJyZW50Q29sb3IiIGZvbnQtZmFtaWx5PSJtb25vc3BhY2UiIGZvbnQtc2l6ZT0iMTQiPlR5cGVTY3JpcHQ8L3RleHQ+PHJlY3QgeD0iMTUwIiB5PSI2MiIgd2lkdGg9IjI4MCIgaGVpZ2h0PSIxMiIgcng9IjMiIGZpbGw9IiMzMDM2M2QiIC8+PHJlY3QgeD0iMTUwIiB5PSI2MiIgd2lkdGg9IjMzLjU0NDAwMDAwMDAwMDAwNCIgaGVpZ2h0PSIxMiIgcng9IjMiIGZpbGw9ImhzbCg3MiwgNzAlLCA1NSUpIiAvPjx0ZXh0IHg9IjQ0MCIgeT0iNzUiIGZpbGw9ImN1cnJlbnRDb2xvciIgZm9udC1mYW1pbHk9Im1vbm9zcGFjZSIgZm9udC1zaXplPSIxNCI+MSBociAxMCBtaW5zPC90ZXh0Pjx0ZXh0IHg9IjIwIiB5PSI5NSIgZmlsbD0iY3VycmVudENvbG9yIiBmb250LWZhbWlseT0ibW9ub3NwYWNlIiBmb250LXNpemU9IjE0Ij5NYXJrZG93bjwvdGV4dD48cmVjdCB4PSIxNTAiIHk9IjgyIiB3aWR0aD0iMjgwIiBoZWlnaHQ9IjEyIiByeD0iMyIgZmlsbD0iIzMwMzYzZCIgLz48cmVjdCB4PSIxNTAiIHk9IjgyIiB3aWR0aD0iMzIuNzMyIiBoZWlnaHQ9IjEyIiByeD0iMyIgZmlsbD0iaHNsKDE0NCwgNzAlLCA1NSUpIiAvPjx0ZXh0IHg9IjQ0MCIgeT0iOTUiIGZpbGw9ImN1cnJlbnRDb2xvciIgZm9udC1mYW1pbHk9Im1vbm9zcGFjZSIgZm9udC1zaXplPSIxNCI+MSBociA5IG1pbnM8L3RleHQ+PHRleHQgeD0iMjAiIHk9IjExNSIgZmlsbD0iY3VycmVudENvbG9yIiBmb250LWZhbWlseT0ibW9ub3NwYWNlIiBmb250LXNpemU9IjE0Ij5IVE1MPC90ZXh0PjxyZWN0IHg9IjE1MCIgeT0iMTAyIiB3aWR0aD0iMjgwIiBoZWlnaHQ9IjEyIiByeD0iMyIgZmlsbD0iIzMwMzYzZCIgLz48cmVjdCB4PSIxNTAiIHk9IjEwMiIgd2lkdGg9IjE3LjUiIGhlaWdodD0iMTIiIHJ4PSIzIiBmaWxsPSJoc2woMjE2LCA3MCUsIDU1JSkiIC8+PHRleHQgeD0iNDQwIiB5PSIxMTUiIGZpbGw9ImN1cnJlbnRDb2xvciIgZm9udC1mYW1pbHk9Im1vbm9zcGFjZSIgZm9udC1zaXplPSIxNCI+MzYgbWluczwvdGV4dD48dGV4dCB4PSIyMCIgeT0iMTM1IiBmaWxsPSJjdXJyZW50Q29sb3IiIGZvbnQtZmFtaWx5PSJtb25vc3BhY2UiIGZvbnQtc2l6ZT0iMTQiPnRxPC90ZXh0PjxyZWN0IHg9IjE1MCIgeT0iMTIyIiB3aWR0aD0iMjgwIiBoZWlnaHQ9IjEyIiByeD0iMyIgZmlsbD0iIzMwMzYzZCIgLz48cmVjdCB4PSIxNTAiIHk9IjEyMiIgd2lkdGg9IjguMDM2IiBoZWlnaHQ9IjEyIiByeD0iMyIgZmlsbD0iaHNsKDI4OCwgNzAlLCA1NSUpIiAvPjx0ZXh0IHg9IjQ0MCIgeT0iMTM1IiBmaWxsPSJjdXJyZW50Q29sb3IiIGZvbnQtZmFtaWx5PSJtb25vc3BhY2UiIGZvbnQtc2l6ZT0iMTQiPjE2IG1pbnM8L3RleHQ+PC9zdmc+" />

```rust
Total Time: 9 hrs 49 mins

- "C#"            ##################             5 hrs 54 mins
- "TypeScript"    ####                           1 hr 10 mins
- "Markdown"      ####                           1 hr 9 mins
- "HTML"          ##                             36 mins
- "tq"            #                              16 mins
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

<img src="data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNjAwIiBoZWlnaHQ9IjQwMCIgdmlld0JveD0iMCAwIDYwMCA0MDAiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+PHN0eWxlPnN2ZyB7CiAgICBmb250LWZhbWlseToKICAgICAgICAtYXBwbGUtc3lzdGVtLAogICAgICAgIEJsaW5rTWFjU3lzdGVtRm9udCwKICAgICAgICAiU2Vnb2UgVUkiLAogICAgICAgIEhlbHZldGljYSwKICAgICAgICBBcmlhbCwKICAgICAgICBzYW5zLXNlcmlmOwp9CgoudHJhY2stdGl0bGUgewogICAgZm9udC1zaXplOiAxNXB4OwogICAgZm9udC13ZWlnaHQ6IDYwMDsKICAgIGZpbGw6ICM3Nzc7Cn0KCi50cmFjay1hcnRpc3QgewogICAgZm9udC1zaXplOiAxM3B4OwogICAgZmlsbDogIzc3NzsKfQoKLnRyYWNrLWR1cmF0aW9uIHsKICAgIGZvbnQtc2l6ZTogMTNweDsKICAgIGZpbGw6ICM3Nzc7CiAgICB0ZXh0LWFuY2hvcjogZW5kOwp9CgoudHJhY2stY292ZXIgewogICAgd2lkdGg6IDYwcHg7CiAgICBoZWlnaHQ6IDYwcHg7Cn08L3N0eWxlPjxnIGNsYXNzPSJ0cmFjayI+PGltYWdlIGNsYXNzPSJ0cmFjay1jb3ZlciIgeD0iMTAiIHk9IjEwIiB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIGhyZWY9Imh0dHBzOi8vaXMxLXNzbC5tenN0YXRpYy5jb20vaW1hZ2UvdGh1bWIvTXVzaWMyMTEvdjQvYTAvOTMvMzMvYTA5MzMzODQtNjFlMi1lYzczLTc5NmYtMmM3N2ZiZDU5ZWEwL2FydHdvcmsuanBnLzYweDYwYmIuanBnIiAvPjx0ZXh0IGNsYXNzPSJ0cmFjay10aXRsZSIgeD0iODUiIHk9IjMyIj5NYWNoaW5lIExvdmU8L3RleHQ+PHRleHQgY2xhc3M9InRyYWNrLWFydGlzdCIgeD0iODUiIHk9IjUyIj5KYW1pZSBQYWlnZTwvdGV4dD48dGV4dCBjbGFzcz0idHJhY2stZHVyYXRpb24iIHg9IjU5MCIgeT0iMzIiPjM6MzY8L3RleHQ+PC9nPjxnIGNsYXNzPSJ0cmFjayI+PGltYWdlIGNsYXNzPSJ0cmFjay1jb3ZlciIgeD0iMTAiIHk9IjkwIiB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIGhyZWY9Imh0dHBzOi8vaXMxLXNzbC5tenN0YXRpYy5jb20vaW1hZ2UvdGh1bWIvTXVzaWMyMTEvdjQvMjUvOWMvYTUvMjU5Y2E1ZTEtYzM2NS04YjcyLWIxMmUtNjYwYWFlNmZmMjFkLzI1VU1HSU04NzY3OS5yZ2IuanBnLzYweDYwYmIuanBnIiAvPjx0ZXh0IGNsYXNzPSJ0cmFjay10aXRsZSIgeD0iODUiIHk9IjExMiI+T25lIE1hbiBDaXJjdXM8L3RleHQ+PHRleHQgY2xhc3M9InRyYWNrLWFydGlzdCIgeD0iODUiIHk9IjEzMiI+ZWxpbyBtZWk8L3RleHQ+PHRleHQgY2xhc3M9InRyYWNrLWR1cmF0aW9uIiB4PSI1OTAiIHk9IjExMiI+NTo0OTwvdGV4dD48L2c+PGcgY2xhc3M9InRyYWNrIj48aW1hZ2UgY2xhc3M9InRyYWNrLWNvdmVyIiB4PSIxMCIgeT0iMTcwIiB3aWR0aD0iNjAiIGhlaWdodD0iNjAiIGhyZWY9Imh0dHBzOi8vaXMxLXNzbC5tenN0YXRpYy5jb20vaW1hZ2UvdGh1bWIvTXVzaWMyMTEvdjQvMjUvOWMvYTUvMjU5Y2E1ZTEtYzM2NS04YjcyLWIxMmUtNjYwYWFlNmZmMjFkLzI1VU1HSU04NzY3OS5yZ2IuanBnLzYweDYwYmIuanBnIiAvPjx0ZXh0IGNsYXNzPSJ0cmFjay10aXRsZSIgeD0iODUiIHk9IjE5MiI+UGxheWluZyBEZWFkPC90ZXh0Pjx0ZXh0IGNsYXNzPSJ0cmFjay1hcnRpc3QiIHg9Ijg1IiB5PSIyMTIiPkVsaW8gTWVpPC90ZXh0Pjx0ZXh0IGNsYXNzPSJ0cmFjay1kdXJhdGlvbiIgeD0iNTkwIiB5PSIxOTIiPjQ6NDc8L3RleHQ+PC9nPjxnIGNsYXNzPSJ0cmFjayI+PGltYWdlIGNsYXNzPSJ0cmFjay1jb3ZlciIgeD0iMTAiIHk9IjI1MCIgd2lkdGg9IjYwIiBoZWlnaHQ9IjYwIiBocmVmPSJodHRwczovL2lzMS1zc2wubXpzdGF0aWMuY29tL2ltYWdlL3RodW1iL011c2ljMjExL3Y0LzI1LzljL2E1LzI1OWNhNWUxLWMzNjUtOGI3Mi1iMTJlLTY2MGFhZTZmZjIxZC8yNVVNR0lNODc2NzkucmdiLmpwZy82MHg2MGJiLmpwZyIgLz48dGV4dCBjbGFzcz0idHJhY2stdGl0bGUiIHg9Ijg1IiB5PSIyNzIiPlZlbGNybzwvdGV4dD48dGV4dCBjbGFzcz0idHJhY2stYXJ0aXN0IiB4PSI4NSIgeT0iMjkyIj5FbGlvIE1laTwvdGV4dD48dGV4dCBjbGFzcz0idHJhY2stZHVyYXRpb24iIHg9IjU5MCIgeT0iMjcyIj4xOjUzPC90ZXh0PjwvZz48ZyBjbGFzcz0idHJhY2siPjxpbWFnZSBjbGFzcz0idHJhY2stY292ZXIiIHg9IjEwIiB5PSIzMzAiIHdpZHRoPSI2MCIgaGVpZ2h0PSI2MCIgaHJlZj0iaHR0cHM6Ly9pczEtc3NsLm16c3RhdGljLmNvbS9pbWFnZS90aHVtYi9NdXNpYzExMy92NC8wMC9jNi80Yy8wMGM2NGNmYS0yN2M4LTgxY2QtYTEyYy0xY2FmYjVlODMyZjIvMDU0MzkxOTQ1ODUzLmpwZy82MHg2MGJiLmpwZyIgLz48dGV4dCBjbGFzcz0idHJhY2stdGl0bGUiIHg9Ijg1IiB5PSIzNTIiPlRoaXMgaXMgaG9tZTwvdGV4dD48dGV4dCBjbGFzcz0idHJhY2stYXJ0aXN0IiB4PSI4NSIgeT0iMzcyIj5DYXZldG93bjwvdGV4dD48dGV4dCBjbGFzcz0idHJhY2stZHVyYXRpb24iIHg9IjU5MCIgeT0iMzUyIj40OjI5PC90ZXh0PjwvZz48L3N2Zz4=" />

### Github

Integrations with the [Github](https://www.github.com) services.

#### Profile

The `github-profile` block tag provides data about someone's profile.

For allowing this service to work, the tool needs the following
variables:

- `github_api_token`: A token from github's API. No private access needed.
- `github_username`: The username of the targeted github user.

```html
<github-profile />
```

<div>
<!-- github-profile Not implemented! -->
</div>

#### Contributions

The `github-contributions` block tag provides data about someone's activity, history and contributions.

For allowing this service to work, the tool needs the following
variables:[README.md](README.md)

- `github_api_token`: A token from github's API. No private access needed.[README.md](README.md)
- `github_username`: The username of the targeted github user.

```html
<github-activity />
```

<github-activity />

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
<source media="(max-width: 1061px)" width="24%" srcset="./actions/cache/steam_recent/cards/1353300_thin.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="./actions/cache/steam_recent/cards/1353300_wide.svg" />
<img style="max-width: 100%;" alt="Idle Slayer – Incremental RPG" />
</picture>
</a>
<a href="https://store.steampowered.com/app/457140" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="./actions/cache/steam_recent/cards/457140_thin.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="./actions/cache/steam_recent/cards/457140_wide.svg" />
<img style="max-width: 100%;" alt="Oxygen Not Included" />
</picture>
</a>
<a href="https://store.steampowered.com/app/1919460" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="./actions/cache/steam_recent/cards/1919460_thin.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="./actions/cache/steam_recent/cards/1919460_wide.svg" />
<img style="max-width: 100%;" alt="Seraph's Last Stand" />
</picture>
</a>
<a href="https://store.steampowered.com/app/431730" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="./actions/cache/steam_recent/cards/431730_thin.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="./actions/cache/steam_recent/cards/431730_wide.svg" />
<img style="max-width: 100%;" alt="Aseprite" />
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
<source media="(max-width: 1061px)" width="24%" srcset="./actions/cache/steam_perfect/cards/1289310_thin.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="./actions/cache/steam_perfect/cards/1289310_wide.svg" />
<img style="max-width: 100%;" alt="Helltaker" />
</picture>
</a>
<a href="https://store.steampowered.com/app/433340" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="./actions/cache/steam_perfect/cards/433340_thin.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="./actions/cache/steam_perfect/cards/433340_wide.svg" />
<img style="max-width: 100%;" alt="Slime Rancher" />
</picture>
</a>
<a href="https://store.steampowered.com/app/255520" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="./actions/cache/steam_perfect/cards/255520_thin.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="./actions/cache/steam_perfect/cards/255520_wide.svg" />
<img style="max-width: 100%;" alt="Viscera Cleanup Detail: Shadow Warrior" />
</picture>
</a>
<a href="https://store.steampowered.com/app/1997680" target="_blank">
<picture>
<source media="(max-width: 1061px)" width="24%" srcset="./actions/cache/steam_perfect/cards/1997680_thin.svg" />
<source media="(min-width: 1061px)" width="49%" srcset="./actions/cache/steam_perfect/cards/1997680_wide.svg" />
<img style="max-width: 100%;" alt="REFLEXIA Prototype ver." />
</picture>
</a>
</p>
<p align="center">
<sub>
<i>Disclaimer: All game titles, arts, logos, and trademarks belong to Steam (Valve Corporation) and their respective developers.</i>
</sub>
</p>
