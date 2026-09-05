using UnityEngine;

public class MetaProgressApplicator : MonoBehaviour
{
    [SerializeField] AbilityData[] allAbilities;
    [SerializeField] PlayerStats playerStats;
    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] PlayerController playerController;
    [SerializeField] SpriteRenderer playerRenderer;

    int _baseDashCount;

    void Start()
    {
        _baseDashCount = playerController != null ? playerController.MaxDashCount : 2;
        Apply();
    }

    public void Reapply()
    {
        if (playerController != null)
        {
            playerController.MaxDashCount = _baseDashCount;
            playerController.CanDoubleJump = false;
        }
        Apply();
    }

    void Apply()
    {
        if (MetaProgress.Instance == null || allAbilities == null) return;

        float totalDamage = 0, totalMaxHp = 0, totalSpeed = 0;
        float totalCrit = 0, totalCritDmg = 0;
        int totalMilestoneDash = 0;
        float shopDiscount = 0f;

        foreach (var ability in allAbilities)
        {
            int level = MetaProgress.Instance.GetAbilityLevel(ability.abilityId);
            if (level <= 0) continue;
            totalDamage  += ability.damagePerLevel     * level;
            totalMaxHp   += ability.maxHpPerLevel      * level;
            totalSpeed   += ability.speedPerLevel      * level;
            totalCrit    += ability.critChancePerLevel * level;
            totalCritDmg += ability.critDamagePerLevel * level;

            if (level >= 5)
            {
                if (ability.milestone5DoubleJump && playerController != null)
                    playerController.CanDoubleJump = true;
                totalMilestoneDash += ability.milestone5DashBonus;
                shopDiscount = Mathf.Max(shopDiscount, ability.milestone5ShopDiscount);
            }
            if (level >= 20)
                totalMilestoneDash += ability.milestone20DashBonus;
        }

        MetaProgress.Instance.SetShopDiscount(shopDiscount);
        if (totalMilestoneDash > 0 && playerController != null)
            playerController.MaxDashCount += totalMilestoneDash;

        var charData = CharacterManager.Selected;
        if (charData != null)
        {
            if (playerRenderer != null && charData.animData != null)
            {
                var anim = playerRenderer.GetComponent<SpriteAnimator>()
                        ?? playerRenderer.GetComponentInParent<SpriteAnimator>();
                anim?.SetAnimData(charData.animData);
            }
            totalDamage += charData.bonusDamage;
            totalMaxHp  += charData.bonusMaxHp;
            totalSpeed  += charData.bonusMoveSpeed;
            if (charData.bonusDashCount > 0 && playerController != null)
                playerController.MaxDashCount += charData.bonusDashCount;
        }

        playerStats?.ApplyMetaBonuses(totalMaxHp);
        playerCombat?.ApplyMetaBonuses(totalDamage, totalCrit, totalCritDmg);
        playerController?.ApplySpeedBonus(totalSpeed);
    }
}
