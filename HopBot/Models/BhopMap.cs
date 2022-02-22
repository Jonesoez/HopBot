using System;
using System.ComponentModel.DataAnnotations;

namespace HopBot.Models
{
    public class BhopMap
    {
        //mapname is the key, gamebanana does not decide our fate.
        [Key]
        public string MapName { get;set; }
        public int MapId { get;set; }
        public string MapCreatorAvatar { get; set; }
        public string MapCreator { get;set; }
        public string MapImage { get; set; }
        public string MapFile { get; set; }

        public string MapDownloadLink { get;set; }
        public DateTime MapUploadDate { get;set; }
        public string RequestedBy { get;set; }
        public DateTime RequestedDate { get;set; }
    }
}
