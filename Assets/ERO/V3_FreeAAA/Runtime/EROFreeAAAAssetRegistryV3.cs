using System;
using System.Collections.Generic;
using UnityEngine;

namespace EternalRealmsOnline.V3
{
    [CreateAssetMenu(menuName = "ERO/V3/Free AAA Asset Registry")]
    public sealed class EROFreeAAAAssetRegistryV3 : ScriptableObject
    {
        [Serializable]
        public sealed class Slot
        {
            public string id;
            public string category;
            public string intendedReplacement;
            public GameObject prefab;
            public Material material;
            public AnimationClip[] animations;
        }

        public List<Slot> slots = new List<Slot>();

        public Slot Find(string id)
        {
            return string.IsNullOrEmpty(id) ? null :
                slots.Find(s => string.Equals(s.id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
