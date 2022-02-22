using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HopBot.Models;
using System;
using System.Threading.Tasks;

namespace HopBot.Commands
{
    public class GeneralModule : BaseModule
    {
        private readonly IServiceProvider _services;
        private readonly IConfiguration _config;
        private readonly ILogger<GeneralModule> _log;

        public GeneralModule(IServiceProvider services, IConfiguration config, ILogger<GeneralModule> log)
        {
            _services = services;
            _config = config;
            _log = log;
        }

        [Command("delay")]
        [Alias("changedelay", "spamdelay", "chatdelay", "spamdelay", "commanddelay", "cdelay")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ChangeChatDelayAsync(int delay)
        {
            if (!int.TryParse(delay.ToString(), out _))
                return;

            if (delay >= 2 && delay <= 300)
            {
                Options.CommandDelay = delay;
                await SendChannelMessageDelete($"Delay of commands changed to {delay} seconds.", 5);
                return;
            }
            else
            {
                await SendChannelMessageDelete("Make sure the value is between 2 and 300 seconds.", 5);
                return;
            }
        }
    }
}
