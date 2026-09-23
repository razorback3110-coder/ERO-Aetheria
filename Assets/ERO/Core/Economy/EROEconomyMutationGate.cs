using System;

namespace EternalRealmsOnline.Core.Economy
{
    /// <summary>
    /// Server-side readiness gate for authoritative economy mutations.
    /// The gate remains closed until durable startup recovery has completed.
    /// </summary>
    public sealed class EROEconomyMutationGate
    {
        private bool ready;

        public bool IsReady => ready;

        /// <summary>
        /// Opens the economy mutation boundary after startup recovery succeeds.
        /// This operation is intentionally one-way for the lifetime of the gate.
        /// </summary>
        public void MarkReady()
        {
            ready = true;
        }

        /// <summary>
        /// Throws before any authoritative economy mutation when startup recovery
        /// has not completed successfully.
        /// </summary>
        public void EnsureReady()
        {
            if (!ready)
                throw new InvalidOperationException(
                    "Economy mutations are blocked until durable startup recovery has completed.");
        }
    }
}
