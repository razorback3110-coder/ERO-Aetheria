using System.Collections.Generic;
using UnityEngine;
using ERO.Data;

namespace ERO.Systems
{
    public sealed class QuestSystem : MonoBehaviour
    {
        public readonly List<QuestData> Active = new List<QuestData>();

        public void Add(QuestData q)
        {
            if (q != null && !Active.Exists(x => x.id == q.id))
                Active.Add(q);
        }

        public void Progress(string id, int amount, CharacterData c)
        {
            var q = Active.Find(x => x.id == id);
            if (q == null || q.completed) return;
            q.progress = Mathf.Min(q.required, q.progress + Mathf.Max(0, amount));
            if (q.progress >= q.required)
            {
                q.completed = true;
                if (c != null && q.creditsReward > 0)
                    c.credits = c.credits > long.MaxValue - q.creditsReward ? long.MaxValue : c.credits + q.creditsReward;
            }
        }
    }
}
