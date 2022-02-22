using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HopBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace HopBot
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly ILogger<CommandHandler> _log;

        private readonly Dictionary<ulong, DateTime> _commandCache = new();
        private readonly System.Timers.Timer _timer = new(1000)
        {
            AutoReset = true,
            Enabled = true
        }; 

        public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService service, IConfiguration config) : base(client, logger)
        {
            _provider = provider;
            _service = service;
            _config = config;
            _log = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Client.WaitForReadyAsync(stoppingToken);
            Logger.LogInformation("Client is ready!");
            await Client.SetActivityAsync(new Game("BHOP"));

            Client.MessageReceived += OnMessageReceived;
            _service.CommandExecuted += OnCommandExecuted;

            _timer.Elapsed += OnElapsed;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        // command cache, gets cleared every second
        private async void OnElapsed(object unused, ElapsedEventArgs arg)
        {
            foreach (var pair in _commandCache.Where(x => x.Value <= DateTime.UtcNow))
            {
                _commandCache.Remove(pair.Key);
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext commandContext, IResult result)
        {
            if (String.IsNullOrEmpty(_config.GetValue<ulong>("DiscordLogChannel").ToString()))
                return;

            if (_commandCache.TryGetValue(commandContext.Client.CurrentUser.Id, out _))
                return;
            else
            {
                _commandCache.Add(commandContext.Client.CurrentUser.Id, DateTime.UtcNow.AddSeconds(Options.CommandDelay));
            }

            var logChannel = Client.GetChannel(_config.GetValue<ulong>("DiscordLogChannel")) as SocketTextChannel;
            if (result.IsSuccess)
            {
                await logChannel.SendMessageAsync($"**{commandContext.Message.Author.Username}#{commandContext.Message.Author.Discriminator}** executed `{commandContext.Message.Content ?? "N/A"}` in channel `{commandContext.Channel.Name}`");
                return;
            }
            
            _log.LogError(result.ErrorReason);
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message)
                return;

            if (message.Source != MessageSource.User)
                return;

            var argPos = 0;
            if (socketMessage.Content.StartsWith(Client.CurrentUser.Mention))
            {
                if (_commandCache.TryGetValue(socketMessage.Author.Id, out _))
                    return;
                else
                    _commandCache.Add(socketMessage.Author.Id, DateTime.UtcNow.AddSeconds(Options.CommandDelay));

                await socketMessage.Channel.SendMessageAsync(String.IsNullOrEmpty(_config.GetValue<string>("DiscordPingEmote")) ? ":eyes:" : _config.GetValue<string>("DiscordPingEmote"));
                return;
            }

            if (!message.HasStringPrefix(_config["DiscordCommandPrefix"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
                return;

            var context = new SocketCommandContext(Client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }
    }
}
