using UnityEngine;

namespace EternalRealmsOnline.V7
{
    public sealed class EROV7AntiCheat
    {
        public static bool ValidateMove(Vector3 current, Vector3 requested, float maxDistance)
        {
            return Vector3.Distance(current, requested) <= maxDistance;
        }

        public static bool ValidatePositiveReward(int amount)
        {
            return amount > 0 && amount <= 1000000;
        }

        public static bool ValidateDamage(int damage)
        {
            return damage > 0 && damage <= 1000000;
        }
    }
}
