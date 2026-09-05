using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon Slots")]
    [SerializeField] WeaponData[] weaponSlots = new WeaponData[2];

    [Header("References")]
    [SerializeField] Transform weaponPivot;
    [SerializeField] SpriteRenderer weaponSpriteRenderer;
    [SerializeField] SpriteRenderer bodySpriteRenderer;
    [SerializeField] Transform projectileSpawnPoint;

    [Header("Melee VFX")]
    [SerializeField] float meleeSwingAngle = 80f;

    int _currentSlot = 0;
    float _attackCooldown;
    bool _isAttacking;
    float _damageBonus, _critChance, _critDamageBonus;

    Camera _cam;

    public void ApplyMetaBonuses(float damage, float critChance, float critDmg)
    {
        _damageBonus = damage;
        _critChance = critChance;
        _critDamageBonus = critDmg;
    }

    float FinalDamage(float baseDamage)
    {
        float dmg = baseDamage + _damageBonus;
        if (_critChance > 0f && UnityEngine.Random.value < _critChance / 100f)
            dmg *= 1f + _critDamageBonus / 100f;
        return dmg;
    }

    WeaponData CurrentWeapon => weaponSlots[_currentSlot];

    void Awake() => _cam = Camera.main;

    void Start() => UpdateWeaponSprite();

    void Update()
    {
        _attackCooldown -= Time.deltaTime;

        RotateWeaponTowardMouse();
        FlipCharacterTowardMouse();
        HandleSlotSwitch();
        HandleAttackInput();
    }

    void RotateWeaponTowardMouse()
    {
        if (_cam == null) return;
        Vector2 mouseWorld = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mouseWorld - (Vector2)weaponPivot.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void FlipCharacterTowardMouse()
    {
        if (_cam == null) return;
        Vector2 mouseWorld = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        bool facingRight = mouseWorld.x >= transform.position.x;
        bodySpriteRenderer.flipX = !facingRight;
        weaponSpriteRenderer.flipY = !facingRight;

        float offset = CurrentWeapon != null ? CurrentWeapon.spriteRotationOffset : 0f;
        weaponSpriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, offset * (facingRight ? 1f : -1f));
    }

    void HandleSlotSwitch()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) SwitchSlot(0);
        if (kb.digit2Key.wasPressedThisFrame) SwitchSlot(1);
    }

    void SwitchSlot(int slot)
    {
        _currentSlot = slot;
        UpdateWeaponSprite();
    }

    public WeaponData GetWeapon(int slot) => weaponSlots[Mathf.Clamp(slot, 0, 1)];

    public void EquipWeapon(WeaponData data)
    {
        weaponSlots[_currentSlot] = data;
        UpdateWeaponSprite();
    }

    public void EquipWeapon(WeaponData data, int slot)
    {
        weaponSlots[Mathf.Clamp(slot, 0, 1)] = data;
        UpdateWeaponSprite();
    }

    void UpdateWeaponSprite()
    {
        if (weaponSpriteRenderer == null) return;
        weaponSpriteRenderer.sprite = CurrentWeapon != null ? CurrentWeapon.sprite : null;
        float offset = CurrentWeapon != null ? CurrentWeapon.spriteRotationOffset : 0f;
        weaponSpriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, offset);
    }

    void HandleAttackInput()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (_attackCooldown > 0f || _isAttacking || CurrentWeapon == null) return;

        _attackCooldown = 1f / CurrentWeapon.attackSpeed;

        if (CurrentWeapon.weaponType == WeaponType.Melee)
            StartCoroutine(MeleeAttackRoutine());
        else
            RangedAttack();
    }

    IEnumerator MeleeAttackRoutine()
    {
        _isAttacking = true;

        float startAngle = weaponPivot.eulerAngles.z + meleeSwingAngle * 0.5f;
        float endAngle = startAngle - meleeSwingAngle;
        float duration = 0.12f;
        float elapsed = 0f;
        var alreadyHit = new System.Collections.Generic.HashSet<GameObject>();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Lerp(startAngle, endAngle, elapsed / duration);
            weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);

            // 피벗~끝 전체 Capsule 체크 — 밀착 시 tip-only 원형이 빗나가는 문제 수정
            Vector2 capsuleCenter = (Vector2)weaponPivot.position + (Vector2)(weaponPivot.right * (CurrentWeapon.attackRange * 0.5f));
            var hits = Physics2D.OverlapCapsuleAll(
                capsuleCenter,
                new Vector2(CurrentWeapon.attackRange, 0.8f),
                CapsuleDirection2D.Horizontal,
                weaponPivot.eulerAngles.z,
                LayerMask.GetMask("Enemy"));
            foreach (var hit in hits)
            {
                if (alreadyHit.Add(hit.gameObject))
                    DamageSystem.Damage(hit.gameObject, FinalDamage(CurrentWeapon.damage));
            }

            yield return null;
        }

        _isAttacking = false;
    }

    void RangedAttack()
    {
        if (CurrentWeapon.projectilePrefab == null) return;

        Vector2 mouseWorld = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mouseWorld - (Vector2)projectileSpawnPoint.position).normalized;

        var go = Instantiate(CurrentWeapon.projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        var proj = go.GetComponent<ProjectileBase>();
        if (proj == null) { Destroy(go); return; }
        proj.Init(dir, CurrentWeapon.projectileSpeed, FinalDamage(CurrentWeapon.damage));
    }

    void OnDrawGizmosSelected()
    {
        if (CurrentWeapon == null || weaponPivot == null) return;
        if (CurrentWeapon.weaponType != WeaponType.Melee) return;
        Gizmos.color = Color.yellow;
        Vector2 hitPoint = (Vector2)weaponPivot.position + (Vector2)(weaponPivot.right * CurrentWeapon.attackRange);
        Gizmos.DrawWireSphere(hitPoint, 0.3f);
    }
}
