using System;

namespace UsFrameApp.Models
{
    // represents local user in a room
    public class UserModel
    {
        // unique user identifier for signaling
        public string UserId { get; set; } = Guid.NewGuid().ToString();
    }
}
