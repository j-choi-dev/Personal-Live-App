using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace LiveAppUI.Model
{
    public interface IResourceListModel
    {
        UniTask<IReadOnlyList<string>> GetCharacterList();
    }
}
