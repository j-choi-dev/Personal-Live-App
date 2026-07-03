using System.Collections.Generic;

namespace LiveAppUI.Model
{
    public interface IResourceListModel
    {
        IReadOnlyList<string> GetCharacterList();
    }
}
