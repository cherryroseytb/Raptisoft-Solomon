// BaseMagicSO.cs
// 기본 마법(불/얼음/번개/대지) 데이터 정의.
// Unity 에디터: Project 창 우클릭 > Create > SolomonCopy > Magic > BaseMagic 으로 에셋 생성.
// 각 ID마다 1개씩 에셋 생성 후 MagicRegistrySO 에 등록.

using UnityEngine;

namespace SolomonCopy.Magic
{
    [CreateAssetMenu(menuName = "SolomonCopy/Magic/BaseMagic", fileName = "BaseMagic_New")]
    public class BaseMagicSO : ScriptableObject
    {
        [Header("식별")]
        public BaseMagicId id = BaseMagicId.None;
        public string displayName = "Unnamed";
        public Sprite icon;

        [Header("발사체")]
        public GameObject projectilePrefab;   // Projectile 컴포넌트가 붙은 프리팹
        public float speed = 10f;
        public float lifetime = 2f;
        public int damage = 10;
        public bool pierce = false;            // 관통 여부

        [Header("발사")]
        public float cooldown = 0.3f;          // 발사 간격(초)

        [Header("상태이상")]
        public StatusEffect statusEffect = StatusEffect.None;
        public float statusDuration = 1.5f;
    }
}
