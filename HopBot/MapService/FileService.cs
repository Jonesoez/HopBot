using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpCompress.Readers;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using SharpCompress.Common;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Net;
using SevenZipExtractor;

namespace HopBot.MapService
{
    public class FileService
    {
        public static readonly Regex RegexGb = new (@"((https|https)\:\/\/)((gamebanana.com)(.*)\/(.*?))\/(\d+)");

        private readonly IConfiguration _config;
        private readonly ILogger<FileService> _log;
        private readonly IHttpClientFactory _httpclientfactory;

        public FileService(IHttpClientFactory httpClientFactory, IConfiguration configs, ILogger<FileService> log)
        {
            _httpclientfactory = httpClientFactory;
            _config = configs;
            _log = log;

            Directory.CreateDirectory(_config.GetValue<string>("DownloadPath"));
        }

        public string ParseMapRequest(string message)
        {
            if (String.IsNullOrEmpty(message))
                return null;

            if (message.All(char.IsDigit))
                return null;

            //if (_config.GetSection("MapTypes").GetChildren().Any(m => message.Contains(m.Value, StringComparison.OrdinalIgnoreCase)))
            //    return message.ToLower();

            return message.Trim();
        }

        public string ParseMapRequestGb(string message)
        {
            if (String.IsNullOrEmpty(message))
                return null;

            if (message.All(char.IsDigit))
                return message;

            Match match = RegexGb.Match(message);
            if (match.Success)
                return match.Groups[7].Value;

            return null;
        }

        public async Task<string> GetGamebananaMap(string modId)
        {
            if (String.IsNullOrEmpty(modId))
                return null;

            try
            {
                using var client = _httpclientfactory.CreateClient();
                using var response = await client.GetAsync($"https://gamebanana.com/apiv5/Mod/{modId}?_csvProperties=_idRow,_sName,_aFiles,_aGame,_sName,_aPreviewMedia,_aSubmitter,_aCategory&_csvFlags=FILE_METADATA");

                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }

            return null;
        }

        public bool CheckMapInMaplist(string mapName)
        {
            if (string.IsNullOrEmpty(mapName))
                return true;

            var mapListFiles = _config.GetSection("MapListFile").GetChildren();

            // If you have multiple bhop servers configured, make sure your mapcycle/maplist files are synced together.
            // If one of the servers has the map listed and the others not, it will not download it.
            foreach (var mapListFile in mapListFiles)
            {
                foreach(var map in File.ReadAllLines(mapListFile.Value))
                    if (map == mapName)
                        return true;
            }

            return false;
        }

