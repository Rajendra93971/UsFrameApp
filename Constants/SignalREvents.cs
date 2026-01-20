namespace UsFrameApp.Constants
{
    // signalr hub event names
    public static class SignalREvents
    {
        // room lifecycle
        public const string JoinRoom = "JoinRoom";
        public const string LeaveRoom = "LeaveRoom";
        public const string TwoUsersConnected = "TwoUsersConnected";
        public const string RoomClosed = "RoomClosed";

        // webrtc signaling
        public const string SendOffer = "SendOffer";
        public const string ReceiveOffer = "ReceiveOffer";
        public const string SendAnswer = "SendAnswer";
        public const string ReceiveAnswer = "ReceiveAnswer";
        public const string SendIceCandidate = "SendIceCandidate";
        public const string ReceiveIceCandidate = "ReceiveIceCandidate";
    }
}
