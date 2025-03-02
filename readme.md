## Introduction

iggtix is simple twitch bot.

## Download

Compiled downloads are not available.

## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/asciiz

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Usage

Set the following environment variables:

- iggtix_broadcasterid - twitch user id of the broadcaster
- iggtix_channel - name of the twitch channel to join
- iggtix_clientid - bot clientid
- iggtix_dbpath - path and filename to create the local SQLite database
- iggtix_moderatorid - twitch user id of the moderator
- iggtix_plugindir - plugin directory
- iggtix_token - bot token
- iggtix_username - bot username

Run the application.

The bot responds to the following commands by default:
- #add #trigger response (mod only command)
- #del #trigger (mod only command)
- #adda #trigger response (mod only command)
- #dela #trigger response (mod only command)
- #lovecheck {user} - calculates a relationship between the invoking user and the target user
- #userinfo - returns the bot's username

Note: The token {chatter} will be replaced with a random chatter from Twitch chat.


## Plugins
Plugins can be added into the Plugin Directory. Plugins must implement the IIggtixCommand interface, from the IIggtixCommand assembly.

Plugins are called in a non-deterministic order based on a message content marker, indicated as {p:Name}.

As an example the included Elden Ring API plugin contains two Handlers, one replaces the input message the other amendtsw the input message.

The first command would be called if the response contains {p:EldenRingApi} while the second command would be called if the response contains {p:EldenRingApi2}


## Licencing

iggtix is licenced under the MIT license. Full licence details are available in license.md