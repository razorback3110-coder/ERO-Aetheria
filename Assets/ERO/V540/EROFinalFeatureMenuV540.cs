using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EternalRealmsOnline.V540
{
    /// <summary>Optional production feature launcher. It exposes every final-game surface from one ERO menu.</summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class EROFinalFeatureMenuV540 : MonoBehaviour
    {
        Canvas canvas; GameObject root; TMP_Text title, body; readonly List<GameObject> cards=new();
        static readonly Color Gold=new(.92f,.72f,.29f), Text=new(.9f,.94f,1f), Muted=new(.55f,.65f,.8f), Panel=new(.012f,.02f,.045f,.98f), Panel2=new(.025f,.04f,.075f,.98f);
        void Start(){if(!Application.isPlaying)return;Invoke(nameof(Build),0.25f);}
        void Update(){if(Input.GetKeyDown(KeyCode.F10))Toggle();if(Input.GetKeyDown(KeyCode.Escape)&&root&&root.activeSelf)Toggle();}
        void Build(){if(root)return;EnsureEventSystem();var cg=new GameObject("ERO_V540_FeatureMenu");canvas=cg.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=9000;cg.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;cg.GetComponent<CanvasScaler>().referenceResolution=new Vector2(1920,1080);cg.AddComponent<GraphicRaycaster>();root=PanelObj(canvas.transform,"Root",Panel);Stretch(root.GetComponent<RectTransform>());root.SetActive(false);
            var top=PanelObj(root.transform,"Top",new Color(.008f,.012f,.03f,.98f));Rect(top.GetComponent<RectTransform>(),0,.91f,1,1);TextLabel(top.transform,"AETHERIA  •  ERO",26,Gold,0.025f,.18f,.5f,.85f);
            var close=Btn(root.transform,"CLOSE  [F10]",14,Gold,.86f,.925f,.975f,.985f);close.onClick.AddListener(Toggle);
            var left=PanelObj(root.transform,"Navigation",Panel2);Rect(left.GetComponent<RectTransform>(),.02f,.05f,.25f,.90f);
            var content=PanelObj(root.transform,"Content",new Color(.008f,.015f,.032f,.97f));Rect(content.GetComponent<RectTransform>(),.27f,.05f,.98f,.90f);
            title=TextLabel(content.transform,"SYSTEM",30,Gold,.035f,.84f,.96f,.94f);body=TextLabel(content.transform,"Select a system",16,Text,.035f,.06f,.96f,.80f);body.textWrappingMode=TextWrappingModes.Normal;
            float y=.84f;foreach(var f in EROFeatureHubV540.Instance.Features){var b=Btn(left.transform,f.title,13,f.category==EROFeatureCategory.System?Gold:Text,.08f,y-.045f,.92f,y);b.onClick.AddListener(()=>Open(f));y-=.055f;if(y<.05f)break;}
        }
        void Open(EROFeatureDefinition f){if(title)title.text=f.title.ToUpperInvariant();if(body)body.text=$"{f.category}\n\n{f.description}\n\nShortcut: {f.shortcut}\n\nThis panel is bound to the ERO feature contract. The final gameplay implementation can use the same ID: {f.id}.";EROFeatureHubV540.Instance.Open(f.id);}
        void Toggle(){if(!root)Build();if(root)root.SetActive(!root.activeSelf);}
        void EnsureEventSystem(){if(EventSystem.current!=null)return;var g=new GameObject("ERO_V540_EventSystem");g.AddComponent<EventSystem>();g.AddComponent<StandaloneInputModule>();}
        GameObject PanelObj(Transform p,string n,Color c){var g=new GameObject(n);g.transform.SetParent(p,false);var i=g.AddComponent<Image>();i.color=c;return g;}
        TMP_Text TextLabel(Transform p,string s,float size,Color c,float a,float b,float d,float e){var g=new GameObject("Text");g.transform.SetParent(p,false);var t=g.AddComponent<TextMeshProUGUI>();t.text=s;t.fontSize=size;t.color=c;t.alignment=TextAlignmentOptions.Left;t.raycastTarget=false;Rect(t.rectTransform,a,b,d,e);return t;}
        Button Btn(Transform p,string s,float size,Color c,float a,float b,float d,float e){var g=PanelObj(p,"Button",new Color(.02f,.035f,.065f,.98f));Rect(g.GetComponent<RectTransform>(),a,b,d,e);var o=g.AddComponent<Outline>();o.effectColor=new Color(Gold.r,Gold.g,Gold.b,.22f);o.effectDistance=new Vector2(1,1);var btt=g.AddComponent<Button>();var cs=btt.colors;cs.highlightedColor=new Color(.12f,.18f,.28f,1);cs.pressedColor=new Color(.32f,.24f,.1f,1);btt.colors=cs;var t=TextLabel(g.transform,s,size,c,.05f,.05f,.95f,.95f);t.alignment=TextAlignmentOptions.Center;return btt;}
        void Rect(RectTransform r,float a,float b,float d,float e){r.anchorMin=new Vector2(a,b);r.anchorMax=new Vector2(d,e);r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
        void Stretch(RectTransform r)=>Rect(r,0,0,1,1);
    }
}
