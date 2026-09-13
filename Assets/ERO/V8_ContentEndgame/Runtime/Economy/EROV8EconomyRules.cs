namespace EternalRealmsOnline.V8
{
    public static class EROV8EconomyRules
    {
        public static bool IsValidGoldAmount(long amount) => amount >= 0 && amount <= 9_999_999_999L;
        public static bool IsValidCrystalAmount(long amount) => amount >= 0 && amount <= 9_999_999_999L;

        public static bool CanTransfer(long amount)
        {
            return amount > 0 && IsValidGoldAmount(amount);
        }
    }
}
