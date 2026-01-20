namespace UsFrameApp.Constants
{
    // api base and endpoint paths
    public static class ApiEndpoints
    {
        // backend base url
        public const string BaseUrl = "https://usframe.com";

        // create new room
        public const string CreateRoom = "/usframe/api/rooms";

        // join existing room
        public const string JoinRoom = "/usframe/api/rooms/{0}/join";

        // close active room
        public const string CloseRoom = "/usframe/api/rooms/{0}/close";
    }
}
