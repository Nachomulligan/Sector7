using System.Text;
using UnityEngine;

public static class RewardPresentationUtility
{
    public static string KindName(GachaRewardKind kind)
    {
        switch (kind)
        {
            case GachaRewardKind.Mecha: return "MECHA";
            case GachaRewardKind.Weapon: return "ARMA";
            case GachaRewardKind.Ability: return "HABILIDAD";
            case GachaRewardKind.Companion: return "APOYO";
            default: return kind.ToString().ToUpperInvariant();
        }
    }

    public static string RarityName(GachaRarity rarity)
    {
        switch (rarity)
        {
            case GachaRarity.Common: return "COMÚN";
            case GachaRarity.Rare: return "RARO";
            case GachaRarity.Epic: return "ÉPICO";
            case GachaRarity.Legendary: return "LEGENDARIO";
            default: return rarity.ToString().ToUpperInvariant();
        }
    }

    public static string BuildStats(GachaRewardSO reward, int level, int previousLevel = -1)
    {
        if (reward == null) return string.Empty;

        level = Mathf.Max(1, level);
        float multiplier = reward.LevelMultiplier(level);
        float previousMultiplier = previousLevel > 0 ? reward.LevelMultiplier(previousLevel) : -1f;

        switch (reward.Kind)
        {
            case GachaRewardKind.Mecha:
                return BuildMechaStats(reward, multiplier, previousMultiplier);
            case GachaRewardKind.Weapon:
                return BuildWeaponStats(reward, multiplier, previousMultiplier);
            case GachaRewardKind.Ability:
                return BuildAbilityStats(reward, multiplier, previousMultiplier);
            case GachaRewardKind.Companion:
                return reward.Companion != null && reward.Companion.Modifier != null
                    ? reward.Companion.Modifier.GetStatsDescription(multiplier, previousMultiplier)
                    : "Sin modificadores pasivos";
            default:
                return string.Empty;
        }
    }

    private static string BuildMechaStats(GachaRewardSO reward, float current, float previous)
    {
        if (reward.Mecha == null) return "Sin datos de mecha";
        int health = Mathf.RoundToInt(reward.Mecha.BaseMaxHealth * current);
        int damage = Mathf.RoundToInt(reward.Mecha.BaseDamage * current);
        StringBuilder builder = new StringBuilder();
        builder.Append("VIDA  ").Append(health);
        AppendDelta(builder, health, previous < 0f ? health : Mathf.RoundToInt(reward.Mecha.BaseMaxHealth * previous));
        builder.AppendLine();
        builder.Append("DAÑO  ").Append(damage);
        AppendDelta(builder, damage, previous < 0f ? damage : Mathf.RoundToInt(reward.Mecha.BaseDamage * previous));
        return builder.ToString();
    }

    private static string BuildWeaponStats(GachaRewardSO reward, float current, float previous)
    {
        if (reward.Weapon == null) return "Sin datos de arma";
        float interval = reward.Weapon.Cooldown / Mathf.Max(0.05f, current);
        float cadence = interval > 0f ? 1f / interval : 0f;
        float oldInterval = previous < 0f ? interval : reward.Weapon.Cooldown / Mathf.Max(0.05f, previous);
        float oldCadence = oldInterval > 0f ? 1f / oldInterval : 0f;

        StringBuilder builder = new StringBuilder();
        builder.Append($"CADENCIA  {cadence:0.##}/s");
        AppendDelta(builder, cadence, oldCadence);
        builder.AppendLine();
        builder.Append($"INTERVALO  {interval:0.##} s");
        if (previous >= 0f && interval < oldInterval)
            builder.Append($"  <color=#65F28A>({interval - oldInterval:0.##} s)</color>");
        builder.AppendLine().Append($"VELOCIDAD  {reward.Weapon.ProjectileSpeed:0.##}");
        return builder.ToString();
    }

    private static string BuildAbilityStats(GachaRewardSO reward, float current, float previous)
    {
        if (reward.Ability == null) return "Sin datos de habilidad";
        float cooldown = reward.Ability.Cooldown / Mathf.Max(0.05f, current);
        float oldCooldown = previous < 0f ? cooldown : reward.Ability.Cooldown / Mathf.Max(0.05f, previous);
        StringBuilder builder = new StringBuilder($"COOLDOWN  {cooldown:0.##} s");
        if (previous >= 0f && cooldown < oldCooldown)
            builder.Append($"  <color=#65F28A>({cooldown - oldCooldown:0.##} s)</color>");

        if (reward.Ability is BombStrategy bomb)
            builder.AppendLine().Append($"DAÑO  {bomb.Damage}\nRADIO  {bomb.BlastRadius:0.##}");
        else if (reward.Ability is ShieldStrategy shield)
            builder.AppendLine().Append($"DURACIÓN  {shield.Duration:0.##} s");
        else if (reward.Ability is HealStrategy heal)
            builder.AppendLine().Append($"CURACIÓN  {heal.HealAmount}");
        return builder.ToString();
    }

    private static void AppendDelta(StringBuilder builder, int current, int previous)
    {
        int delta = current - previous;
        if (delta > 0) builder.Append($"  <color=#65F28A>(+{delta})</color>");
    }

    private static void AppendDelta(StringBuilder builder, float current, float previous)
    {
        float delta = current - previous;
        if (delta > 0.0001f) builder.Append($"  <color=#65F28A>(+{delta:0.##})</color>");
    }
}
