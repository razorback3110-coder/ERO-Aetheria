namespace EternalRealmsOnline.V8
{
    public enum EROV8PvPMode
    {
        OpenWorld,
        Arena,
        Ranked,
        GvG
    }

    public static class EROV8PvPRules
    {
        public static bool IsAutoCombatAllowed(EROV8PvPMode mode)
        {
            return mode == EROV8PvPMode.OpenWorld;
        }

        public static bool CanDamage(EROV8PvPMode mode, bool sameTeam)
        {
            if (sameTeam) return false;
            return true;
        }
    }
}
