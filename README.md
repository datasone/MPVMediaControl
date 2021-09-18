# MPVMediaControl
This tool adds SMTC feature to mpv player, it communicates with mpv by named pipe, and can be controlled by any other programs.

<img src="https://github.com/datasone/MPVMediaControl/blob/master/img/screenshot.png?raw=true" height="175px" />

*Screenshot on Windows 11 22000 (It's much more useable than the one in 10)*

## Features
- Media metadata display including title and artist information
- Thumbnail generated from mpv screenshot
- Media controls (play/pause, prev, next)

## Usage
Put `notify_media.lua` in mpv's `scripts` directory and make sure `MPVMediaControl` is running in background.

## Protocol
This tool listens on a named pipe called `mpvmcsocket`, and receives commands through this protocol:
```
[commandName](param1=value1)(param2=value2)...
```

e.g. 

```
[setState](pid=1000)(playing=true)
```

### Commands
- `setFile`: set currently playing file
- `setState`: set player state (playing or not)
- `setQuit`: notify the tool that player has quit, and the SMTC should be cleared

### Parameters
- Common parameters
  - `pid={num}`: the player's pid. This is a mandatory parameter for every command, it is used to identify different instances.
- Parameters for `setFile`
  - `title={hexString}`: the title of the media file
  - `artist={hexString}`: the artist of the media file, music files only. The value can be empty.
  - `path={hexString}`: the path of the media file
  - `shot_path={hexString}`: the path of the thumbnail image file. The value can be empty.
  
  The `hexString` mentioned above means the original string should be encoded to hex, e.g. `Hello <=> 48656C6C6F`
- Parameters for `setState`
  - `playing={bool}`: if the media is being played, `true` for playing, `false` for pausing
- Parameters for `setQuit`
  - `quit=true`: this is always true if you want to quit

The media control part works by sending commands to mpv's ipc socket, which `notify_media.lua` will set mpv to listen on, called `mpvsocket_{pid}`.
