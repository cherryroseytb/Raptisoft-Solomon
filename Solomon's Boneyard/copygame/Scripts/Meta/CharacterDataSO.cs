using UnityEngine;
using SolomonCopy.Magic;

namespace SolomonCopy.Meta
{
    [CreateAssetMenu(menuName = "SolomonCopy/Meta/CharacterData", fileName = "CharacterData")]
    public class CharacterDataSO : ScriptableObject
    {
        public string characterId; 
        public string displayName;
        public Sprite portrait;
        public GameObject playerPrefab; 

        [Header("Starting Skills")]
        public BaseMagicId startingPrimary = BaseMagicId.Fire;
        public BaseMagicId startingSecondary = BaseMagicId.Fire;

        [Header("Starting Stats")]
        public float moveSpeed = 5f;
        public int maxHp = 10;
    }
}
