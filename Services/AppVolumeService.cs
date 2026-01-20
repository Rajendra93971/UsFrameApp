namespace UsFrameApp.Services
{
    // app level volume controller (does not change system volume)
    public static class AppVolumeService
    {
        // volume range: 0.0 to 1.0
        private static double _volume = 1.0;

        // current app audio volume
        public static double Volume
        {
            get => _volume;
            set => _volume = Math.Clamp(value, 0.0, 1.0);
        }

        // true when volume is effectively muted
        public static bool IsMuted => _volume <= 0.01;
    }
}
