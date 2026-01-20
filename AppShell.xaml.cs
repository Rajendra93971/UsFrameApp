namespace UsFrameApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(View.RoomPage), typeof(View.RoomPage));
        }
    }
}
