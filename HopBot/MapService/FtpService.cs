using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace HopBot.MapService
{
    public class FtpService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FtpService> _log;
        private readonly IHttpClientFactory _httpclientfactory;

        public FtpService(IConfiguration config, ILogger<FtpService> log, IHttpClientFactory httpclientfactory)
        {
            _config = config;
            _log = log;
            _httpclientfactory = httpclientfactory;
        }

        public async Task UploadMap(string map)
        {
            // TODO
        }
    }
}
