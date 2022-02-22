using HopBot.Helpers;
using HopBot.MapService;
using HopBot.Models;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HopBot.Commands
{
    public class RequestModule : BaseModule
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<RequestModule> _log;
        private readonly IConfiguration _config;

        private string _mapFileBsp = String.Empty;
        public RequestModule(IServiceProvider services, ILogger<RequestModule> log, IConfiguration config)
        {
            _services = services;
            _log = log;
            _config = config;
        }

        [Command("request")]
        [Alias("r", "req", "download", "dl", "get", "getmap")]
        public async Task DownloadMapAsync(string msg)
        {
            if (String.IsNullOrEmpty(msg))
                return;

            if (Context.Channel.Id != _config.GetValue<ulong>("DiscordRequestChannel"))
                return;

            if (String.IsNullOrEmpty(_config.GetValue<string>("MapDownloaderRole")))
                return;

            if (Context.User is not SocketGuildUser gUser)
                return;

            if (!gUser.Roles.Any(r => r.Name == _config.GetValue<string>("MapDownloaderRole")))
            {
                try
                {
                    await SendChannelMessageDelete("You don't have the rights to use this command.", 5);
                    return;
                }
                catch (Exception ex)
                {
                    _log.LogError(ex.Message);
                }
            }

            try
            {
                // Delete the request chat message
                await Context.Message.DeleteAsync();

                // Get our required services
                var fileService = _services.GetRequiredService<FileService>();
                var dbService = _services.GetRequiredService<DbService>();

                // Gamebanana method, if null jumps to acers method and tries to parse it there
                var mapRequest = fileService.ParseMapRequestGb(msg);
                if(mapRequest != null)
                {
                    var jsongb = await fileService.GetGamebananaMap(mapRequest).ConfigureAwait(false);
                    if (jsongb == null)
                    {
                        await SendChannelMessageDelete("Could not find map. Are you sure you are requesting a bhop map?", 10);
                        return;
                    }
                    
                    var gb = JsonConvert.DeserializeObject<Gamebanana.Data>(jsongb);
                    if (gb == null)
                    {
                        await SendChannelMessageDelete("This map does not exist. Are you sure you are requesting a bhop map?", 10);
                        return;
                    }

                    // 5568 is Gamebananas CS:S BHOP category, fuck you Gamebanana
                    if(gb._aCategory._idRow != 5568)
                    {
                        await SendChannelMessageDelete("This map does not exist for CS:S. Are you sure you are requesting a CS:S bhop map?", 10);
                        return;
                    }

                    var _map = new BhopMap
                    {
                        MapCreator = gb._aSubmitter._sName ?? "Unknown",
                        MapCreatorAvatar = gb._aSubmitter._sAvatarUrl ?? "https://images.gamebanana.com/static/img/defaults/avatar.gif",
                        MapName = gb._sName ?? "Unknown",
                        MapUploadDate = TimeStamp.UnixTimeStampToDateTime(gb._aFiles[0]._tsDateAdded),
                        MapImage = $"{gb._aPreviewMedia[0]._sBaseUrl}/{gb._aPreviewMedia[0]._sFile530 ?? "530-90_55134131f21c0.jpg"}",
                        MapDownloadLink = gb._aFiles[0]._sDownloadUrl ?? "",
                        MapFile = gb._aFiles[0]._sFile,
                        MapId = gb._idRow,
                        RequestedBy = $"{gUser.Username}#{gUser.Discriminator}",
                        RequestedDate = DateTime.UtcNow
                    };

                    foreach (var item in gb._aFiles[0]._aMetadata._aArchiveFileTree)
                    {
                        if (Path.GetExtension(item) == ".bsp")
                            _map.MapName = item.Split('.')[0];
                    }

                    if (fileService.CheckMapInMaplist(_map.MapName))
                    {
                        await SendChannelMessageDelete("This map is already available.", 5);
                        return;
                    }
                    fileService.AddToMaplist(_map.MapName);

                    var embedMessageGb = await SendEmbedMessage(_map);
                    var downloadMessageGb = await Context.Channel.SendMessageAsync($"```Downloading map {_map.MapName}...```");

                    if (fileService.DownloadFile(_map.MapDownloadLink, _map.MapFile))
                        await downloadMessageGb.ModifyAsync(msg => msg.Content = $"```Download completed. Decompressing...```");

                    fileService.ExtractFile(_map.MapFile, out _mapFileBsp);

                    // At this point it's already extracted, so lets add it to our db
                    await dbService.AddMap(_map);

                    if (string.IsNullOrEmpty(_mapFileBsp))
                    {
                        await ModifyDelete(downloadMessageGb, "Request completed successfully.", 5);
                        await EmbedMessageReact(embedMessageGb);
                        return;
                    }

                    await downloadMessageGb.ModifyAsync(msg => msg.Content = $"```Decompressing completed. Now compressing to Fastdl...```");
                    fileService.CompressToFastdl(_mapFileBsp);

                    await ModifyDelete(downloadMessageGb, "Request completed successfully.", 5);
                    await EmbedMessageReact(embedMessageGb);
                    return;
                }

                // acers fastdl method
                if (mapRequest == null)
                    mapRequest = fileService.ParseMapRequest(msg);

                // The one nation gave us such good download speeds to europe, so let's thank them
                await SendChannelMessageDelete("This might take a few seconds, please be patient.", 10);

                if (mapRequest == null)
                {
                    await SendChannelMessageDelete("This is not a valid map.", 10);
                    return;
                }

                if(!await fileService.CheckIfAcerMapExists(mapRequest))
                {
                    await SendChannelMessageDelete("Map not found on acer's fastdl", 10);
                    return;
                }

                if (await dbService.GetMap(mapRequest.ToLower().Trim()) != null)
                {
                    await SendChannelMessageDelete("This map has already been requested and can be played.", 10);
                    return;
                }

                if (fileService.CheckMapInMaplist(mapRequest))
                {
                    await SendChannelMessageDelete("This map is already available.", 5);
                    return;
                }
                fileService.AddToMaplist(mapRequest);

                var embedMessage = await SendEmbedMessage(gUser.Username, mapRequest);

                await dbService.AddMap(new BhopMap
                {
                    MapName = mapRequest.Trim().ToLower(),
                    MapCreator = null,
                    MapDownloadLink = null,
                    MapUploadDate = DateTime.UtcNow,
                    RequestedBy = $"{gUser.Username}#{gUser.Discriminator}",
                    RequestedDate = DateTime.UtcNow
                });
                
                var downloadMessage = await Context.Channel.SendMessageAsync($"```Downloading map {mapRequest.Trim().ToLower()}...```");

                // case sensitive on acers fastdl, fucking standardize map names already ffs
                if (fileService.DownloadFile($"http://sojourner.me/fastdl/maps/{mapRequest.Trim()}.bsp.bz2", $"{mapRequest.Trim()}.bsp.bz2"))
                    await downloadMessage.ModifyAsync(msg => msg.Content = $"```Download completed. Decompressing...```");

                fileService.ExtractFile($"{mapRequest.Trim()}.bsp.bz2", out _mapFileBsp, true);

                await downloadMessage.ModifyAsync(msg => msg.Content = $"```Decompressing completed. Now compressing to Fastdl...```");
                fileService.CompressToFastdl(_mapFileBsp);

                await ModifyDelete(downloadMessage, "Request completed successfully.", 5);
                await EmbedMessageReact(embedMessage);
                return;
                
            }
            catch (Exception ex)
            {
                await SendChannelMessageDelete("Could not find requested map. Are you sure that map is on Gamebanana?", 15);
                _log.LogError(ex.Message);
            }
        }
    }
}
