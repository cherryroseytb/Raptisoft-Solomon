using UnityEngine;

namespace SolomonCopy.Meta
{
    // 캐릭터 선택 UI에서 해금 여부를 조회하기 위한 서비스.
    public class CharacterSelectionService : MonoBehaviour
    {
        public bool IsUnlocked(string characterId)
        {
            if (MetaProgressionManager.Instance == null) return false;
            switch (characterId)
            {
                // 기본 4캐릭터
                case "Sirmin":
                case "Lucritius":
                case "Aliss":
                case "Morth":
                    return true;
                // Fate 해금 캐릭터
                case "Griselda":
                    return MetaProgressionManager.Instance.IsCharacterUnlocked(LobbyFeatureId.Griselda);
                case "Wegnus":
                    return MetaProgressionManager.Instance.IsCharacterUnlocked(LobbyFeatureId.Wegnus);
                case "Vorpus":
                    return MetaProgressionManager.Instance.IsCharacterUnlocked(LobbyFeatureId.Vorpus);
                case "Wazoo":
                    return MetaProgressionManager.Instance.IsCharacterUnlocked(LobbyFeatureId.Wazoo);
                case "Athicus":
                    return MetaProgressionManager.Instance.IsCharacterUnlocked(LobbyFeatureId.Athicus);
                case "Andra":
                    return MetaProgressionManager.Instance.IsCharacterUnlocked(LobbyFeatureId.Andra);
                default:
                    return false;
            }
        }
    }
}
