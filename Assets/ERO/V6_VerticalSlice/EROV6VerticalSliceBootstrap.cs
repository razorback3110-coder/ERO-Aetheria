using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EternalRealmsOnline.V6
{
    /// <summary>
    /// ERO V6 presentation gate: uses the existing V536 master canvas and gates the
    /// first-launch flow as Main Menu -> Character Creator -> Greenhaven.
    /// It reuses V536's player/profile implementation instead of creating a second player.
    /// </summary>
    public sealed class EROV6VerticalSliceBootstrap : MonoBehaviour
    {
        const string SceneName = "ERO_Playable";
        const string CreatedKey = "ERO_V536_CREATED";
        const string UiName = "ERO_V536_UI";
        const string MenuName = "ERO_V6_MainMenu";

        GameObject menu;
        GameObject v536Ui;
        Type presentationType;
        MethodInfo setCreatorVisible;
        bool ready;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (FindFirstObjectByType<EROV6VerticalSliceBootstrap>() != null) return;
            var go = new GameObject("ERO_V6_VerticalSlice");
            DontDestroyOnLoad(go);
            go.AddComponent<EROV6VerticalSliceBootstrap>();
        }

        void Awake() { SceneManager.sceneLoaded += OnSceneLoaded; }
        void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }
        void Start() { StartCoroutine(WaitForPresentation()); }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.Equals(SceneName, StringComparison.OrdinalIgnoreCase))
                StartCoroutine(WaitForPresentation());
        }

        System.Collections.IEnumerator WaitForPresentation()
        {
            ready = false;
            for (int i = 0; i < 240; i++)
            {
                v536Ui = GameObject.Find(UiName);
                if (v536Ui != null)
                {
                    presentationType = Type.GetType("EternalRealmsOnline.V536.EROFinalPresentationV536, Assembly-CSharp");
                    if (presentationType != null)
                        setCreatorVisible = presentationType.GetMethod("SetCreatorVisible", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (setCreatorVisible != null) break;
                }
                yield return null;
            }
            if (v536Ui == null || setCreatorVisible == null) yield break;
            BuildMenu();
            ready = true;
            ShowMainMenu();
        }

        void BuildMenu()
        {
            if (menu != null) Destroy(menu);
            var canvas = v536Ui.GetComponent<Canvas>();
            if (canvas == null) return;

            menu = new GameObject(MenuName);
            menu.transform.SetParent(canvas.transform, false);
            var root = menu.AddComponent<RectTransform>();
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero; root.offsetMax = Vector2.zero;

            AddPanel(root, new Color(.004f, .008f, .02f, .985f));
            AddPanel(root, new Color(.02f, .045f, .09f, .58f), .055f, .055f, .945f, .945f);
            AddText(root, "ETERNAL REALMS ONLINE", 34, new Color(.95f,.78f,.34f), .08f,.76f,.92f,.88f, TextAlignmentOptions.Center);
            AddText(root, "AETHERIA  •  THE ETERNAL RIFT", 14, new Color(.62f,.73f,.90f), .08f,.70f,.92f,.75f, TextAlignmentOptions.Center);
            AddText(root, "Enter a world shaped by the Eternal Rift.", 12, Color.white, .12f,.62f,.88f,.67f, TextAlignmentOptions.Center);

            var create = AddButton(root, "CREATE CHARACTER", .34f,.45f,.66f,.54f);
            create.onClick.AddListener(OpenCreator);
            bool hasCharacter = PlayerPrefs.GetInt(CreatedKey, 0) == 1;
            var enter = AddButton(root, "ENTER AETHERIA", .34f,.35f,.66f,.43f);
            enter.interactable = hasCharacter;
            enter.onClick.AddListener(EnterWorld);
            var quit = AddButton(root, "RETURN TO DESKTOP", .34f,.25f,.66f,.33f);
            quit.onClick.AddListener(Application.Quit);

            string status = hasCharacter ? "Character saved • Greenhaven ready" : "No character created • Create your Aetherian hero first";
            AddText(root, status, 11, new Color(.58f,.68f,.82f), .20f,.15f,.80f,.20f, TextAlignmentOptions.Center);
            AddText(root, "WASD Move  •  RMB Camera  •  LMB / E Attack  •  F8 Creator", 9, new Color(.55f,.64f,.78f), .12f,.06f,.88f,.11f, TextAlignmentOptions.Center);
        }

        void ShowMainMenu()
        {
            if (!ready || menu == null) return;
            menu.SetActive(true);
            v536Ui.SetActive(true);
            InvokeCreator(false);
        }

        void OpenCreator()
        {
            menu.SetActive(false);
            v536Ui.SetActive(true);
            InvokeCreator(true);
        }

        void EnterWorld()
        {
            menu.SetActive(false);
            v536Ui.SetActive(true);
            InvokeCreator(false);
        }

        void InvokeCreator(bool visible)
        {
            var presentation = presentationType == null ? null : FindFirstObjectByType(presentationType);
            if (presentation != null && setCreatorVisible != null)
                setCreatorVisible.Invoke(presentation, new object[] { visible });
        }

        GameObject AddPanel(RectTransform parent, Color color, float x0=0, float y0=0, float x1=1, float y1=1)
        {
            var go = new GameObject("Panel"); go.transform.SetParent(parent,false);
            var rt = go.AddComponent<RectTransform>(); rt.anchorMin=new Vector2(x0,y0); rt.anchorMax=new Vector2(x1,y1); rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            go.AddComponent<Image>().color=color; return go;
        }

        TMP_Text AddText(RectTransform parent, string value, float size, Color color, float x0,float y0,float x1,float y1, TextAlignmentOptions alignment)
        {
            var go=new GameObject("Text"); go.transform.SetParent(parent,false);
            var rt=go.AddComponent<RectTransform>(); rt.anchorMin=new Vector2(x0,y0); rt.anchorMax=new Vector2(x1,y1); rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            var text=go.AddComponent<TextMeshProUGUI>(); text.text=value; text.fontSize=size; text.color=color; text.alignment=alignment; text.textWrappingMode=TextWrappingModes.Normal; return text;
        }

        Button AddButton(RectTransform parent, string label, float x0,float y0,float x1,float y1)
        {
            var go=new GameObject("Button_"+label.Replace(" ","_")); go.transform.SetParent(parent,false);
            var rt=go.AddComponent<RectTransform>(); rt.anchorMin=new Vector2(x0,y0); rt.anchorMax=new Vector2(x1,y1); rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
            var image=go.AddComponent<Image>(); image.color=new Color(.035f,.07f,.13f,.98f);
            var button=go.AddComponent<Button>(); button.targetGraphic=image;
            var colors=button.colors; colors.normalColor=new Color(.035f,.07f,.13f,.98f); colors.highlightedColor=new Color(.18f,.15f,.08f,.99f); colors.pressedColor=new Color(.35f,.27f,.10f,.99f); colors.disabledColor=new Color(.04f,.045f,.06f,.55f); button.colors=colors;
            AddText(rt,label,12,new Color(.94f,.82f,.48f),0,0,1,1,TextAlignmentOptions.Center); return button;
        }
    }
}
