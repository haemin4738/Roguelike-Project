using UnityEngine;

public enum WeaponType { Melee, Ranged }

[CreateAssetMenu(fileName = "WeaponData", menuName = "Roguelike/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType weaponType;
    public Sprite sprite;

    [Header("Rotation")]
    public float spriteRotationOffset = 0f; // 스프라이트 각도 보정

    [Header("Stats")]
    public float damage = 10f;
    public float attackSpeed = 2f;   // 초당 공격 횟수
    public float attackRange = 1.5f; // 근접: 판정 반경 / 원거리: 사용 안 함

    [Header("Ranged")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
}
