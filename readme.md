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
- iggtix_token - bot token
- iggtix_username - bot username

Run the application.

The bot responds to the following commands by default:
- #add #trigger response (mod only command)
- #del #trigger (mod only command)
- #stepdaddy - returns a random chatter
- #userinfo - returns the bot's username
- #eldenringitem - returns a random Elden Ring item
- #hello? - returns 'hello'

## Licencing

iggtix is licenced under the MIT license. Full licence details are available in license.md