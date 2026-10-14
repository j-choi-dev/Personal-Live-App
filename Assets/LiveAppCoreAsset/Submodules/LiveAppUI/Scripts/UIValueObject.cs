using System;

namespace LiveAppUI
{
    public enum ResourceType
    {
        None = 0,
        Character,
        BackGround,
        Prop,
    }

    public enum NoticePurpose
    {
        None,
        InitialOAuth,
        ResourceRetry,
        ResourceReauthorize
    }

    public enum ServerType
    {
        None = 0,
        Develop,
        Staging,
        Master,
    }
    public enum ResourceListFailureKind
    {
        MissingConfiguration,
        SheetOrTabNotFound,
        Unauthorized,
        PermissionDenied,
        Network,
        Other,
        ConfigurationInitializationFailed,
        AuthorizationCancelled
    }

    public sealed class ResourceListFailureData
    {
        public ResourceListFailureKind Kind { get; }
        public ResourceType ResourceType { get; }
        public ServerType ServerType { get; }
        public long StatusCode { get; }

        public ResourceListFailureData(
            ResourceListFailureKind kind,
            ResourceType resourceType,
            ServerType serverType,
            long statusCode = 0)
        {
            Kind = kind;
            ResourceType = resourceType;
            ServerType = serverType;
            StatusCode = statusCode;
        }
    }

    public class NoticeViewMessageData
    {
        public string HeaderTitle { get; private set; } = "NOTICE";
        public string MainMessage { get; private set; }
        public string SubMessage { get; private set; }

        public NoticeViewMessageData(string headerTitle, string mainMessagem, string subMessage) 
        {
            HeaderTitle = headerTitle;
            MainMessage = mainMessagem;
            SubMessage = subMessage;
        }

        public NoticeViewMessageData(string mainMessagem, string subMessage)
        {
            MainMessage = mainMessagem;
            SubMessage = subMessage;
        }

        public NoticeViewMessageData()
        {
            MainMessage = string.Empty;
            SubMessage = string.Empty;
        }

        public void SetHeaderTitle(string msg)
            => HeaderTitle = msg;

        public void SetMainMessage(string msg)
            => MainMessage = msg;

        public void SetSubMessage(string msg)
            =>  SubMessage = msg;
    }
}