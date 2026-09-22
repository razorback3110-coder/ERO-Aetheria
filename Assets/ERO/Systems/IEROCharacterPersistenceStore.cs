using ERO.Data;

namespace ERO.Systems
{
    /// <summary>
    /// Persistence boundary for character state. The live MMO server can inject an
    /// authoritative implementation while offline/client builds may use the local
    /// checkpoint adapter.
    /// </summary>
    public interface IEROCharacterPersistenceStore
    {
        bool Save(CharacterData character);
        bool TryLoad(string characterId, out CharacterData character);
        bool Delete(string characterId);
    }
}
