using UnityEngine;

namespace EternalRealmsOnline.V4
{
    /// <summary>
    /// Defines the first shippable ERO gameplay loop without inventing new backend
    /// authority: Spawn -> Explore -> Combat -> Loot -> Return.
    /// Existing systems can bind to these presentation hooks.
    /// </summary>
    public sealed class EROV4VerticalSliceLoop : MonoBehaviour
    {
        public string startZone = "Greenhaven";
        public string dungeon = "Greenhaven Rift";
        public string boss = "Rift Guardian";

        public enum SliceState { Spawn, Explore, Combat, Loot, Return }
        public SliceState state { get; private set; } = SliceState.Spawn;

        public void SetState(SliceState next) => state = next;
    }
}
