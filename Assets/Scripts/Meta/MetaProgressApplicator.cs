using UnityEngine;

public class MetaProgressApplicator : MonoBehaviour
{
    [SerializeField] AbilityData[] allAbilities;
    [SerializeField] PlayerStats playerStats;
    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] PlayerController playerController;

    void Start()
    {
        if (MetaProgress.Instance == null || allAbilities == null) return;

        float totalDamage = 0, totalMaxHp = 0, totalSpeed = 0;
        float totalCrit = 0, totalCritDmg = 0;

        foreach (var ability in allAbilities)
        {
            int level = MetaProgress.Instance.GetAbilityLevel(ability.abilityId);
            if (level <= 0) continue;
            totalDamage   += ability.damagePerLevel      * level;
            totalMaxHp    += ability.maxHpPerLevel       * level;
            totalSpeed    += ability.speedPerLevel       * level;
            totalCrit     += ability.critChancePerLevel  * level;
            totalCritDmg  += ability.critDamagePerLevel  * level;
        }

        playerStats?.ApplyMetaBonuses(totalMaxHp);
        playerCombat?.ApplyMetaBonuses(totalDamage, totalCrit, totalCritDmg);
        playerController?.ApplySpeedBonus(totalSpeed);
    }
}
