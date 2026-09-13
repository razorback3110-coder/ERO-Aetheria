using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EternalRealmsOnline.V502
{
    public sealed class EROSystemBootstrapV502 : MonoBehaviour
    {
        static EROSystemBootstrapV502 instance;
        Canvas canvas;
        Text status;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (instance != null) return;
            var go = new GameObject("ERO_V502_System");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<EROSystemBootstrapV502>();
            go.AddComponent<EROPlayerProfileRuntime>();
        }

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Startup" || scene.name == "MainMenu")
                BuildStatusOverlay();
            else if (scene.name == "CharSelect")
            {
                if (canvas != null) canvas.gameObject.SetActive(false);
            }
            else if (canvas != null) canvas.gameObject.SetActive(false);
        }

        void BuildStatusOverlay()
        {
            if (canvas != null) { canvas.gameObject.SetActive(true); return; }
            var cgo = new GameObject("ERO_V502_Status");
            cgo.transform.SetParent(transform, false);
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            var textGo = new GameObject("Status");
            textGo.transform.SetParent(cgo.transform, false);
            status = textGo.AddComponent<Text>();
            status.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            status.fontSize = 16;
            status.alignment = TextAnchor.UpperLeft;
            status.color = Color.white;
            var rt = status.rectTransform;
            rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(0,1);
            rt.pivot = new Vector2(0,1); rt.anchoredPosition = new Vector2(18,-18);
            rt.sizeDelta = new Vector2(620,80);
            status.text = "ETERNAL REALMS ONLINE  •  V509\n8 classes • progression 1→75 • Localization + Founder systems ready";
        }
    }
}
