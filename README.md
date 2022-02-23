# HopBot

This is a discord bot that downloads bhop maps to your server(s). It's written in C# with Discord.Net. It runs both on Windows and Linux.

## Setup
1. Create a new discord token [here](https://discord.com/developers/applications). You can use an existing one too but make sure the command prefixes are not identical. (*)
2. Create a new role in discord e.g. `Map Downloader` and assign it to the users that will have the permissions to use the bot. (*)
3. Create a hidden channel for logging and a channel for the map requests. (*)
4. You can define a ping emote to check if the bot is running. The default response is `:eyes:`, in the config example, it will be `:pingemote:`.
5. If you have multiple servers make sure they are setup just like in the example below.
6. Rename the `appsettings.json.example` to `appsettings.json`and place it in the same folder as the executable. (*)
7. Run it

Steps marked with a `(*)` are needed to run the bot.

**Note**: You can get the channel and emote id with a `\` before the emote or channel link. You can also activate developer mode in your discord client and copy it from there too.
#### appsettings.json.example
```json
{
  "DiscordToken": "xxxxxxxxxxxxxxx",
  "DiscordCommandPrefix": "!",
  "DiscordLogChannel": 93971258174561734,
  "DiscordRequestChannel": 93971258174561734,
  "DiscordPingEmote": "<:pingemote:93971258174561734>",
  "DownloadPath": "tmp_maps",
  "ExtractPath": [
    "\/var\/lib\/pterodactyl\/volumes\/server1\/cstrike\/maps",
    "\/var\/lib\/pterodactyl\/volumes\/server2\/cstrike\/maps"
  ],
  "MapListFile": [
    "\/var\/lib\/pterodactyl\/volumes\/server1\/cstrike\/cfg\/mapcycle.txt",
    "\/var\/lib\/pterodactyl\/volumes\/server2\/cstrike\/cfg\/mapcycle.txt"
  ],
  "FastDlPath": "\/var\/lib\/pterodactyl\/volumes\/web\/www\/fastdl\/maps",
  "MapDownloaderRole": "Map Downloader"
}
```
