using System;
using System.Threading.Tasks;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using UsFrameApp.Models;
using UsFrameApp.Services;

namespace UsFrameApp.ViewModels
{
    public partial class RoomViewModel : BaseViewModel
    {
        // handles realtime signaling
        private readonly SignalRService _signalR;

        // handles room api calls
        private readonly ApiService _api;

        // bridge between webview js and native
        private readonly WebRtcBridgeService _webrtc;

        // current room data
        public RoomModel? Room { get; private set; }

        // local user info
        public UserModel User { get; }

        // current room key
        public string? RoomKey { get; private set; }

        public RoomViewModel(
            SignalRService signalR,
            ApiService api,
            WebRtcBridgeService webrtc)
        {
            _signalR = signalR;
            _api = api;
            _webrtc = webrtc;

            // generate local user id
            User = new UserModel();
        }

        // entry point when room key is received
        public async Task InitializeAsync(string roomKey)
        {
            Debug.WriteLine($"[RoomVM] InitializeAsync called with roomKey='{roomKey}'");

            if (string.IsNullOrWhiteSpace(roomKey))
                throw new ArgumentException("roomKey must be provided", nameof(roomKey));

            IsBusy = true;

            RoomKey = roomKey.Trim();

            // fetch room details from backend
            Room = await _api.GetRoomByKeyAsync(RoomKey);

            if (Room == null)
            {
                IsBusy = false;
                throw new Exception("Room not found");
            }

            // connect signalr after room validation
            await InitializeAsync();

            IsBusy = false;
        }

        // connects to signalr hub
        public async Task InitializeAsync()
        {
            if (Room == null)
                throw new InvalidOperationException("Room is not initialized.");

            await _signalR.ConnectAsync(Room.Key, User.UserId);
        }

        // triggered when user leaves the room
        [RelayCommand]
        public async Task LeaveRoomAsync()
        {
            if (Room == null)
                return;

            try
            {
                await _signalR.LeaveRoomAsync(Room.Key, User.UserId);
                await _api.CloseRoomAsync(Room.Id);
            }
            catch
            {
                // ignore backend errors on leave
            }
            finally
            {
                await _signalR.DisconnectAsync();
            }
        }

    }
}
