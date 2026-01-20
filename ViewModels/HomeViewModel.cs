using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using UsFrameApp.Models;
using UsFrameApp.Services;
using UsFrameApp.View;

namespace UsFrameApp.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {
        // api calls for room creation
        private readonly ApiService _api;

        // service provider for page resolution
        private readonly IServiceProvider _serviceProvider;

        // holds newly created room
        public RoomModel? CreatedRoom { get; private set; }

        // static ui timer value
        public string StaticTimer { get; } = "00:03:27";

        public HomeViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // api service instance
            _api = new ApiService();
        }

        // starts a new video session
        [RelayCommand]
        public async Task StartSessionAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                // create room on backend
                CreatedRoom = await _api.CreateRoomAsync();

                if (CreatedRoom == null)
                    return;

                // navigate to connecting page
                var connectingPage = new ConnectingPage(CreatedRoom.Key);
                await Application.Current.MainPage.Navigation.PushAsync(connectingPage);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
