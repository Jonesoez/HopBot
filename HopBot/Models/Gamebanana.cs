namespace HopBot.Models
{
    public class Gamebanana
    {
        // disgusting fucking json schema by gamebanana, just why
        public class Data
        {
            public int _idRow { get; set; }
            public string _sName { get; set; }
            public _Apreviewmedia[] _aPreviewMedia { get; set; }
            public _Acategory _aCategory { get; set; }
            public _Agame _aGame { get; set; }
            public _Asubmitter _aSubmitter { get; set; }
            public _Afiles[] _aFiles { get; set; }
        }

        public class _Acategory
        {
            public int _idRow { get; set; }
            public string _sName { get; set; }
            public string _sModelName { get; set; }
            public string _sProfileUrl { get; set; }
            public string _sIconUrl { get; set; }
        }

        public class _Agame
        {
            public int _idRow { get; set; }
            public string _sName { get; set; }
            public string _sAbbreviation { get; set; }
            public string _sProfileUrl { get; set; }
            public string _sIconUrl { get; set; }
            public string _sBannerUrl { get; set; }
            public int _nSubscriberCount { get; set; }
            public bool _bHasSubmissionQueue { get; set; }
        }

        public class _Asubmitter
        {
            public int _idRow { get; set; }
            public string _sName { get; set; }
            public string _sUserTitle { get; set; }
            public string _sHonoraryTitle { get; set; }
            public int _tsJoinDate { get; set; }
            public string _sAvatarUrl { get; set; }
            public string _sSigUrl { get; set; }
            public string _sProfileUrl { get; set; }
            public string _sUpicUrl { get; set; }
            public string _sPointsUrl { get; set; }
            public string _sMedalsUrl { get; set; }
            public bool _bIsOnline { get; set; }
            public string _sLocation { get; set; }
            public string _sOnlineTitle { get; set; }
            public string _sOfflineTitle { get; set; }
            public int _nPoints { get; set; }
            public int _nPointsRank { get; set; }
            public object[][] _aNormalMedals { get; set; }
            public object[][] _aRareMedals { get; set; }
            public object[] _aLegendaryMedals { get; set; }
            public object[] _aClearanceLevels { get; set; }
            public bool _bHasRipe { get; set; }
            public string _sSubjectShaperCssCode { get; set; }
            public string _sCooltipCssCode { get; set; }
            public int _nBuddyCount { get; set; }
            public int _nSubscriberCount { get; set; }
            public object[] _aDonationMethods { get; set; }
            public bool _bAccessorIsBuddy { get; set; }
            public bool _bBuddyRequestExistsWithAccessor { get; set; }
            public bool _bAccessorIsSubscribed { get; set; }
            public string _sHdAvatarUrl { get; set; }
        }

        public class _Apreviewmedia
        {
            public string _sType { get; set; }
            public string _sBaseUrl { get; set; }
            public string _sFile { get; set; }
            public string _sFile530 { get; set; }
            public string _sFile100 { get; set; }
            public string _sFile220 { get; set; }
        }

        public class _Afiles
        {
            public string _idRow { get; set; }
            public string _sFile { get; set; }
            public int _nFilesize { get; set; }
            public bool _bIsMissing { get; set; }
            public string _sDescription { get; set; }
            public int _tsDateAdded { get; set; }
            public int _nDownloadCount { get; set; }
            public string _sAnalysisCode { get; set; }
            public string _sAnalysisResult { get; set; }
            public _Ametadata _aMetadata { get; set; }
            public bool _bContainsExe { get; set; }
            public string _sDownloadUrl { get; set; }
        }

        public class _Ametadata
        {
            public string _sMimeType { get; set; }
            public string[] _aArchiveFileTree { get; set; }
        }

    }
}
