using UnityEngine;

namespace EternalRealmsOnline.Gameplay
{
    /// <summary>
    /// Lightweight MMORPG-style HUD for the ERO playable slice.
    /// Uses Unity IMGUI only so the slice remains dependency-free while the production UI stack is built.
    /// </summary>
    public sealed class EROPlayableHUD : MonoBehaviour
    {
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle barStyle;
        private GUIStyle barBackgroundStyle;
        private GUIStyle hintStyle;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            BuildStyles();
        }

        private void BuildStyles()
        {
            panelStyle = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, padding = new RectOffset(14, 14, 10, 10) };
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold };
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
            barBackgroundStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(0, 0, 0, 0) };
            barStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(0, 0, 0, 0) };
        }

        private void OnGUI()
        {
            if (panelStyle == null) BuildStyles();

            const float width = 360f;
            const float height = 116f;
            var panel = new Rect(18f, Screen.height - height - 18f, width, height);
            GUI.Box(panel, GUIContent.none, panelStyle);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 8f, 220f, 26f), "AETHERIA ADVENTURER", titleStyle);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 35f, 220f, 20f), "Level 1  •  Adventurer", labelStyle);
            DrawBar(new Rect(panel.x + 14f, panel.y + 58f, 250f, 12f), 1f, "HP");
            DrawBar(new Rect(panel.x + 14f, panel.y + 76f, 250f, 12f), 0.72f, "MP");
            GUI.Label(new Rect(panel.x + 274f, panel.y + 58f, 70f, 30f), "XP 0 / 100", labelStyle);
            GUI.Label(new Rect(panel.x + 14f, panel.y + 94f, 330f, 18f), "WASD Move   •   SPACE / LMB Attack   •   F1-F3 Skills", hintStyle);

            var right = new Rect(Screen.width - 318f, Screen.height - 156f, 300f, 138f);
            GUI.Box(right, GUIContent.none, panelStyle);
            GUI.Label(new Rect(right.x + 12f, right.y + 10f, 270f, 22f), "QUICK ACTIONS", titleStyle);
            DrawSkillSlot(right, 0, "1", "Basic Attack");
            DrawSkillSlot(right, 1, "F1", "Skill I");
            DrawSkillSlot(right, 2, "F2", "Skill II");
            DrawSkillSlot(right, 3, "F3", "Skill III");
        }

        private void DrawBar(Rect rect, float fill, string label)
        {
            GUI.Box(rect, GUIContent.none, barBackgroundStyle);
            var filled = new Rect(rect.x + 2f, rect.y + 2f, (rect.width - 4f) * Mathf.Clamp01(fill), rect.height - 4f);
            GUI.Box(filled, GUIContent.none, barStyle);
            GUI.Label(new Rect(rect.x + 6f, rect.y - 2f, 35f, rect.height + 4f), label, hintStyle);
        }

        private void DrawSkillSlot(Rect panel, int index, string key, string name)
        {
            const float slot = 56f;
            var x = panel.x + 12f + index * (slot + 10f);
            var y = panel.y + 45f;
            GUI.Box(new Rect(x, y, slot, slot), GUIContent.none, panelStyle);
            GUI.Label(new Rect(x, y + 4f, slot, 18f), key, titleStyle);
            GUI.Label(new Rect(x + 3f, y + 28f, slot - 6f, 24f), name, hintStyle);
        }
    }
}
