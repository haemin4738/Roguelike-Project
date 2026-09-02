using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float jumpForce = 14f;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] int maxDashCount = 2;
    [SerializeField] float dashRechargeTime = 0.75f;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.1f;
    [SerializeField] LayerMask groundLayer;

    const float CoyoteTime = 0.15f;
    const float JumpBufferTime = 0.1f;

    Rigidbody2D _rb;
    Collider2D _playerCollider;
    Collider2D _groundCollider;
    bool _isGrounded;
    bool _isDashing;
    int _dashCount;
    float _dashRechargeTimer;
    float _coyoteTimer;
    float _jumpBufferTimer;
    float _facingDir = 1f;

    // 어빌리티 시스템 연동 (신속 5레벨: 이단점프, 신속 20레벨: 대시횟수+1)
    public int MaxDashCount { get => maxDashCount; set => maxDashCount = value; }
    public bool CanDoubleJump { get; set; } = false;
    bool _usedDoubleJump;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<Collider2D>();
        _dashCount = maxDashCount;

        // 벽 마찰 제거 — 마찰력이 있으면 벽에 붙어 공중에 멈추는 현상 발생
        _rb.sharedMaterial = new PhysicsMaterial2D { friction = 0f, bounciness = 0f };
    }

    void Update()
    {
        CheckGround();
        UpdateTimers();
        HandleJumpInput();
        HandleDashInput();
    }

    void FixedUpdate()
    {
        if (_isDashing) return;
        HandleMovement();
    }

    void CheckGround()
    {
        bool wasGrounded = _isGrounded;
        _groundCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        _isGrounded = _groundCollider != null;

        if (_isGrounded && !wasGrounded)
            _usedDoubleJump = false;
    }

    void UpdateTimers()
    {
        _coyoteTimer = _isGrounded ? CoyoteTime : _coyoteTimer - Time.deltaTime;
        if (_jumpBufferTimer > 0f) _jumpBufferTimer -= Time.deltaTime;

        if (_dashCount < maxDashCount)
        {
            _dashRechargeTimer += Time.deltaTime;
            if (_dashRechargeTimer >= dashRechargeTime)
            {
                _dashCount++;
                _dashRechargeTimer = 0f;
            }
        }
    }

    void HandleJumpInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        bool jumpPressed = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
        bool downHeld = kb.sKey.isPressed || kb.downArrowKey.isPressed;

        // 하향 점프 (S + 점프키)
        if (jumpPressed && downHeld && _isGrounded)
        {
            StartCoroutine(DropRoutine());
            return;
        }

        if (jumpPressed)
            _jumpBufferTimer = JumpBufferTime;

        if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
        {
            Jump();
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            return;
        }

        // 이단점프 (신속 5레벨 해금)
        if (jumpPressed && CanDoubleJump && !_isGrounded && !_usedDoubleJump)
        {
            Jump();
            _usedDoubleJump = true;
        }
    }

    void Jump() => _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);

    void HandleDashInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        bool dashPressed = kb.leftShiftKey.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame;
        if (dashPressed && _dashCount > 0 && !_isDashing)
            StartCoroutine(DashRoutine());
    }

    IEnumerator DropRoutine()
    {
        var target = _groundCollider;
        if (target == null || target.GetComponent<PlatformEffector2D>() == null) yield break;

        Physics2D.IgnoreCollision(_playerCollider, target, true);
        yield return new WaitForSeconds(0.25f);
        if (target != null)
            Physics2D.IgnoreCollision(_playerCollider, target, false);
    }

    IEnumerator DashRoutine()
    {
        _dashCount--;
        _isDashing = true;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = ((mouseWorld - (Vector2)transform.position).normalized);

        float originalGravity = _rb.gravityScale;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        _rb.gravityScale = originalGravity;
        _isDashing = false;
    }

    void HandleMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float horizontal = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;

        _rb.linearVelocity = new Vector2(horizontal * moveSpeed, _rb.linearVelocity.y);

        if (horizontal != 0f)
            _facingDir = horizontal;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
