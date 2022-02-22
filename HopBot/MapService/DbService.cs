using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HopBot.Infrastructure;
using HopBot.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HopBot.MapService
{
    public class DbService
    {
        private readonly SBotDatabase _db;
        private readonly ILogger<DbService> _log;

        public DbService(SBotDatabase db, ILogger<DbService> log)
        {
            _db = db;
            _log = log;
        }

        public async Task AddMap(BhopMap map)
        {
            try
            {
                if (map != null && !_db.Maps.Any(m => m.MapName == map.MapName))
                {
                    await _db.AddAsync(map);
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }
        }

        public async Task<BhopMap> GetMap(string mapName)
        {
            if (string.IsNullOrEmpty(mapName))
                return null;

            return await _db.Maps.AsNoTracking().FirstOrDefaultAsync(m => m.MapName == mapName); ;
        }
    }
}
