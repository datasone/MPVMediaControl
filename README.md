# MPVMediaControl
This tool adds SMTC feature to mpv player, it communicates with mpv by named pipe, and can be controlled by any other programs.

<img src="https://github.com/datasone/MPVMediaControl/blob/master/img/screenshot.png?raw=true" height="175px" />

*Screenshot on Windows 11 22000 (It's much more useable than the one in 10)*

## Features
- Media metadata display including title and artist information
- Thumbnail generated from mpv screenshot or youtube cover image
- Media controls (play/pause, prev, next)

## Usage
Put `notify_media.lua` in mpv's `scripts` directory and place `MPVMediaControl.exe` to `~~/bin`.

You can change the default settings via the `notify_media.conf` file.

A `Reset SMTC` item in menu will reset the state of SMTC, useful when Windows is glitched and controls are not working properly (e.g. not displaying or disappearing).

For retrieving youtube cover image, `curl.exe` is used. It has been bundled in Windows 10 since [1803, or actually build 17063](https://devblogs.microsoft.com/commandline/tar-and-curl-come-to-windows/). So if you are using older versions of Windows, you may need to manually download and put `curl.exe` into $PATH.

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
- `setShot`: set screenshot file path
- `setState`: set player state (playing or not)
- `setQuit`: notify the tool that player has quit, and the SMTC should be cleared

### Parameters
- Common parameters
  - `pid={num}`: the player's pid. This is a mandatory parameter for every command, it is used to identify different instances.
- Parameters for `setFile`
  - `title={hexString}`: the title of the media file
  - `artist={hexString}`: the artist of the media file, music files only. The value can be empty.
  - `path={hexString}`: the path of the media file
  - `type={music,video,image}`: the type of the media file
- Parameters for `setShot`
  - `shot_path={hexString}`: the path of the thumbnail image file. The value can be empty.

The `hexString` mentioned above means the original string should be encoded to hex, e.g. `Hello <=> 48656C6C6F`
- Parameters for `setState`
  - `playing={bool}`: if the media is being played, `true` for playing, `false` for pausing
- Parameters for `setQuit`
  - `quit=true`: this is always true if you want to quit

The media control part works by sending commands to mpv's ipc socket, which `notify_media.lua` will set mpv to listen on, called `mpvsocket_{pid}`.
