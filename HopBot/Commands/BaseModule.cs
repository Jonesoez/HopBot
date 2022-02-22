using Discord;
using Discord.Commands;
using Discord.Rest;
using HopBot.Models;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace HopBot.Commands
{
    public abstract class BaseModule : ModuleBase<SocketCommandContext>
    {
        public async Task SendChannelMessageDelete(string message, int deleteDelay)
        {
            var userMessageRights = await Context.Channel.SendMessageAsync(message);
            await Task.Delay(TimeSpan.FromSeconds(deleteDelay)).ConfigureAwait(false);
            await userMessageRights.DeleteAsync().ConfigureAwait(false);
        }

        public async Task<RestUserMessage> SendEmbedMessage(string commandUser, string map)
        {
            var author = new EmbedAuthorBuilder()
                .WithName($"Downloading map via acer's fastdl");

            var footer = new EmbedFooterBuilder()
                .WithText($"Requested by {commandUser}");

            var embed = new EmbedBuilder()
                .WithTitle(map.Trim())
                .WithAuthor(author)
                .WithFooter(footer)
                .WithCurrentTimestamp()
                .WithColor(color: Color.Green)
                .Build();

            return await Context.Channel.SendMessageAsync(embed: embed);
        }

        public async Task<RestUserMessage> SendEmbedMessage(BhopMap map)
        {
            var authorGb = new EmbedAuthorBuilder()
                .WithName($"Map by {map.MapCreator}")
                .WithIconUrl(map.MapCreatorAvatar);

            var footerGb = new EmbedFooterBuilder()
                .WithText($"Requested by {map.RequestedBy}");

            var embedGb = new EmbedBuilder()
                .WithTitle(map.MapName)
                .WithAuthor(authorGb)
                .AddField("Release date:", map.MapUploadDate.ToString("dd. MMMM yyyy", new CultureInfo("en-GB")), false)
                .WithImageUrl(map.MapImage)
                .AddField("Map ID:", map.MapId, false)
                .AddField("Mapname:", map.MapName, false)
                .WithFooter(footerGb)
                .WithCurrentTimestamp()
                .WithColor(color: Color.DarkBlue)
                .Build();

            return await Context.Channel.SendMessageAsync(embed: embedGb);
        }

        public async Task EmbedMessageReact(RestUserMessage embedMessage)
        {
            await embedMessage.RemoveAllReactionsAsync();
            await embedMessage.AddReactionAsync(emote: Emoji.Parse("✅"));
            await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        }

        public async Task ModifyDelete(RestUserMessage embedMessage, string message, int deleteDelay = 10)
        {
            await embedMessage.ModifyAsync(msg => msg.Content = $"```{message}```");
            await Task.Delay(TimeSpan.FromSeconds(deleteDelay)).ConfigureAwait(false);
            await embedMessage.DeleteAsync().ConfigureAwait(false);
        }
    }
}
