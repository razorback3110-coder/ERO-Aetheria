using ERO.Data;
using UnityEngine;

namespace ERO.Systems
{
    /// <summary>
    /// Economy facade for gameplay/UI. Production transactions must be authorized by the server.
    /// Credits are the earnable standard currency; Cristaux ERO are premium and never convert back.
    /// </summary>
    public sealed class EconomySystem : MonoBehaviour
    {
        public const long CreditsPerCristalEro = EROEconomyRules.CreditsPerEroCrystal;

        public bool SpendCredits(CharacterData c, long amount)
        {
            if (c == null || amount < 0 || c.credits < amount)
                return false;
            c.credits -= amount;
            return true;
        }

        public bool SpendCristauxEro(CharacterData c, long amount)
        {
            if (c == null || amount < 0 || c.eroCrystals < amount)
                return false;
            c.eroCrystals -= amount;
            return true;
        }

        public void GrantCredits(CharacterData c, long amount)
        {
            if (c == null || amount <= 0)
                return;
            c.credits = SaturatingAdd(c.credits, amount);
        }

        public void GrantCristauxEro(CharacterData c, long amount)
        {
            if (c == null || amount <= 0)
                return;
            c.eroCrystals = SaturatingAdd(c.eroCrystals, amount);
        }

        public void GrantGuildTokens(CharacterData c, long amount) => GrantActivity(c, amount, 0);
        public void GrantArenaTokens(CharacterData c, long amount) => GrantActivity(c, amount, 1);
        public void GrantDungeonStones(CharacterData c, long amount) => GrantActivity(c, amount, 2);
        public void GrantMvpTokens(CharacterData c, long amount) => GrantActivity(c, amount, 3);
        public void GrantEventTokens(CharacterData c, long amount) => GrantActivity(c, amount, 4);

        // One-way conversion by design: Credits can never be converted into premium currency.
        public bool TryBuyCreditsWithCristauxEro(CharacterData c, long cristauxEro)
        {
            if (c == null || cristauxEro < 0 || c.eroCrystals < cristauxEro)
                return false;

            c.eroCrystals -= cristauxEro;
            GrantCredits(c, SaturatingMultiply(cristauxEro, CreditsPerCristalEro));
            return true;
        }

        private static void GrantActivity(CharacterData c, long amount, int currency)
        {
            if (c == null || amount <= 0)
                return;
            switch (currency)
            {
                case 0: c.guildTokens = SaturatingAdd(c.guildTokens, amount); break;
                case 1: c.arenaTokens = SaturatingAdd(c.arenaTokens, amount); break;
                case 2: c.dungeonStones = SaturatingAdd(c.dungeonStones, amount); break;
                case 3: c.mvpTokens = SaturatingAdd(c.mvpTokens, amount); break;
                case 4: c.eventTokens = SaturatingAdd(c.eventTokens, amount); break;
            }
        }

        private static long SaturatingAdd(long a, long b) =>
            b > 0 && a > long.MaxValue - b ? long.MaxValue : a + Mathf.Max(0L, b);

        private static long SaturatingMultiply(long a, long b) =>
            a > 0 && b > long.MaxValue / a ? long.MaxValue : a * b;
    }
}
