using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 키보드 입력 받기 (WASD 또는 방향키)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 대각선 이동 시 속도가 빨라지지 않게 정규화
        moveInput = moveInput.normalized;

        // 마우스 방향 바라보기
        LookAtMouse();
    }

    void LookAtMouse()
    {
        // 마우스의 화면 위치를 게임 세상 위치로 변환
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // 캐릭터에서 마우스를 향하는 방향 벡터 계산
        Vector2 lookDir = (Vector2)mousePos - rb.position;
        
        // 방향 벡터를 각도(degree)로 변환
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        
        // 캐릭터 회전 적용 (2D에서는 Z축 회전)
        rb.rotation = angle - 90f;
    }

    void FixedUpdate()
    {
        // 실제로 캐릭터 이동시키기
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}