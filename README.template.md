# MD+
A new way of enchanting your github overview

---

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
```

<badge color="202020" icon="python">python</badge>
<badge color="303030" style="flat" icon="c">C</badge> \
<badge color="404040" style="flat-square" icon="c++">C++</badge> \
<badge color="505050" style="plastic" icon="dotnet">C#</badge> \
<badge color="606060" style="for-the-badge" icon="zig">Zig</badge> \
<badge color="202020" style="social" icon="lua">Lua</badge>

The following attributes can be applied to a `badge` tag:
- `icon`: The icon shown in the tag. Icons provided by [Simple Icons](https://simpleicons.org/).
- `style`: Badge style. Options are  [`flat`, `flat-square`, `plastic`, `for-the-badge`, `social`] (default is `flat`)
- `color`: The tag's background color.
- `icon-color`: The tag's icon color.
- `label-color`: The tag's label color.

### Typing

Integration with [readme-typing-svg](https://readme-typing-svg.herokuapp.com/demo/) made straight forward.

The `typing` tag is a block tag that can be used to create typing animations.

```html
<typing
    font="Rock Salt " size="20" duration="2000" pause="150"
    width="500" height="100" repeat="true"
>
Look at me!
I'm typing!
</typing>
```

<typing
font="Rock Salt " size="20" duration="3000" pause="300"
width="500" height="120" repeat="off"
>
Look at me!
I'm typing!
</typing>

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
<wakatime option="weekly-langs" />
```

<wakatime option="weekly-langs" />


### Last.fm

Integrations with the [last.fm](https://www.last.fm/home) service.

The `last-fm` block tag provides the list of your mostly listened
songs, as tracked by last.fm.

For allowing this service to work, the tool needs the following
variables:
- `lastfm_api_key`: The API key provided by the last.fm service.
- `lastfm_username`: The username of the targeted last.fm profile.


```html
<last-fm />
```

<last-fm />

### Github

Integrations with the [Github](https://www.github.com) services.

The `github` block tag provides data about someone's github profile,
history and contributions.

For allowing this service to work, the tool needs the following
variables:
- `github_api_token`: A token from github's API. No private access needed.
- `github_username`: The username of the targeted github user.

Some options are:
- `activity`: Recent github activity. Includes commits, pull requests,
  issues and discussions.

```html
<github option="activity" />
```

<github option="activity" />

### Steam

Integrations with the [Steam](https://store.steampowered.com/) services.

The `steam-lib` block tag provides data about a user's steam library.

For allowing this service to work, the tool needs the following
variables:
- `steam_api_key`: A steam API key.
- `steam_user_id`: The user id of the targeted steam account.

Some options are:
- `recent`: Recent played games.
- `perfected`: Last perfected (100% achievements) games.

```html
<steam-lib option="recent" />
<steam-lib option="perfected" />
```

<steam-lib option="recent" />
<steam-lib option="perfected" />
