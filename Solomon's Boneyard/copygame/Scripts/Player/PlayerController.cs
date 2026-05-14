// PlayerController.cs (모바일 듀얼 조이스틱 버전)
// Unity 에디터:
//   - Player GameObject에 Rigidbody2D(GravityScale=0, FreezeRotation Z) + CircleCollider2D 부착.
//   - Tag "Player".
//   - 자식으로 빈 Transform "Muzzle" 만들고 MagicCaster.muzzle에 연결.
//   - Canvas에 VirtualJoystick 2개 만들어 moveStick / aimStick 필드에 연결.
//   - MagicCaster 같은 오브젝트에 부착.
// 동작:
//   - 안드로이드/모바일: 왼쪽 조이스틱으로 이동, 오른쪽 조이스틱으로 조준+자동연사.
//   - 에디터/스탠드얼론: WASD 이동, 마우스 위치 조준, 좌클릭(또는 우조이스틱 미사용 시) 발사.

using UnityEngine;
using SolomonCopy.UI;
using SolomonCopy.Magic; // 참조 통일성용

namespace SolomonCopy.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("이동")]
        public float moveSpeed = 5f;

        [Header("UI 입력")]
        public VirtualJoystick moveStick;
        public VirtualJoystick aimStick;

        [Header("발사")]
        public float aimThreshold = 0.2f;   // 우 조이스틱 데드존 임계값

        private Rigidbody2D _rb;
        private MagicCaster _caster;
        private Vector2 _moveInput;
        private Vector2 _aimDir;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _caster = GetComponent<MagicCaster>();
        }

        private void Update()
        {
            ReadInput();
            ApplyAimRotation();
            TryFire();
        }

        private void FixedUpdate()
        {
            // 대각선 이동 정규화
            Vector2 move = _moveInput.sqrMagnitude > 1f ? _moveInput.normalized : _moveInput;
            _rb.MovePosition(_rb.position + move * moveSpeed * Time.fixedDeltaTime);
        }

        private void ReadInput()
        {
            // 1) 이동 입력
            if (moveStick != null && moveStick.IsPressed)
            {
                _moveInput = moveStick.Direction;
            }
            else
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                _moveInput = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical"));
#else
                _moveInput = Vector2.zero;
#endif
            }

            // 2) 조준 입력
            if (aimStick != null && aimStick.IsPressed)
            {
                _aimDir = aimStick.Direction;
            }
            else
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                // 마우스 폴백: 마우스 위치를 향한 방향
                Vector3 mouseWorld = Camera.main != null
                    ? Camera.main.ScreenToWorldPoint(Input.mousePosition)
                    : Vector3.zero;
                Vector2 toMouse = (Vector2)mouseWorld - _rb.position;
                // 좌클릭 시에만 발사 의도가 있다고 봄
                _aimDir = Input.GetMouseButton(0) ? toMouse.normalized : Vector2.zero;
#else
                _aimDir = Vector2.zero;
#endif
            }
        }

        private void ApplyAimRotation()
        {
            if (_aimDir.sqrMagnitude < aimThreshold * aimThreshold) return;
            // 기존 회전 로직 유지: 스프라이트가 위쪽을 보고 있다고 가정
            float angle = Mathf.Atan2(_aimDir.y, _aimDir.x) * Mathf.Rad2Deg;
            _rb.rotation = angle - 90f;
        }

        private void TryFire()
        {
            if (_caster == null) return;
            if (_aimDir.sqrMagnitude < aimThreshold * aimThreshold) return;
            _caster.TryFire(_aimDir);
        }
    }
}
