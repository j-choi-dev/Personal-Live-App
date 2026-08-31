using System;

namespace StudioRendererSDK.Domain
{
    [Serializable]
    public sealed class YoutubeLivePrepareRequest
    {
        public string title;
        public int width;
        public int height;
        public string streamKey;
    }

    [Serializable]
    public sealed class YoutubeLiveStatusResponse
    {
        public bool success;
        public string state;
        public string message;
        public string broadcastId;
        public string utcTime;
    }
}