using System;

namespace ERO.Systems
{
    /// <summary>
    /// Fixed-step simulation clock for authoritative gameplay/network ticks.
    /// Keeps simulation cadence independent from render framerate and exposes a
    /// monotonically increasing tick id that can be persisted or replicated.
    /// </summary>
    public sealed class EROSimulationClock
    {
        public const int DefaultTicksPerSecond = 20;
        public const int DefaultMaxCatchUpTicks = 4;

        private readonly double tickDuration;
        private readonly int maxCatchUpTicks;
        private double accumulator;

        public ulong TickId { get; private set; }
        public int TicksPerSecond { get; }

        public EROSimulationClock(
            int ticksPerSecond = DefaultTicksPerSecond,
            int maxCatchUpTicks = DefaultMaxCatchUpTicks)
        {
            if (ticksPerSecond <= 0) throw new ArgumentOutOfRangeException(nameof(ticksPerSecond));
            if (maxCatchUpTicks <= 0) throw new ArgumentOutOfRangeException(nameof(maxCatchUpTicks));

            TicksPerSecond = ticksPerSecond;
            this.maxCatchUpTicks = maxCatchUpTicks;
            tickDuration = 1d / ticksPerSecond;
        }

        /// <summary>
        /// Advances real elapsed time and invokes one fixed simulation step per tick.
        /// Excessive stalls are bounded so a hitch cannot create an unbounded catch-up loop.
        /// </summary>
        public int Advance(double elapsedSeconds, Action<ulong> simulateTick)
        {
            if (simulateTick == null) throw new ArgumentNullException(nameof(simulateTick));
            if (elapsedSeconds <= 0d) return 0;

            accumulator += elapsedSeconds;
            int executed = 0;

            while (accumulator >= tickDuration && executed < maxCatchUpTicks)
            {
                accumulator -= tickDuration;
                TickId++;
                simulateTick(TickId);
                executed++;
            }

            if (executed == maxCatchUpTicks && accumulator >= tickDuration)
                accumulator = tickDuration * 0.5d;

            return executed;
        }

        public void Reset(ulong tickId = 0)
        {
            TickId = tickId;
            accumulator = 0d;
        }
    }
}
