using HopBot.MapService;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using HopBot.Infrastructure;

namespace HopBot
{
    public class Program
    {
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        public static async Task Main()
        {
            using var db = new SBotDatabase();
            db.Database.EnsureCreated();

            var buildsettings = new ConfigurationBuilder();
            BuildConfig(buildsettings);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(buildsettings.Build())
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("Logs", "Log-.txt"),
                    retainedFileCountLimit: 20,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            Log.Logger.Information("Bot is starting up...");

            var builder = Host.CreateDefaultBuilder()
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Debug,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                        GatewayIntents = GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessageTyping | GatewayIntents.Guilds
                    };

                    config.Token = context.Configuration["DiscordToken"];
                })
                .UseCommandService((context, config) =>
                {
                    config.DefaultRunMode = RunMode.Async;
                    config.CaseSensitiveCommands = false;
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<SBotDatabase>();
                    services.AddHttpClient();
                    services.AddSingleton<FileService>();
                    services.AddSingleton<DbService>();
                    services.AddHostedService<CommandHandler>();
                })
                .UseSerilog()
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }

        public static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
        }
    }
}
