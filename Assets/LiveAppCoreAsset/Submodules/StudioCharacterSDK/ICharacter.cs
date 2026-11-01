namespace StudioCharacterSDK.Domain
{
    public interface ICharacter
    {
        string ID { get; } // TODO 리소스 오브젝트 공용 IF로 뺄 것 @Choi 26.07.28
        void SetID(string id);
        void Init();
    }
}
