using System;
using System.Collections.Generic;
using System.Text;
using Android.Webkit;

namespace UsFrameApp.Platforms.Android
{
    public class WebRtcWebChromeClient : WebChromeClient
    {
        public override void OnPermissionRequest(PermissionRequest request)
        {
            try
            {
                if (request == null)
                    return;

                request.Grant(request.GetResources());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebRTC][OnPermissionRequest] {ex}");
            }
        }
    }
}
