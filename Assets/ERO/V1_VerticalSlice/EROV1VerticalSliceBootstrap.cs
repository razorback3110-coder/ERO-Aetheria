using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EternalRealmsOnline.V1
{
    /// <summary>
    /// V1 vertical slice bootstrap. Builds a compact playable dungeon showcase:
    /// player, party presentation, arena, enemies, boss, lighting and ERO HUD.
    /// It deliberately avoids legacy Boss Room presentation layers.
    /// </summary>
    public sealed class EROV1VerticalSliceBootstrap : MonoBehaviour
    {
        [Header("V1")]
        public string playerName = "NoHealNoGame";
        public int playerLevel = 58;
        public Color accent = new Color(0.25f, 0.55f, 1f, 1f);

        private readonly List<GameObject> spawned = new List<GameObject>();

        void Start()
        {
            BuildLighting();
            BuildArena();
            BuildPlayer();
            BuildEnemies();
            BuildBoss();
            BuildHUD();
            BuildCamera();
        }

        void BuildLighting()
        {
            RenderSettings.ambientIntensity = 0.45f;
            RenderSettings.reflectionIntensity = 0.65f;
            var go = new GameObject("V1_KeyLight");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(38f, -28f, 0f);
            spawned.Add(go);
        }

        void BuildArena()
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = "V1_DungeonArena";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(10f, 0.25f, 10f);
            ApplyMat(floor, new Color(0.045f, 0.055f, 0.08f));
            spawned.Add(floor);

            for (int i = 0; i < 12; i++)
            {
                float a = i * Mathf.PI * 2f / 12f;
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = "V1_GothicPillar";
                pillar.transform.position = new Vector3(Mathf.Cos(a) * 8f, 1.8f, Mathf.Sin(a) * 8f);
                pillar.transform.localScale = new Vector3(0.65f, 3.6f, 0.65f);
                ApplyMat(pillar, new Color(0.075f, 0.085f, 0.12f));
                spawned.Add(pillar);
            }
        }

        void BuildPlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "V1_Player_" + playerName;
            player.transform.position = new Vector3(0f, 1.2f, -5.5f);
            player.transform.localScale = new Vector3(0.55f, 1.2f, 0.55f);
            ApplyMat(player, new Color(0.72f, 0.78f, 0.9f));

            var ctrl = player.AddComponent<EROV1ThirdPersonController>();
            ctrl.moveSpeed = 4.2f;
            ctrl.cameraDistance = 6.2f;
            spawned.Add(player);
        }

        void BuildEnemies()
        {
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f;
                var mob = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                mob.name = "V1_Rift_Minion_" + i;
                mob.transform.position = new Vector3(Mathf.Cos(a) * 4.2f, 1f, Mathf.Sin(a) * 4.2f);
                mob.transform.localScale = Vector3.one * 0.8f;
                ApplyMat(mob, new Color(0.22f, 0.10f, 0.25f));
                var ai = mob.AddComponent<EROV1DungeonEnemy>();
                ai.target = GameObject.Find("V1_Player_" + playerName);
                spawned.Add(mob);
            }
        }

        void BuildBoss()
        {
            var boss = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boss.name = "V1_Rift_Boss";
            boss.transform.position = new Vector3(0f, 2.0f, 3.2f);
            boss.transform.localScale = new Vector3(2.4f, 2.8f, 2.4f);
            ApplyMat(boss, new Color(0.16f, 0.06f, 0.20f));

            var encounter = boss.AddComponent<EROV1BossEncounter>();
            encounter.maxHealth = 100000;
            encounter.displayName = "Lord of the Rift";
            spawned.Add(boss);
        }

        void BuildHUD()
        {
            var canvasGO = new GameObject("ERO_V1_HUD");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            var title = MakeText(canvasGO.transform, "ETERNAL REALMS ONLINE  •  AETHERIA", 24, new Vector2(30, -28), TextAnchor.UpperLeft);
            title.color = new Color(0.86f, 0.88f, 0.95f);

            var player = MakeText(canvasGO.transform, playerName + "  •  Lv. " + playerLevel, 22, new Vector2(30, -70), TextAnchor.UpperLeft);
            player.color = Color.white;

            var boss = MakeText(canvasGO.transform, "LORD OF THE RIFT  •  WORLD BOSS", 24, new Vector2(0, -35), TextAnchor.UpperCenter);
            boss.color = new Color(1f, 0.45f, 0.7f);

            var skills = MakeText(canvasGO.transform, "[1]  [2]  [3]  [4]  [5]  [6]        [R] [T] [G]", 23, new Vector2(0, 32), TextAnchor.LowerCenter);
            skills.color = accent;

            var location = MakeText(canvasGO.transform, "SHADOWSPIRE KEEP  •  DUNGEON", 18, new Vector2(30, 30), TextAnchor.LowerLeft);
            location.color = new Color(0.75f, 0.78f, 0.86f);

            var quest = MakeText(canvasGO.transform,
                "OBJECTIF\nVaincre le Seigneur du Rift\nRécupérer l'Artefact éternel",
                18, new Vector2(-30, -145), TextAnchor.UpperRight);
            quest.color = new Color(0.95f, 0.9f, 0.72f);
        }

        TextMeshProUGUI MakeText(Transform parent, string text, int size, Vector2 pos, TextAnchor anchor)
        {
            var go = new GameObject("HUD_Text");
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = Map(anchor);
            tmp.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            tmp.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            tmp.rectTransform.anchoredPosition = pos;
            tmp.rectTransform.sizeDelta = new Vector2(700, 180);
            return tmp;
        }

        TextAlignmentOptions Map(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                default: return TextAlignmentOptions.Center;
            }
        }

        void BuildCamera()
        {
            var go = new GameObject("V1_CinematicCamera");
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = 58f;
            go.AddComponent<EROV1CinematicCamera>();
            spawned.Add(go);
        }

        void ApplyMat(GameObject go, Color color)
        {
            var r = go.GetComponent<Renderer>();
            if (r == null) return;
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = color;
            r.sharedMaterial = m;
        }
    }

    public sealed class EROV1DungeonEnemy : MonoBehaviour
    {
        public GameObject target;
        public float speed = 1.3f;
        void Update()
        {
            if (target == null) return;
            Vector3 p = target.transform.position;
            p.y = transform.position.y;
            Vector3 d = p - transform.position;
            if (d.magnitude > 2.2f) transform.position += d.normalized * speed * Time.deltaTime;
            if (d.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
        }
    }

    public sealed class EROV1BossEncounter : MonoBehaviour
    {
        public string displayName = "Lord of the Rift";
        public int maxHealth = 100000;
        public float attackPulse = 0f;

        void Update()
        {
            attackPulse += Time.deltaTime;
            transform.Rotate(Vector3.up, 12f * Time.deltaTime, Space.World);
            float s = 1f + Mathf.Sin(attackPulse * 2f) * 0.04f;
            transform.localScale = new Vector3(2.4f, 2.8f, 2.4f) * s;
        }
    }
}
