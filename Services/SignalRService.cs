using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Maui.ApplicationModel;
using UsFrameApp.Constants;

namespace UsFrameApp.Services
{
    public class SignalRService
    {
        private HubConnection? _connection;

        // connection state events
        public event Action? OnTwoUsersConnected;
        public event Action? OnRoomClosed;

        // webrtc signaling events
        public event Action<string>? OnOfferReceived;
        public event Action<string>? OnAnswerReceived;
        public event Action<string>? OnIceCandidateReceived;

        // connects to signalr hub and joins room
        public async Task ConnectAsync(string roomKey, string userId)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl($"{ApiEndpoints.BaseUrl}/meeting")
                .WithAutomaticReconnect()
                .Build();

            RegisterListeners();

            await _connection.StartAsync();

            await _connection.InvokeAsync(
                SignalREvents.JoinRoom,
                roomKey,
                userId
            );
        }

        // registers all hub event listeners
        private void RegisterListeners()
        {
            _connection!.On(SignalREvents.TwoUsersConnected, () =>
            {
                SafeInvoke(OnTwoUsersConnected);
            });

            _connection.On(SignalREvents.RoomClosed, () =>
            {
                SafeInvoke(OnRoomClosed);
            });

            _connection.On<string>(SignalREvents.ReceiveOffer, offer =>
            {
                SafeInvoke(OnOfferReceived, offer);
            });

            _connection.On<string>(SignalREvents.ReceiveAnswer, answer =>
            {
                SafeInvoke(OnAnswerReceived, answer);
            });

            _connection.On<string>(SignalREvents.ReceiveIceCandidate, ice =>
            {
                SafeInvoke(OnIceCandidateReceived, ice);
            });
        }

        // sends webrtc offer to server
        public async Task SendOfferAsync(string roomKey, string offer)
        {
            if (_connection == null) return;

            await _connection.InvokeAsync(
                SignalREvents.SendOffer,
                roomKey,
                offer
            );
        }

        // sends webrtc answer to server
        public async Task SendAnswerAsync(string roomKey, string answer)
        {
            if (_connection == null) return;

            await _connection.InvokeAsync(
                SignalREvents.SendAnswer,
                roomKey,
                answer
            );
        }

        // sends ice candidate to server
        public async Task SendIceCandidateAsync(string roomKey, string ice)
        {
            if (_connection == null) return;

            await _connection.InvokeAsync(
                SignalREvents.SendIceCandidate,
                roomKey,
                ice
            );
        }

        // notifies server when user leaves room
        public async Task LeaveRoomAsync(string roomKey, string userId)
        {
            if (_connection == null) return;

            await _connection.InvokeAsync(
                SignalREvents.LeaveRoom,
                roomKey,
                userId
            );
        }

        // stops signalr connection
        public async Task DisconnectAsync()
        {
            if (_connection != null)
                await _connection.StopAsync();
        }

        // safe ui thread invoke (no params)
        private static void SafeInvoke(Action? ev)
        {
            if (ev == null) return;

            foreach (Delegate d in ev.GetInvocationList())
            {
                var del = (Action)d;
                try
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try { del(); }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SignalR] handler error: {ex}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SignalR] dispatch error: {ex}");
                }
            }
        }

        // safe ui thread invoke (single param)
        private static void SafeInvoke<T>(Action<T>? ev, T arg)
        {
            if (ev == null) return;

            foreach (Delegate d in ev.GetInvocationList())
            {
                var del = (Action<T>)d;
                try
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try { del(arg); }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SignalR] handler error: {ex}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SignalR] dispatch error: {ex}");
                }
            }
        }
    }
}
