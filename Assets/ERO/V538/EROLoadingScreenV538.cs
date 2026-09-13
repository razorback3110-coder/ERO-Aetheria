using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EternalRealmsOnline.V538
{
    public sealed class EROLoadingScreenV538 : MonoBehaviour
    {
        public static EROLoadingScreenV538 I { get; private set; }
        Canvas canvas; GameObject root; Image backdrop, progress, vignette; TMP_Text title, subtitle, tip, percent; float shownAt;
        static readonly string[] Tips = {
            "Explore Aetheria and discover hidden Rift passages.",
            "Party play becomes essential in high-level raids and World Boss encounters.",
            "Your class evolution unlocks new skills at Lv.18, Lv.40 and Lv.75.",
            "Use the Map to travel between discovered regions.",
            "The Eternal Rift changes the rules of combat. Prepare before entering.",
            "Equipment, pets and mounts are part of your long-term progression."
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (I != null) return;
            var go = new GameObject("ERO_LoadingScreen_V538");
            DontDestroyOnLoad(go);
            I = go.AddComponent<EROLoadingScreenV538>();
        }

        void Awake() { Build(); SceneManager.sceneLoaded += SceneLoaded; }
        void OnDestroy(){ SceneManager.sceneLoaded -= SceneLoaded; }
        void SceneLoaded(Scene scene, LoadSceneMode mode){ if(scene.name != "ERO_Playable") return; Brief(scene.name,"LOADING WORLD"); }

        void Build()
        {
            canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30000;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920,1080); scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>();
            root = new GameObject("LoadingRoot"); root.transform.SetParent(canvas.transform,false); Stretch(root.AddComponent<RectTransform>());
            var bg = new GameObject("AetheriaBackdrop"); bg.transform.SetParent(root.transform,false); var raw = bg.AddComponent<RawImage>(); raw.texture = Resources.Load<Texture2D>("ERO/ERO_CharacterCreation_Reference"); raw.color = new Color(1,1,1,.94f); Stretch(raw.rectTransform);
            vignette = Image(root.transform,"Vignette",new Color(.005f,.008f,.02f,.72f)); Stretch(vignette.rectTransform);
            var frame = Image(root.transform,"Frame",new Color(.01f,.018f,.04f,.94f)); Set(frame.rectTransform,.12f,.12f,.88f,.88f);
            var line = Image(frame.transform,"GoldLine",new Color(.78f,.58f,.18f,.75f)); Set(line.rectTransform,.035f,.055f,.965f,.06f);
            title = Label(frame.transform,"ETERNAL REALMS ONLINE",34,new Color(1,.86f,.48f,1),.07f,.78f,.93f,.92f,TextAlignmentOptions.Center);
            subtitle = Label(frame.transform,"AETHERIA  •  THE ETERNAL RIFT",15,new Color(.65f,.78f,1,1),.07f,.72f,.93f,.78f,TextAlignmentOptions.Center);
            tip = Label(frame.transform,"",13,new Color(.86f,.90f,1,1),.12f,.19f,.88f,.29f,TextAlignmentOptions.Center);
            percent = Label(frame.transform,"0 %",12,new Color(1,.86f,.48f,1),.42f,.13f,.58f,.18f,TextAlignmentOptions.Center);
            var barBg = Image(frame.transform,"ProgressBackground",new Color(.015f,.025f,.05f,1)); Set(barBg.rectTransform,.17f,.115f,.83f,.145f);
            progress = Image(barBg.transform,"Progress",new Color(.78f,.58f,.18f,1)); Set(progress.rectTransform,0,0,0,1);
            HideImmediate();
        }

        public static void Show(string destination, string reason = "TRAVELING") { if (I != null) I.ShowInternal(destination,reason); }
        public static void Hide() { if (I != null) I.HideInternal(); }

        void ShowInternal(string destination,string reason)
        {
            if (root == null) return; root.SetActive(true); shownAt=Time.unscaledTime;
            title.text="ETERNAL REALMS ONLINE"; subtitle.text=reason+"  •  "+destination.ToUpperInvariant(); tip.text=Tips[Random.Range(0,Tips.Length)]; percent.text="0 %"; progress.rectTransform.anchorMax=new Vector2(0,1); vignette.color=new Color(.005f,.008f,.02f,.72f);
        }
        void HideInternal(){if(root)root.SetActive(false);}
        void HideImmediate(){if(root)root.SetActive(false);}

        public static void Brief(string destination,string reason="LOADING") { if(I!=null) I.StartCoroutine(I.BriefRoutine(destination,reason)); }
        IEnumerator BriefRoutine(string destination,string reason){ShowInternal(destination,reason);float duration=.85f;float t=0;while(t<duration){t+=Time.unscaledDeltaTime;float p=Mathf.Clamp01(t/duration);SetProgress(p);yield return null;}HideInternal();}

        public static void LoadScene(string sceneName,string destination,string reason="ENTERING")
        { if(I!=null) I.StartCoroutine(I.LoadSceneRoutine(sceneName,destination,reason)); else SceneManager.LoadSceneAsync(sceneName); }
        IEnumerator LoadSceneRoutine(string sceneName,string destination,string reason)
        {
            ShowInternal(destination,reason);
            yield return null;
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Single);
            if(op==null){HideInternal();yield break;}
            while(!op.isDone){SetProgress(Mathf.Clamp01(op.progress/.9f));yield return null;}
            SetProgress(1);yield return new WaitForSecondsRealtime(.35f);HideInternal();
        }
        void SetProgress(float p){if(progress)progress.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(p),1);if(percent)percent.text=Mathf.RoundToInt(Mathf.Clamp01(p)*100)+" %";}
        static Image Image(Transform p,string n,Color c){var g=new GameObject(n);g.transform.SetParent(p,false);var i=g.AddComponent<Image>();i.color=c;return i;}
        static TMP_Text Label(Transform p,string s,float size,Color c,float a,float b,float d,float e,TextAlignmentOptions align){var g=new GameObject("Text");g.transform.SetParent(p,false);var t=g.AddComponent<TextMeshProUGUI>();t.text=s;t.fontSize=size;t.color=c;t.alignment=align;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;Set(t.rectTransform,a,b,d,e);return t;}
        static void Stretch(RectTransform r){Set(r,0,0,1,1);} static void Set(RectTransform r,float a,float b,float c,float d){r.anchorMin=new Vector2(a,b);r.anchorMax=new Vector2(c,d);r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    }
}
