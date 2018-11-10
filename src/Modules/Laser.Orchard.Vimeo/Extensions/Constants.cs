
namespace Laser.Orchard.Vimeo.Extensions {
    public static class Constants {
        public const string LocalArea = "Laser.Orchard.Vimeo";
        
        //strings for the tasks
        public const string TaskTypeBase = "Laser.Orchard.Vimeo.Task";
        public const string TaskSubtypeInProgress = ".UploadsInProgress";
        public const string TaskSubtypeComplete = ".CompleteUploads";

        //the following strings have to do with the Vimeo API
        public const string HeaderAcceptName = "Accept";
        public const string HeaderAcceptValue = "application/vnd.vimeo.*+json;version=3.2";
        public const string HeaderAuthorizationName = "Authorization"; 
        public const string HeaderAuthorizationValue = "Bearer "; //add the Access Token after this

        //optimize api calls
        public const double MaxDelayBetweenVerifications = 100; //minutes
        public const double MaxDelaySeconds = 60 * MaxDelayBetweenVerifications;
        public const double MinDelayBetweenVerifications = 1; //minutes
        public const double MinDelaySeconds = 60 * MinDelayBetweenVerifications;
        public const double SecToMinMultiplier = 1.0 / 60.0;
        public const double MinDelayBetweenTerminations = 1; //minutes
        public const double HoursToRefreshURLs = 12; //duration of cache for video URLs
        public const int SecondsInAnHour = 3600;

        //string for events
        public const string BaseSignalName = "LaserVimeo";
        public const string PublishedSignalName = BaseSignalName + "Published";
    }

    public static class VimeoEndpoints {
        public const string APIEntry = "https://api.vimeo.com";
        public const string Me = APIEntry + "/me";
        public const string Authorize = APIEntry + "/oauth/authorize";
        public const string MyAlbums = Me + "/albums";
        public const string MyGroups = Me + "/groups";
        public const string MyChannels = Me + "/channels";
        public const string VideoUpload = Me + "/videos";
        public const string Groups = APIEntry + "/groups";
        public const string Channels = APIEntry + "/channels";
        public const string PlayerEntry = "https://player.vimeo.com";
    }
}