        public void AddToMaplist(string mapName)
        {
            if (string.IsNullOrEmpty(mapName))
                return;

            try
            {
                var mapLists = _config.GetSection("MapListFile").GetChildren();

                foreach(var mapList in mapLists)
                {
                    using StreamWriter readMapList = File.AppendText(mapList.Value);
                    readMapList.WriteLine(mapName);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
        }

        public async Task<bool> CheckIfAcerMapExists(string mapName)
        {
            if (String.IsNullOrEmpty(mapName))
                return false;

            try
            {
                using var client = _httpclientfactory.CreateClient();
                using var response = await client.GetAsync($"http://sojourner.me/fastdl/maps/{mapName}.bsp.bz2");

                if (response.StatusCode != HttpStatusCode.OK)
                    return false;

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }

            return false;
        }

        public bool DownloadFile(string url, string filename)
        {
            if (String.IsNullOrEmpty(_config.GetValue<string>("DownloadPath")))
                return false;

            if (!Directory.Exists(_config.GetValue<string>("DownloadPath")))
                return false;

            var mapfolders = _config.GetSection("ExtractPath").GetChildren();
            if(mapfolders == null)
            {
                _log.LogError($"Extract directories are not set!");
                return false;
            }

            foreach (var mapfolder in mapfolders)
            {
                if (!Directory.Exists(mapfolder.Value))
                {
                    _log.LogError($"The directory {mapfolder.Value} does not exist. Make sure you set the directories right!");
                    return false;
                }
            }

            using var _client = _httpclientfactory.CreateClient();
            using var response = _client.GetAsync(url);

            using (var stream = response.Result.Content.ReadAsStreamAsync())
            {
                var fileInfo = new FileInfo(Path.Combine(_config.GetValue<string>("DownloadPath"), filename));
                using var fileStream = fileInfo.OpenWrite();
                stream.Result.CopyTo(fileStream);
                stream.Result.Close();
            }
            return true;
        }

        public void ExtractFile(string compressedFile, out string fileName, bool acerMethod = false)
        {
            fileName = null;
            if (!File.Exists(Path.Combine(_config.GetValue<string>("DownloadPath"), compressedFile)))
                return;

            var mapfolders = _config.GetSection("ExtractPath").GetChildren();
            if (mapfolders == null)
                return;

            try
            {
                using Stream stream = File.OpenRead(Path.Combine(_config.GetValue<string>("DownloadPath"), compressedFile));

                // bz2 method/acer method
                if(acerMethod)
                {
                    if (Path.GetExtension(compressedFile) == ".bz2")
                    {
                        BZip2.Decompress(stream, File.Create(Path.Combine(_config.GetSection("ExtractPath:0").Value, compressedFile.Split('.')[0] + ".bsp")), true);
                        fileName = compressedFile.Split('.')[0] + ".bsp";
                        File.Delete(Path.Combine(_config.GetValue<string>("DownloadPath"), compressedFile));
                    }

                    if (String.IsNullOrEmpty(fileName))
                        return;

                    // skip first entry path since its already extracted there
                    foreach (var mapfolder in mapfolders.Skip(1))
                    {
                        if (!File.Exists(Path.Combine(mapfolder.Value, fileName)))
                            File.Copy(Path.Combine(_config.GetSection("ExtractPath:0").Value, fileName), Path.Combine(mapfolder.Value, fileName));
                    }
                    return;
                }

                // 7z method
                if(Path.GetExtension(compressedFile) == ".7z")
                {
                    using (ArchiveFile archiveFile = new ArchiveFile(stream))
                    {
                        foreach (SevenZipExtractor.Entry entry in archiveFile.Entries)
                        {
                            if (Path.GetExtension(entry.FileName) == ".bsp")
                            {
                                entry.Extract(Path.Combine(_config.GetSection("ExtractPath:0").Value, entry.FileName));
                                fileName = entry.FileName;
                            }
                        }
                    }
                    stream.Close();
                    File.Delete(Path.Combine(_config.GetValue<string>("DownloadPath"), compressedFile));

                    if (String.IsNullOrEmpty(fileName))
                        return;

                    foreach (var mapfolder in mapfolders.Skip(1))
                    {
                        if (!File.Exists(Path.Combine(mapfolder.Value, fileName)))
                            File.Copy(Path.Combine(_config.GetSection("ExtractPath:0").Value, fileName), Path.Combine(mapfolder.Value, fileName));
                    }
                    return;
                }

                // every other compression type
                using var reader = ReaderFactory.Open(stream);
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        if (Path.GetExtension(reader.Entry.Key) == ".bsp")
                        {
                            fileName = reader.Entry.Key;
                            reader.WriteEntryToDirectory(_config.GetSection("ExtractPath:0").Value, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                }
                stream.Close();
                File.Delete(Path.Combine(_config.GetValue<string>("DownloadPath"), compressedFile));

                if (String.IsNullOrEmpty(fileName))
                    return;

                foreach (var mapfolder in mapfolders.Skip(1))
                {
                    if(!File.Exists(Path.Combine(mapfolder.Value, fileName)))
                        File.Copy(Path.Combine(_config.GetSection("ExtractPath:0").Value, fileName), Path.Combine(mapfolder.Value, fileName));
                }
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }
        }
        
	//New Function :v
	    public void CopyToFastdl(string filename)
        {
            var _mapbsp = Path.Combine(_config.GetValue<string>("ExtractPath:0"), filename);
            var _tobz2 = Path.Combine(_config.GetValue<string>("FastDlPath"), filename + ".bz2");
            var _tobsp = Path.Combine(_config.GetValue<string>("FastDlPath"), filename);

            if (!File.Exists(_mapbsp))
                return;

            if (File.Exists(_tobz2))
            {
                _log.LogInformation("Map already compressed and existing in {FastDlFolder}", _config.GetValue<string>("FastDlPath"));
                return;
            }
			
            if (File.Exists(_tobsp))
            {
                _log.LogInformation("Map already exists in {FastDlFolder}", _config.GetValue<string>("FastDlPath"));
                return;
            }
		
            try
            {
		        File.Copy(_mapbsp, _tobsp, true);
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }
        }
        
        public void CompressToFastdl(string filename)
        {
            var _mapbsp = Path.Combine(_config.GetValue<string>("ExtractPath:0"), filename);
            var _tobz2 = Path.Combine(_config.GetValue<string>("FastDlPath"), filename + ".bz2");

            if (!File.Exists(_mapbsp))
                return;

            if (File.Exists(_tobz2))
            {
                _log.LogInformation("Map already compressed and existing in {FastDlFolder}", _config.GetValue<string>("FastDlPath"));
                return;
            }

            try
            {
                BZip2.Compress(File.OpenRead(_mapbsp), File.Create(_tobz2), true, 9);
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }
        }
    }
}
