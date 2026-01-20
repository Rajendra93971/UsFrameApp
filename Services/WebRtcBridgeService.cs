using System;
using System.Text.Json;
using Microsoft.Maui.ApplicationModel;

namespace UsFrameApp.Services
{
    public class WebRtcBridgeService
    {
        // raised when offer is received from js
        public event Action<string, string>? OnOfferReceived;

        // raised when answer is received from js
        public event Action<string, string>? OnAnswerReceived;

        // raised when ice candidate is received from js
        public event Action<string>? OnIceCandidateReceived;

        // handles messages coming from webview javascript
        public void HandleJsMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                System.Diagnostics.Debug.WriteLine("[WebRTC] empty js message");
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(message);
                var root = doc.RootElement;

                // message type (offer / answer / ice)
                if (!root.TryGetProperty("type", out var typeElem))
                {
                    System.Diagnostics.Debug.WriteLine("[WebRTC] missing type");
                    return;
                }

                var type = typeElem.GetString();
                if (string.IsNullOrEmpty(type)) return;

                switch (type)
                {
                    case "offer":
                        // incoming webrtc offer
                        if (root.TryGetProperty("sdp", out var offerSdp) &&
                            root.TryGetProperty("room", out var offerRoom))
                        {
                            SafeInvoke(
                                OnOfferReceived,
                                offerSdp.GetString() ?? string.Empty,
                                offerRoom.GetString() ?? string.Empty
                            );
                        }
                        break;

                    case "answer":
                        // incoming webrtc answer
                        if (root.TryGetProperty("sdp", out var answerSdp) &&
                            root.TryGetProperty("room", out var answerRoom))
                        {
                            SafeInvoke(
                                OnAnswerReceived,
                                answerSdp.GetString() ?? string.Empty,
                                answerRoom.GetString() ?? string.Empty
                            );
                        }
                        break;

                    case "ice":
                        // incoming ice candidate
                        if (root.TryGetProperty("candidate", out var cand))
                        {
                            SafeInvoke(
                                OnIceCandidateReceived,
                                cand.GetString() ?? string.Empty
                            );
                        }
                        break;

                    default:
                        System.Diagnostics.Debug.WriteLine($"[WebRTC] unknown type '{type}'");
                        break;
                }
            }
            catch (JsonException jex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebRTC] json parse error: {jex}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebRTC] handler error: {ex}");
            }
        }

        // safely invokes event on ui thread (two params)
        private static void SafeInvoke(Action<string, string>? ev, string a, string b)
        {
            if (ev == null) return;

            foreach (Delegate d in ev.GetInvocationList())
            {
                var del = (Action<string, string>)d;

                try
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try { del(a, b); }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebRTC] subscriber error: {ex}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebRTC] dispatch error: {ex}");
                }
            }
        }

        // safely invokes event on ui thread (single param)
        private static void SafeInvoke(Action<string>? ev, string a)
        {
            if (ev == null) return;

            foreach (Delegate d in ev.GetInvocationList())
            {
                var del = (Action<string>)d;

                try
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        try { del(a); }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[WebRTC] subscriber error: {ex}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebRTC] dispatch error: {ex}");
                }
            }
        }
    }
}
