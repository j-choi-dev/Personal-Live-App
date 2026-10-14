namespace LiveAppUI
{
    public enum ResourceType
    {
        None = 0,
        Character,
        BackGround,
        Prop,
    }

    public enum ServerType
    {
        None = 0,
        Develop,
        Staging,
        Master,
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