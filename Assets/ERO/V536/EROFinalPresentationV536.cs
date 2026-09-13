using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EternalRealmsOnline.V536
{
    public sealed class EROFinalPresentationV536 : MonoBehaviour
    {
        public static EROFinalPresentationV536 I { get; private set; }
        const string SceneName="ERO_Playable", CreatedKey="ERO_V536_CREATED";
        const string NameKey="ERO_V536_NAME", ClassKey="ERO_V536_CLASS", GenderKey="ERO_V536_GENDER", HairKey="ERO_V536_HAIR", EyesKey="ERO_V536_EYES", SkinKey="ERO_V536_SKIN", FaceKey="ERO_V536_FACE", ZoneKey="ERO_V536_ZONE";
        static readonly string[] Classes={"Knight","Assassin","Ranger","Mage","Priest","Monk","Summoner","Paladin"};
        static readonly string[] Roles={"Tank / Frontline","Melee Burst","Ranged DPS","Magic / Control","Healer / Support","Combo DPS","Summon / Control","Holy Tank / DPS"};
        static readonly string[] Zones={"Greenhaven","Everwood","Elyndor","Frostfall","Sunscar","Sylvaris","Abyssia","The Rift","Eternal Rift","Endless Abyss"};
        static readonly int[] ZoneLv={1,10,20,30,40,50,60,70,80,90};
        static readonly Color[] ZoneColor={new Color(.16f,.40f,.22f),new Color(.08f,.28f,.16f),new Color(.16f,.24f,.46f),new Color(.48f,.60f,.76f),new Color(.65f,.32f,.11f),new Color(.14f,.36f,.29f),new Color(.30f,.07f,.16f),new Color(.10f,.08f,.28f),new Color(.07f,.05f,.18f),new Color(.025f,.012f,.05f)};
        static readonly Color Gold=new Color(.92f,.72f,.29f), Gold2=new Color(1f,.88f,.52f), Text=new Color(.92f,.95f,1f), Muted=new Color(.58f,.68f,.82f), PanelColor=new Color(.015f,.025f,.05f,.97f), Panel2=new Color(.035f,.055f,.09f,.99f);
        readonly Dictionary<string,GameObject> windows=new(); readonly Dictionary<string,int> items=new(); readonly Dictionary<string,GameObject> cards=new(); readonly List<Enemy> enemies=new(); readonly List<GameObject> world=new(); readonly List<GameObject> preview=new();
        Canvas canvas; GameObject uiRoot,creator,hud,player,visual,previewStage,previewRoot; Transform worldRoot; Camera previewCam; RenderTexture previewTexture; CharacterController cc; Camera cam; EventSystem es; TMP_Text pName,pStats,zoneText,targetText,questText,goldText,crystalText,logText,cClass,cGender,cAppearance; TMP_InputField nameInput; Image hpBar,mpBar,xpBar,targetBar; bool creatorOpen,initialized,autoCombat; string playerName="Aetherian",zone="Greenhaven"; int cls,gender,hair,eyes,skin,face,level=1,hp,maxHp,mp,maxMp,gold=250,crystals; long xp; Enemy target; float attackCd,yaw=35,pitch=18; Vector3 velocity;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Boot(){if(I!=null)return;var g=new GameObject("ERO_V536_Runtime");DontDestroyOnLoad(g);I=g.AddComponent<EROFinalPresentationV536>();}
        void Awake(){if(I!=null&&I!=this){Destroy(gameObject);return;}I=this;SceneManager.sceneLoaded+=Loaded;}
        void OnDestroy(){SceneManager.sceneLoaded-=Loaded;}
        void Start(){Loaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);}
        void Loaded(Scene s,LoadSceneMode m){if(!s.name.Equals(SceneName,StringComparison.OrdinalIgnoreCase))return;Cleanup();EnsureEventSystem();HideLegacy(s);ReadProfile();BuildWorld();BuildPlayer();BuildCamera();BuildUI();SpawnEnemies();initialized=true;creatorOpen=!PlayerPrefs.HasKey("ERO_V12_PRESENTATION_READY")||PlayerPrefs.GetInt(CreatedKey,0)!=1;SetCreatorVisible(creatorOpen);Log(creatorOpen?"Create your Aetherian elf.":"Welcome to Greenhaven.");}
        void Cleanup(){foreach(string n in new[]{"ERO_V535_VisualWorld","Aetheria_Runtime_World_V533","ERO_V533_FullPlayable","ERO_V532_PlayableRuntime"}){var o=GameObject.Find(n);if(o!=null)Destroy(o);}if(uiRoot)Destroy(uiRoot);if(previewCam)Destroy(previewCam.gameObject);if(previewTexture!=null){previewTexture.Release();previewTexture=null;}if(previewRoot)Destroy(previewRoot);if(worldRoot)Destroy(worldRoot.gameObject);if(player)Destroy(player);if(cam)Destroy(cam.gameObject);enemies.Clear();world.Clear();}
        void EnsureEventSystem(){es=EventSystem.current;if(es==null){var g=new GameObject("ERO_EventSystem_V536");es=g.AddComponent<EventSystem>();g.AddComponent<StandaloneInputModule>();}}
        void HideLegacy(Scene s){foreach(var r in s.GetRootGameObjects()){if(r==gameObject)continue;foreach(var c in r.GetComponentsInChildren<Canvas>(true))c.enabled=false;foreach(var b in r.GetComponentsInChildren<MonoBehaviour>(true)){if(b==null)continue;string n=b.GetType().Name;if(n.IndexOf("NetworkSimulatorUIMediator",StringComparison.OrdinalIgnoreCase)>=0)b.enabled=false;}}}
        void ReadProfile(){playerName=PlayerPrefs.GetString(NameKey,"Aetherian");cls=Mathf.Clamp(PlayerPrefs.GetInt(ClassKey,0),0,7);gender=Mathf.Clamp(PlayerPrefs.GetInt(GenderKey,0),0,1);hair=Mathf.Clamp(PlayerPrefs.GetInt(HairKey,0),0,5);eyes=Mathf.Clamp(PlayerPrefs.GetInt(EyesKey,0),0,5);skin=Mathf.Clamp(PlayerPrefs.GetInt(SkinKey,0),0,4);face=Mathf.Clamp(PlayerPrefs.GetInt(FaceKey,0),0,3);level=Mathf.Clamp(PlayerPrefs.GetInt("ERO_LEVEL",1),1,100);gold=Mathf.Max(0,PlayerPrefs.GetInt("ERO_GOLD",250));crystals=Mathf.Max(0,PlayerPrefs.GetInt("ERO_CRYSTALS",0));xp=Math.Max(0,PlayerPrefs.GetInt("ERO_XP",0));zone=Zones[Mathf.Clamp(PlayerPrefs.GetInt(ZoneKey,0),0,9)];maxHp=240+level*42;hp=maxHp;maxMp=120+level*24;mp=maxMp;}

        void BuildWorld(){worldRoot=new GameObject("ERO_V536_World").transform;worldRoot.SetParent(transform,false);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0035f;RenderSettings.fogColor=new Color(.025f,.04f,.075f);RenderSettings.ambientLight=new Color(.16f,.19f,.27f);var sun=new GameObject("Aetheria_Light");sun.transform.SetParent(worldRoot);var l=sun.AddComponent<Light>();l.type=LightType.Directional;l.intensity=1.55f;l.shadows=LightShadows.Soft;l.color=new Color(.72f,.82f,1);sun.transform.rotation=Quaternion.Euler(42,-32,0);world.Add(sun);BuildZone(Array.IndexOf(Zones,zone));}
        void BuildZone(int z){Color c=ZoneColor[Mathf.Clamp(z,0,9)];BuildTerrain(c);BuildRoads(c);BuildSky(c,z);switch(z){case 0:Greenhaven(c);break;case 1:Everwood(c);break;case 2:Elyndor(c);break;case 3:Frostfall(c);break;case 4:Sunscar(c);break;case 5:Sylvaris(c);break;case 6:Abyssia(c);break;default:RiftZone(c,z);break;}for(int i=0;i<22;i++){float a=i*2.39f,r=30+(i%5)*4;Vector3 p=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);if(z<6)Tree(p,c,.65f+(i%3)*.18f);else Rock(p,c);}LabelWorld(Zones[z]+"  •  Lv."+ZoneLv[z]+"–"+(z==9?100:ZoneLv[z+1]-1),new Vector3(0,8,18));}
        void BuildTerrain(Color c){int n=32;float size=140;var m=new Mesh();var v=new Vector3[n*n];var uv=new Vector2[v.Length];var t=new int[(n-1)*(n-1)*6];for(int z=0;z<n;z++)for(int x=0;x<n;x++){float fx=(float)x/(n-1),fz=(float)z/(n-1);float h=(Mathf.PerlinNoise(fx*2.4f,fz*2.4f)-.5f)*3.2f;v[z*n+x]=new Vector3((fx-.5f)*size,h,(fz-.5f)*size);uv[z*n+x]=new Vector2(fx,fz);}int k=0;for(int z=0;z<n-1;z++)for(int x=0;x<n-1;x++){int a=z*n+x,b=a+1,c0=a+n,d=c0+1;t[k++]=a;t[k++]=c0;t[k++]=b;t[k++]=b;t[k++]=c0;t[k++]=d;}m.vertices=v;m.uv=uv;m.triangles=t;m.RecalculateNormals();var g=new GameObject("Aetheria_Terrain");g.transform.SetParent(worldRoot);g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=Mat("Terrain",Color.Lerp(c,new Color(.008f,.012f,.02f),.48f),.8f,0);g.AddComponent<MeshCollider>().sharedMesh=m;world.Add(g);}
        void BuildRoads(Color c){var rm=Mat("Road",Color.Lerp(c,Color.black,.7f),.9f,0);for(int i=-2;i<=2;i++){var r=Cube("Road",new Vector3(i*23,-.3f,0),new Vector3(6,.25f,100));r.GetComponent<Renderer>().sharedMaterial=rm;world.Add(r);}}
        void BuildSky(Color c,int z){var o=Sphere("AetherMoon",new Vector3(0,50,32),new Vector3(7,7,7));o.GetComponent<Renderer>().sharedMaterial=Mat("Moon",Color.Lerp(c,Color.white,.65f),.1f,2.5f);var l=o.AddComponent<Light>();l.type=LightType.Point;l.range=70;l.intensity=4;l.color=c;world.Add(o);if(z>=7)for(int i=0;i<8;i++){float a=i*Mathf.PI*2/8;var s=Cube("RiftCrystal",new Vector3(Mathf.Cos(a)*13,3+Mathf.Sin(a*2),Mathf.Sin(a)*13),new Vector3(.8f,4,.8f));s.transform.rotation=Quaternion.Euler(15,i*45,15);s.GetComponent<Renderer>().sharedMaterial=Mat("RiftCrystal",Color.Lerp(c,Color.cyan,.3f),.2f,2.5f);world.Add(s);}}
        void Greenhaven(Color c){for(int i=0;i<8;i++){float a=i*Mathf.PI*2/8;House(new Vector3(Mathf.Cos(a)*18,2,Mathf.Sin(a)*18),c);}Fountain(Vector3.zero);Grove(c,20,28);Water(new Vector3(-28,.05f,24),new Vector3(18,.18f,10));}
        void Water(Vector3 p,Vector3 scale){var q=Cube("AetherWater",p,scale);q.GetComponent<Renderer>().sharedMaterial=Mat("Water",new Color(.025f,.16f,.34f),.25f,0);world.Add(q);}
        void Everwood(Color c){Grove(c,48,58);AncientTree(new Vector3(0,0,10),8,c);Shrine(new Vector3(20,0,-18),c);}
        void Elyndor(Color c){for(int s=-1;s<=1;s+=2)Tower(new Vector3(s*15,0,4),c);House(new Vector3(0,2,4),c);Grove(c,18,26);}
        void Frostfall(Color c){for(int i=0;i<9;i++){float a=i*Mathf.PI*2/9;var p=new Vector3(Mathf.Cos(a)*16,0,Mathf.Sin(a)*16);var q=Cylinder("IceSpire",p+Vector3.up*4,new Vector3(2,4+(i%3),2));q.GetComponent<Renderer>().sharedMaterial=Mat("Ice",new Color(.55f,.8f,1),.15f,1.4f);world.Add(q);}}
        void Sunscar(Color c){for(int i=0;i<10;i++){float a=i*Mathf.PI*2/10;var p=new Vector3(Mathf.Cos(a)*17,2,Mathf.Sin(a)*17);var q=Cube("DesertRuin",p,new Vector3(2,4+(i%2)*2,2));q.transform.rotation=Quaternion.Euler(0,i*37,0);q.GetComponent<Renderer>().sharedMaterial=Mat("Sandstone",new Color(.50f,.28f,.10f),.6f,0);world.Add(q);}}
        void Sylvaris(Color c){Grove(c,32,34);for(int i=0;i<14;i++){float a=i*2.4f;var f=Sphere("MoonFlower",new Vector3(Mathf.Cos(a)*17,.8f,Mathf.Sin(a)*17),new Vector3(.8f,1.4f,.8f));f.GetComponent<Renderer>().sharedMaterial=Mat("MoonFlower",new Color(.52f,.25f,1),.1f,2);world.Add(f);}}
        void Abyssia(Color c){for(int i=0;i<8;i++){float a=i*Mathf.PI*2/8;Tower(new Vector3(Mathf.Cos(a)*16,0,Mathf.Sin(a)*16),c);}Obelisk(Vector3.zero,new Color(.65f,.08f,.15f));}
        void RiftZone(Color c,int z){Obelisk(Vector3.zero,Color.Lerp(c,Color.cyan,.4f));for(int i=0;i<5;i++){var b=Cube("RiftBridge",new Vector3(0,.2f,(i-2)*18),new Vector3(8,.5f,15));b.GetComponent<Renderer>().sharedMaterial=Mat("RiftStone",new Color(.08f,.07f,.15f),.85f,.5f);world.Add(b);}}
        void Grove(Color c,int count,float radius){for(int i=0;i<count;i++){float a=i*2.399f,r=radius*(.35f+.65f*((i*17)%100)/100f);Tree(new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r),c,1+(i%3)*.2f);}}
        void Tree(Vector3 p,Color c,float s){var tr=Cylinder("TreeTrunk",p+Vector3.up*2*s,new Vector3(.65f*s,2.1f*s,.65f*s));tr.GetComponent<Renderer>().sharedMaterial=Mat("Bark",new Color(.14f,.075f,.04f),.3f,0);world.Add(tr);for(int i=0;i<3;i++){var cr=Sphere("TreeCrown",p+Vector3.up*(4.1f+i*.8f)*s,new Vector3((2.2f-i*.25f)*s,(2.2f-i*.25f)*s,(2.2f-i*.25f)*s));cr.GetComponent<Renderer>().sharedMaterial=Mat("Leaves",Color.Lerp(c,Color.white,.1f),.2f,.05f);world.Add(cr);}}
        void AncientTree(Vector3 p,float s,Color c){Tree(p,c,s*1.8f);for(int i=0;i<6;i++){var b=Cylinder("Branch",p+Vector3.up*7,new Vector3(.4f,3,.4f));b.transform.rotation=Quaternion.Euler(25,i*60,20);b.GetComponent<Renderer>().sharedMaterial=Mat("AncientBark",new Color(.08f,.04f,.025f),.3f,0);world.Add(b);}}
        void House(Vector3 p,Color c){var h=Cube("House",p,new Vector3(8,4,7));h.GetComponent<Renderer>().sharedMaterial=Mat("House",new Color(.20f,.14f,.10f),.35f,0);world.Add(h);var r=Cylinder("Roof",p+Vector3.up*3.1f,new Vector3(4.6f,4.4f,4.6f));r.transform.rotation=Quaternion.Euler(0,0,90);r.GetComponent<Renderer>().sharedMaterial=Mat("Roof",Color.Lerp(c,Color.black,.2f),.65f,0);world.Add(r);var w=Cube("Window",p+new Vector3(0,.2f,-3.6f),new Vector3(1.5f,1.4f,.1f));w.GetComponent<Renderer>().sharedMaterial=Mat("Window",new Color(.25f,.65f,1),.1f,2);world.Add(w);}
        void Fountain(Vector3 p){var b=Cylinder("Fountain",p,new Vector3(5,.6f,5));b.GetComponent<Renderer>().sharedMaterial=Mat("Stone",new Color(.18f,.20f,.25f),.8f,0);world.Add(b);var w=Cylinder("Water",p+Vector3.up*.6f,new Vector3(4.2f,.08f,4.2f));w.GetComponent<Renderer>().sharedMaterial=Mat("Water",new Color(.06f,.32f,.7f),.05f,1.5f);world.Add(w);}
        void Shrine(Vector3 p,Color c){for(int i=0;i<4;i++){var q=Cube("ShrinePillar",p+new Vector3((i%2==0?-1:1)*3,2,(i<2?-1:1)*3),new Vector3(.7f,4,.7f));q.GetComponent<Renderer>().sharedMaterial=Mat("Shrine",c,.6f,1);world.Add(q);}Obelisk(p,c);}
        void Tower(Vector3 p,Color c){var q=Cylinder("Tower",p+Vector3.up*5,new Vector3(3,5,3));q.GetComponent<Renderer>().sharedMaterial=Mat("Tower",Color.Lerp(c,Color.black,.45f),.8f,.2f);world.Add(q);}
        void Obelisk(Vector3 p,Color c){var q=Cube("Obelisk",p+Vector3.up*3.5f,new Vector3(2,7,2));q.transform.rotation=Quaternion.Euler(0,45,0);q.GetComponent<Renderer>().sharedMaterial=Mat("Obelisk",c,.25f,2);var l=q.AddComponent<Light>();l.type=LightType.Point;l.range=16;l.intensity=3;l.color=c;world.Add(q);}
        void Rock(Vector3 p,Color c){var q=Sphere("Rock",p+Vector3.up*.5f,new Vector3(1.6f,.8f,1.2f));q.GetComponent<Renderer>().sharedMaterial=Mat("Rock",Color.Lerp(c,Color.black,.55f),.6f,.2f);world.Add(q);}
        void LabelWorld(string text,Vector3 p){var g=new GameObject("ZoneTitle");g.transform.SetParent(worldRoot);g.transform.position=p;var t=g.AddComponent<TextMesh>();t.text=text;t.fontSize=48;t.characterSize=.11f;t.alignment=TextAlignment.Center;t.anchor=TextAnchor.MiddleCenter;t.color=Gold2;g.AddComponent<BillboardV536>();world.Add(g);}
        GameObject Cube(string n,Vector3 p,Vector3 s){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(worldRoot);g.transform.position=p;g.transform.localScale=s;return g;} GameObject Sphere(string n,Vector3 p,Vector3 s){var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name=n;g.transform.SetParent(worldRoot);g.transform.position=p;g.transform.localScale=s;return g;} GameObject Cylinder(string n,Vector3 p,Vector3 s){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=n;g.transform.SetParent(worldRoot);g.transform.position=p;g.transform.localScale=s;return g;}

        void BuildPlayer(){player=new GameObject("ERO_Player_Elf");player.transform.SetParent(worldRoot);player.transform.position=new Vector3(0,1.1f,5);cc=player.AddComponent<CharacterController>();cc.height=2.1f;cc.radius=.42f;cc.center=new Vector3(0,1.05f,0);visual=BuildElf(cls,gender,hair,eyes,skin,face,false);visual.transform.SetParent(player.transform,false);}
        void RefreshPlayer(){if(visual)Destroy(visual);visual=BuildElf(cls,gender,hair,eyes,skin,face,false);visual.transform.SetParent(player.transform,false);}
        GameObject BuildElf(int c,int g,int h,int e,int s,int f,bool isPreview){var root=new GameObject(isPreview?"ElfPreview":"ElfPlayer");Color[] skins={new Color(.80f,.63f,.50f),new Color(.90f,.73f,.60f),new Color(.67f,.48f,.38f),new Color(.96f,.82f,.70f),new Color(.56f,.38f,.30f)};Color[] clothes={new Color(.25f,.32f,.43f),new Color(.18f,.08f,.20f),new Color(.18f,.30f,.20f),new Color(.18f,.22f,.46f),new Color(.70f,.68f,.80f),new Color(.48f,.22f,.16f),new Color(.28f,.16f,.46f),new Color(.55f,.48f,.20f)};Color skinC=skins[s%5],cloth=clothes[c];Part(root,PrimitiveType.Capsule,"Body",new Vector3(0,1.05f,0),new Vector3(g==0?.58f:.52f,.9f,g==0?.42f:.38f),skinC,.35f);Part(root,PrimitiveType.Capsule,"Tunic",new Vector3(0,1.08f,0),new Vector3(g==0?.67f:.61f,.88f,g==0?.48f:.44f),cloth,.5f);Part(root,PrimitiveType.Sphere,"Head",new Vector3(0,2.25f,0),new Vector3(.48f,.52f,.46f),skinC,.25f);for(int side=-1;side<=1;side+=2){var ear=Part(root,PrimitiveType.Capsule,"ElfEar",new Vector3(side*.47f,2.28f,.02f),new Vector3(.12f,.38f,.10f),skinC,.15f);ear.transform.rotation=Quaternion.Euler(0,0,side*-32);}Color[] hairs={new Color(.05f,.025f,.015f),new Color(.32f,.15f,.06f),new Color(.62f,.42f,.18f),new Color(.10f,.20f,.28f),new Color(.38f,.08f,.14f),new Color(.72f,.72f,.78f)};Color hairC=hairs[h%6];Part(root,PrimitiveType.Sphere,"Hair",new Vector3(0,2.56f,0),new Vector3(.53f,.36f,.50f),hairC,.35f);for(int i=0;i<4;i++){var l=Part(root,PrimitiveType.Capsule,"HairLock",new Vector3((i-1.5f)*.18f,2.34f,-.38f),new Vector3(.10f,.34f,.10f),hairC,.3f);l.transform.rotation=Quaternion.Euler(10,(i-1.5f)*10,0);}Color[] eyesC={new Color(.18f,.55f,1),new Color(.12f,.8f,.42f),new Color(.62f,.86f,1),new Color(.78f,.38f,.12f),new Color(.65f,.18f,.85f),Color.white};for(int side=-1;side<=1;side+=2)Part(root,PrimitiveType.Sphere,"Eye",new Vector3(side*.17f,2.30f,-.43f),new Vector3(.075f,.075f,.035f),eyesC[e%6],.1f);Part(root,PrimitiveType.Capsule,"Neck",new Vector3(0,1.82f,0),new Vector3(.20f,.25f,.20f),skinC,.2f);for(int side=-1;side<=1;side+=2){Part(root,PrimitiveType.Capsule,"Leg",new Vector3(side*.21f,.45f,0),new Vector3(.19f,.50f,.20f),cloth,.45f);Part(root,PrimitiveType.Cube,"Boot",new Vector3(side*.21f,.10f,-.10f),new Vector3(.25f,.20f,.46f),Color.Lerp(cloth,Color.black,.5f),.65f);Part(root,PrimitiveType.Capsule,"Arm",new Vector3(side*.62f,1.15f,0),new Vector3(.16f,.55f,.16f),cloth,.35f);}Weapon(root,c,cloth);root.AddComponent<V536ProceduralAnimator>();if(c==4||c==7){var ring=Part(root,PrimitiveTypeExtensions.TorusSafe,"Halo",new Vector3(0,2.98f,0),new Vector3(.62f,.62f,.08f),Gold2,.1f);ring.transform.rotation=Quaternion.Euler(90,0,0);}root.transform.localScale=isPreview?Vector3.one*1.15f:Vector3.one;return root;}
        void Weapon(GameObject r,int c,Color col){if(c==0||c==7){var sw=Part(r,PrimitiveType.Cube,"Sword",new Vector3(.80f,1.1f,-.12f),new Vector3(.10f,.90f,.12f),new Color(.72f,.78f,.88f),.85f);sw.transform.rotation=Quaternion.Euler(0,0,-25);Part(r,PrimitiveType.Cube,"Shield",new Vector3(-.76f,1.08f,.02f),new Vector3(.10f,.65f,.58f),col,.7f);}else if(c==1){for(int side=-1;side<=1;side+=2){var d=Part(r,PrimitiveType.Cube,"Dagger",new Vector3(side*.72f,.98f,-.20f),new Vector3(.08f,.55f,.08f),new Color(.8f,.85f,.95f),.85f);d.transform.rotation=Quaternion.Euler(0,0,side*35);}}else if(c==2){var b=Part(r,PrimitiveType.Cylinder,"Bow",new Vector3(.78f,1.2f,0),new Vector3(.07f,.9f,.07f),new Color(.48f,.28f,.10f),.55f);b.transform.rotation=Quaternion.Euler(0,0,12);}else{Part(r,PrimitiveType.Cylinder,"Staff",new Vector3(.78f,1.2f,0),new Vector3(.08f,1.25f,.08f),new Color(.28f,.16f,.08f),.45f);Part(r,PrimitiveType.Sphere,"StaffOrb",new Vector3(.78f,2.0f,0),new Vector3(.18f,.18f,.18f),c==4?Gold2:new Color(.35f,.55f,1),.1f);}}
        GameObject Part(GameObject r,PrimitiveType type,string n,Vector3 p,Vector3 s,Color c,float smooth){GameObject g=type==PrimitiveTypeExtensions.TorusSafe?Torus(n):GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(r.transform,false);g.transform.localPosition=p;g.transform.localScale=s;var rr=g.GetComponent<Renderer>();if(rr)rr.sharedMaterial=Mat(n,c,smooth,0);return g;}
        GameObject Torus(string n){var g=new GameObject(n);g.AddComponent<MeshFilter>().sharedMesh=MakeTorus(.6f,.06f,24,8);g.AddComponent<MeshRenderer>();return g;}
        Mesh MakeTorus(float R,float r,int seg,int ring){var m=new Mesh();var v=new Vector3[seg*ring];var t=new int[seg*ring*6];for(int i=0;i<seg;i++){float a=i*Mathf.PI*2/seg;for(int j=0;j<ring;j++){float b=j*Mathf.PI*2/ring;v[i*ring+j]=new Vector3((R+r*Mathf.Cos(b))*Mathf.Cos(a),r*Mathf.Sin(b),(R+r*Mathf.Cos(b))*Mathf.Sin(a));}}int k=0;for(int i=0;i<seg;i++)for(int j=0;j<ring;j++){int cur=i*ring+j,ni=((i+1)%seg)*ring+j,nj=i*ring+(j+1)%ring,nij=((i+1)%seg)*ring+(j+1)%ring;t[k++]=cur;t[k++]=ni;t[k++]=nj;t[k++]=nj;t[k++]=ni;t[k++]=nij;}m.vertices=v;m.triangles=t;m.RecalculateNormals();return m;}

        void BuildCamera(){var g=new GameObject("ERO_V536_Camera");cam=g.AddComponent<Camera>();cam.tag="MainCamera";cam.fieldOfView=58;cam.nearClipPlane=.05f;cam.farClipPlane=900;cam.transform.position=player.transform.position+new Vector3(7,4.8f,-9);cam.transform.LookAt(player.transform.position+Vector3.up*1.2f);}
        void BuildUI(){uiRoot=new GameObject("ERO_V536_UI");uiRoot.transform.SetParent(transform,false);canvas=uiRoot.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=10000;var sc=uiRoot.AddComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1920,1080);sc.matchWidthOrHeight=.5f;uiRoot.AddComponent<GraphicRaycaster>();BuildHUD();BuildCreator();BuildWindows();}
        void BuildHUD(){
            hud=new GameObject("HUD");
            hud.transform.SetParent(canvas.transform,false);
            Stretch(hud.AddComponent<RectTransform>());

            // Top-left character card
            var playerCard=Panel(hud.transform,.018f,.855f,.335f,.982f);
            Label(playerCard.transform,"AETHERIAN",18,Gold2,.055f,.66f,.70f,.94f);
            pName=Label(playerCard.transform,"",13,Text,.055f,.43f,.94f,.67f);
            pStats=Label(playerCard.transform,"",10,Muted,.055f,.27f,.94f,.45f);
            hpBar=Bar(playerCard.transform,.055f,.16f,.94f,.23f,new Color(.72f,.12f,.18f),"HP");
            mpBar=Bar(playerCard.transform,.055f,.08f,.94f,.15f,new Color(.12f,.38f,.82f),"MP");
            xpBar=Bar(playerCard.transform,.055f,.025f,.94f,.07f,new Color(.55f,.32f,.85f),"XP");

            // Top-center zone identity
            Label(hud.transform,"AETHERIA",18,Gold2,.38f,.945f,.62f,.985f).alignment=TextAlignmentOptions.Center;
            zoneText=Label(hud.transform,"GREENHAVEN  •  SAFE HAVEN",11,Muted,.35f,.915f,.65f,.945f);
            zoneText.alignment=TextAlignmentOptions.Center;

            // Top-right currencies
            var currency=Panel(hud.transform,.72f,.875f,.982f,.982f);
            goldText=Label(currency.transform,"",12,Gold2,.06f,.52f,.94f,.90f);
            crystalText=Label(currency.transform,"",11,new Color(.55f,.85f,1f),.06f,.15f,.94f,.50f);

            // Right-side compact minimap
            var map=Panel(hud.transform,.805f,.655f,.982f,.855f);
            Label(map.transform,"WORLD MAP",10,Gold2,.08f,.83f,.92f,.98f);
            var mapInner=Image(map.transform,"MapField",new Color(.015f,.045f,.075f,.95f));
            SetRect(mapInner.rectTransform,.08f,.10f,.92f,.80f);
            Label(map.transform,"✦",20,Gold2,.43f,.42f,.57f,.62f).alignment=TextAlignmentOptions.Center;
            Label(map.transform,"GREENHAVEN",8,Muted,.20f,.14f,.80f,.27f).alignment=TextAlignmentOptions.Center;

            // Target card
            var tp=Panel(hud.transform,.805f,.505f,.982f,.625f);
            Label(tp.transform,"TARGET",9,Muted,.07f,.80f,.93f,.95f);
            targetText=Label(tp.transform,"No Target",11,Text,.07f,.48f,.93f,.78f);
            targetText.alignment=TextAlignmentOptions.Center;
            targetBar=Bar(tp.transform,.07f,.14f,.93f,.29f,new Color(.75f,.12f,.16f),"HP");

            // Quest tracker
            var quest=Panel(hud.transform,.018f,.535f,.275f,.825f);
            Label(quest.transform,"QUEST TRACKER",10,Gold2,.07f,.86f,.93f,.97f);
            questText=Label(quest.transform,"",10,Text,.07f,.12f,.93f,.82f);

            // Bottom action bar
            var actions=Panel(hud.transform,.265f,.018f,.735f,.135f);
            for(int i=0;i<8;i++){
                int k=i;
                float x=.018f+i*.121f;
                var b=Button(actions.transform,(i+1).ToString(),12,Gold2,x,.20f,x+.102f,.84f);
                b.onClick.AddListener(()=>Skill(k));
            }
            Label(actions.transform,"LMB / E  Attack     RMB  Camera     WASD  Move",8,Muted,.02f,.02f,.98f,.18f).alignment=TextAlignmentOptions.Center;

            // Bottom-right controls
            var controls=Panel(hud.transform,.755f,.018f,.982f,.135f);
            var auto=Button(controls.transform,"AUTO",9,Text,.06f,.55f,.46f,.88f);
            auto.onClick.AddListener(()=>{autoCombat=!autoCombat;Log(autoCombat?"AUTO COMBAT ENABLED":"AUTO COMBAT DISABLED");});
            var menu=Button(controls.transform,"MENU",9,Gold2,.54f,.55f,.94f,.88f);
            menu.onClick.AddListener(()=>Open("menu"));
            var bag=Button(controls.transform,"BAG",9,Gold2,.06f,.12f,.46f,.45f);
            bag.onClick.AddListener(()=>Open("inventory"));
            var charBtn=Button(controls.transform,"CHARACTER",8,Text,.54f,.12f,.94f,.45f);
            charBtn.onClick.AddListener(()=>SetCreatorVisible(true));

            logText=Label(hud.transform,"",11,Gold2,.30f,.15f,.70f,.205f);
            logText.alignment=TextAlignmentOptions.Center;
        }
        void BuildCreator(){
            creator=new GameObject("CharacterCreator");
            creator.transform.SetParent(canvas.transform,false);
            Stretch(creator.AddComponent<RectTransform>());

            var bg=Image(creator.transform,"CreatorBackground",new Color(.004f,.007f,.018f,1f));
            Stretch(bg.rectTransform);

            // Subtle framed background, intentionally original and not a baked reference image.
            var glow=Image(creator.transform,"AetherGlow",new Color(.035f,.07f,.13f,.28f));
            SetRect(glow.rectTransform,.24f,.04f,.76f,.96f);

            Label(creator.transform,"ETERNAL REALMS ONLINE",28,Gold2,.035f,.925f,.62f,.975f);
            Label(creator.transform,"AETHERIA  •  THE ETERNAL RIFT",11,Muted,.038f,.895f,.62f,.925f);

            // Left class panel
            var left=Panel(creator.transform,.025f,.125f,.285f,.875f);
            Label(left.transform,"CHOOSE YOUR CLASS",16,Gold2,.06f,.925f,.94f,.98f);
            Label(left.transform,"Select a path for your Aetherian hero",9,Muted,.06f,.885f,.94f,.925f);
            string[] sym={"⚔","✦","➶","✧","✚","✺","◈","✜"};
            for(int i=0;i<8;i++){
                int k=i;
                float y=.775f-i*.087f;
                var card=Button(left.transform,sym[i]+"   "+Classes[i]+"\n      "+Roles[i],9,Text,.055f,y,.945f,y+.074f);
                cards[Classes[i]]=card.gameObject;
                card.onClick.AddListener(()=>{cls=k;RefreshCreator();});
            }

            // Center character preview
            var center=Panel(creator.transform,.305f,.125f,.695f,.875f);
            Label(center.transform,"CHARACTER PREVIEW",11,Muted,.06f,.93f,.94f,.98f).alignment=TextAlignmentOptions.Center;
            var stage=Image(center.transform,"PreviewFrame",new Color(.006f,.012f,.026f,.94f));
            SetRect(stage.rectTransform,.035f,.035f,.965f,.945f);

            previewStage=new GameObject("ElfPreviewStage");
            previewStage.transform.SetParent(center.transform,false);
            var pr=previewStage.AddComponent<RectTransform>();
            SetRect(pr,.055f,.055f,.945f,.945f);
            var pi=previewStage.AddComponent<RawImage>();
            pi.color=Color.white;

            previewRoot=new GameObject("ERO_V9_CharacterPreviewWorld");
            previewRoot.transform.SetParent(transform,false);
            previewRoot.layer=0;

            previewCam=new GameObject("ERO_V9_CharacterPreviewCamera").AddComponent<Camera>();
            previewCam.transform.SetParent(transform,false);
            previewCam.transform.position=new Vector3(0,1.58f,-7.2f);
            previewCam.transform.LookAt(new Vector3(0,1.45f,0));
            previewCam.fieldOfView=28;
            previewCam.nearClipPlane=.03f;
            previewCam.farClipPlane=50f;
            previewCam.cullingMask=~0;

            previewTexture=new RenderTexture(1024,1024,32,RenderTextureFormat.ARGB32);
            previewTexture.name="ERO_V10_CharacterPreview_RT";
            previewTexture.Create();
            previewCam.targetTexture=previewTexture;
            previewCam.enabled=true;
            previewCam.clearFlags=CameraClearFlags.SolidColor;
            previewCam.backgroundColor=new Color(.012f,.018f,.045f,1f);
            previewCam.cullingMask=~0;
            previewCam.allowHDR=true;
            previewCam.allowMSAA=true;
            previewCam.forceIntoRenderTexture=true;
            previewCam.stereoTargetEye=StereoTargetEyeMask.None;
            pi.texture=previewTexture;
            BuildPreviewEnvironment();

            Label(center.transform,"Rotate with RMB  •  Preview uses your final character",9,Muted,.10f,.025f,.90f,.07f).alignment=TextAlignmentOptions.Center;

            // Right customization panel
            var right=Panel(creator.transform,.715f,.125f,.975f,.875f);
            Label(right.transform,"CUSTOMIZATION",16,Gold2,.07f,.925f,.93f,.98f);

            cGender=Label(right.transform,"",11,Text,.07f,.84f,.93f,.895f);
            var gm=Button(right.transform,"♂  MALE",9,Gold2,.07f,.77f,.46f,.83f);
            gm.onClick.AddListener(()=>{gender=0;RefreshCreator();});
            var gf=Button(right.transform,"♀  FEMALE",9,Gold2,.54f,.77f,.93f,.83f);
            gf.onClick.AddListener(()=>{gender=1;RefreshCreator();});

            string[] f={"HAIR","EYES","SKIN","FACE"};
            for(int i=0;i<4;i++){
                int k=i;
                float y=.64f-i*.105f;
                Label(right.transform,f[i],9,Muted,.07f,y+.035f,.38f,y+.085f);
                var val=Label(right.transform,"",9,Text,.39f,y+.035f,.56f,y+.085f);
                var b=Button(right.transform,"CHANGE",8,Gold2,.59f,y,.93f,y+.09f);
                b.onClick.AddListener(()=>{
                    if(k==0)hair=(hair+1)%6;
                    else if(k==1)eyes=(eyes+1)%6;
                    else if(k==2)skin=(skin+1)%5;
                    else face=(face+1)%4;
                    RefreshCreator();
                });
            }

            cAppearance=Label(right.transform,"",10,Text,.07f,.18f,.93f,.57f);

            // Bottom creation strip
            var bottom=Panel(creator.transform,.025f,.025f,.975f,.105f);
            Label(bottom.transform,"CHARACTER NAME",8,Muted,.025f,.60f,.14f,.90f);
            nameInput=InputField(bottom.transform,.12f,.22f,.34f,.90f,playerName);
            Label(bottom.transform,"Your choices become the character used in Aetheria.",9,Text,.37f,.28f,.65f,.80f).alignment=TextAlignmentOptions.Center;
            var create=Button(bottom.transform,"CREATE CHARACTER",13,Gold2,.70f,.20f,.975f,.88f);
            create.onClick.AddListener(CreateCharacter);

            RefreshCreator();
        }
        void BuildPreviewEnvironment(){
            // Dedicated layer 30 isolates the character preview from the gameplay world.
            // The camera renders this small studio scene into the RenderTexture shown by the RawImage.
            previewRoot.layer=0;
            var floor=CubePreview("PreviewFloor",new Vector3(0,.02f,0),new Vector3(4.8f,.08f,4.8f),new Color(.035f,.045f,.075f));
            floor.transform.SetParent(previewRoot.transform,false);
            var backdrop=CubePreview("PreviewBackdrop",new Vector3(0,2.7f,1.85f),new Vector3(5.2f,5.6f,.12f),new Color(.018f,.028f,.055f));
            backdrop.transform.SetParent(previewRoot.transform,false);

            var left=NewPreviewLight("PreviewKey",LightType.Spot,new Vector3(-3.0f,4.2f,-3.0f),new Color(.62f,.76f,1f),7.5f,75f);
            left.transform.SetParent(previewRoot.transform,false);
            left.transform.LookAt(new Vector3(0,1.45f,0));
            var right=NewPreviewLight("PreviewFill",LightType.Spot,new Vector3(3.0f,2.8f,-1.5f),new Color(1f,.72f,.40f),5.0f,65f);
            right.transform.SetParent(previewRoot.transform,false);
            right.transform.LookAt(new Vector3(0,1.4f,0));
            var rim=NewPreviewLight("PreviewRim",LightType.Point,new Vector3(0,3.2f,2.0f),new Color(.35f,.55f,1f),4.5f,30f);
            rim.transform.SetParent(previewRoot.transform,false);

            // Decorative runic pillars give the creator a real fantasy studio backdrop.
            for(int side=-1;side<=1;side+=2){
                var p=CubePreview("RunePillar",new Vector3(side*2.15f,1.35f,1.35f),new Vector3(.22f,2.7f,.22f),new Color(.12f,.16f,.28f));
                p.transform.SetParent(previewRoot.transform,false);
                var orb=SpherePreview("RuneOrb",new Vector3(side*2.15f,2.95f,1.35f),new Vector3(.34f,.34f,.34f),new Color(.25f,.55f,1f));
                orb.transform.SetParent(previewRoot.transform,false);
            }
        }
        GameObject CubePreview(string n,Vector3 p,Vector3 s,Color c){
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name=n;g.transform.position=p;g.transform.localScale=s;
            var rr=g.GetComponent<Renderer>();if(rr)rr.sharedMaterial=Mat("Preview_"+n,c,.65f,0.8f);
            foreach(var tr in g.GetComponentsInChildren<Transform>(true))tr.gameObject.layer=0;
            return g;
        }
        GameObject SpherePreview(string n,Vector3 p,Vector3 s,Color c){
            var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.name=n;g.transform.position=p;g.transform.localScale=s;
            var rr=g.GetComponent<Renderer>();if(rr)rr.sharedMaterial=Mat("Preview_"+n,c,.25f,2.0f);
            foreach(var tr in g.GetComponentsInChildren<Transform>(true))tr.gameObject.layer=0;
            return g;
        }
        GameObject NewPreviewLight(string n,LightType type,Vector3 p,Color color,float intensity,float range){
            var g=new GameObject(n);g.transform.position=p;
            g.layer=30;
            var l=g.AddComponent<Light>();l.type=type;l.color=color;l.intensity=intensity;l.range=range;
            if(type==LightType.Spot)l.spotAngle=72f;
            return g;
        }
        void RefreshCreator(){cGender.text="GENDER  •  "+(gender==0?"MALE ELF":"FEMALE ELF");cAppearance.text="Hair "+(hair+1)+"\nEyes "+(eyes+1)+"\nSkin "+(skin+1)+"\nFace "+(face+1)+"\n\nClass  •  "+Classes[cls]+"\nRole   •  "+Roles[cls];foreach(var kv in cards)kv.Value.GetComponent<Image>().color=kv.Key==Classes[cls]?new Color(.22f,.17f,.08f,.99f):Panel2;foreach(var o in preview.ToArray())if(o)Destroy(o);preview.Clear();if(previewRoot==null)return;var e=BuildElf(cls,gender,hair,eyes,skin,face,true);e.transform.SetParent(previewRoot.transform,false);e.layer=0;foreach(var tr in e.GetComponentsInChildren<Transform>(true))tr.gameObject.layer=0;e.transform.localPosition=new Vector3(0,0,0);
            e.transform.localRotation=Quaternion.Euler(0,15,0);
            e.transform.localScale=Vector3.one*1.22f;
            foreach(var rr in e.GetComponentsInChildren<Renderer>(true)){rr.enabled=true;rr.gameObject.layer=30;}
            preview.Add(e);
            if(previewCam!=null){previewCam.enabled=true;previewCam.Render();}}
        void CreateCharacter(){playerName=string.IsNullOrWhiteSpace(nameInput.text)?"Aetherian":nameInput.text.Trim();PlayerPrefs.SetString(NameKey,playerName);PlayerPrefs.SetInt(ClassKey,cls);PlayerPrefs.SetInt(GenderKey,gender);PlayerPrefs.SetInt(HairKey,hair);PlayerPrefs.SetInt(EyesKey,eyes);PlayerPrefs.SetInt(SkinKey,skin);PlayerPrefs.SetInt(FaceKey,face);PlayerPrefs.SetInt(CreatedKey,1);PlayerPrefs.SetInt("ERO_V12_PRESENTATION_READY",1);PlayerPrefs.Save();SetCreatorVisible(false);RefreshPlayer();EternalRealmsOnline.V538.EROLoadingScreenV538.Brief("GREENHAVEN","ENTERING AETHERIA");Log("Welcome to Greenhaven, "+playerName+".");}
        void SetCreatorVisible(bool v){creatorOpen=v;if(creator)creator.SetActive(v);if(hud)hud.SetActive(!v);if(cc)cc.enabled=!v;}

        void BuildWindows(){
            Window("menu","AETHERIA",new[]{"CHARACTER","INVENTORY","EQUIPMENT","SKILLS","QUESTS","MAP","PARTY","GUILD","SOCIAL","MARKET","CRAFTING","CODEX","ACHIEVEMENTS","RANKINGS","PVP","DUNGEON","RAID","MVP","SETTINGS"},"Eternal Realms Online\nAetheria — The Eternal Rift\n\nAll ERO systems are presented through the unified dark-fantasy interface.");
            Window("inventory","INVENTORY",new[]{"ALL","EQUIPMENT","CONSUMABLES","MATERIALS","QUEST","MISC"},"Search • Sort • Compact • Favorites • Lock\nEquipment and item comparison are supported by the ERO item architecture.");
            Window("character","CHARACTER",new[]{"PROFILE","EQUIPMENT","STATS","SKILLS","AWAKENING"},"Your character is the same 3D character used in the world.\nF8 reopens character creation.");
            Window("quest","QUESTS",new[]{"MAIN","SIDE","DAILY","WEEKLY"},"Rift Echoes\nExplore Greenhaven\nDefeat Rift creatures\nRewards: XP + Gold + Rift Fragment");
            Window("guild","GUILD",new[]{"HOME","MEMBERS","BANK","GUILD QUEST","GUILD WAR"},"Guild: Patetik\nGuild level, members, contributions, guild bank and GvG.");
            Window("map","WORLD MAP",Zones,"Greenhaven → Everwood → Elyndor → Frostfall → Sunscar → Sylvaris → Abyssia → Rift → Eternal Rift → Endless Abyss.");
            Window("pvp","PVP / ENDGAME",new[]{"ARENA","RANKED","GvG","DUNGEON","RAID","MVP","WORLD BOSS"},"Competitive modes, raids, MVPs and World Boss progression.");
            Window("craft","CRAFTING",new[]{"WEAPON","ARMOR","POTION","MATERIALS"},"Crafting, gathering, recipes, upgrades and dismantling.");
            Window("settings","SETTINGS",new[]{"GRAPHICS","AUDIO","CONTROLS","LANGUAGE","ACCESSIBILITY"},"Graphics • Audio • Controls • FR / EN / DE / ES / IT / NL / PT / JP / KO / ZH");
        }

        void Window(string id,string title,string[] tabs,string body){var p=Panel(canvas.transform,.16f,.12f,.84f,.88f);p.name="Window_"+id;p.gameObject.SetActive(false);windows[id]=p.gameObject;Label(p.transform,title,25,Gold2,.05f,.90f,.80f,.98f);var x=Button(p.transform,"X",14,Gold2,.93f,.91f,.98f,.98f);x.onClick.AddListener(()=>p.gameObject.SetActive(false));float w=Mathf.Min(.16f,.84f/Mathf.Max(1,tabs.Length));for(int i=0;i<tabs.Length;i++){int k=i;var b=Button(p.transform,tabs[i],10,Text,.05f+i*w,.82f,.05f+i*w+w-.01f,.88f);b.onClick.AddListener(()=>WindowAction(id,tabs[k]));}Label(p.transform,body,14,Text,.06f,.30f,.94f,.77f);var a=Button(p.transform,"ACTION / TEST",12,Gold2,.06f,.18f,.28f,.26f);a.onClick.AddListener(()=>WindowAction(id,"ACTION"));}
        void WindowAction(string id,string action){if(id=="map"&&Zones.Contains(action)){Travel(action);return;}if(id=="menu"&&action=="CHARACTER"){SetCreatorVisible(true);return;}if(id=="craft"&&gold>=500){gold-=500;AddItem("Rift Blade");Log("Rift Blade crafted");return;}if(id=="pvp"&&(action=="PVP"||action=="RANKED"||action=="GvG"||action=="RAID"||action=="DUNGEON"||action=="MVP"||action=="WORLD BOSS")){EternalRealmsOnline.V538.EROLoadingScreenV538.Brief(action,"ENTERING ACTIVITY");Log(action+" • queue / instance preparation");return;}Log(id+" • "+action);}
        void Open(string id){foreach(var p in windows.Values)p.SetActive(false);if(windows.TryGetValue(id,out var w))w.SetActive(true);}

        void SpawnEnemies(){enemies.Clear();int z=Array.IndexOf(Zones,zone);for(int i=0;i<12;i++){var go=new GameObject("Mob_"+i);go.transform.SetParent(worldRoot);go.transform.position=new Vector3((i%4-1.5f)*9,1,(i/4-1)*9);var v=Monster(i%5);v.transform.SetParent(go.transform,false);enemies.Add(new Enemy{Name=MonsterName(i%5),level=ZoneLv[z]+i%3,hp=90+ZoneLv[z]*14+i*5,maxHp=90+ZoneLv[z]*14+i*5,damage=8+ZoneLv[z]/2,speed=.7f+z*.04f,go=go});}}
        string MonsterName(int i){return new[]{"Aether Slime","Everwolf","Aether Sprite","Rift Cultist","Abyss Hound"}[i];}
        GameObject Monster(int k){var r=new GameObject("MonsterVisual");Color[] c={new Color(.25f,.75f,1),new Color(.18f,.32f,.20f),new Color(.65f,.25f,1),new Color(.28f,.08f,.12f),new Color(.18f,.06f,.08f)};if(k==0)Part(r,PrimitiveType.Sphere,"Slime",new Vector3(0,.8f,0),new Vector3(1.2f,.8f,1.2f),c[k],.2f);else if(k==1){Part(r,PrimitiveType.Capsule,"Wolf",new Vector3(0,.8f,0),new Vector3(1.1f,.65f,.65f),c[k],.45f);Part(r,PrimitiveType.Sphere,"Head",new Vector3(.9f,1.1f,0),new Vector3(.45f,.42f,.42f),c[k],.4f);}else if(k==2){Part(r,PrimitiveType.Sphere,"Sprite",new Vector3(0,1.2f,0),new Vector3(.55f,.75f,.55f),c[k],.1f);for(int s=-1;s<=1;s+=2){var w=Part(r,PrimitiveType.Capsule,"Wing",new Vector3(s*.55f,1.25f,0),new Vector3(.15f,.65f,.55f),Color.Lerp(c[k],Color.white,.4f),.1f);w.transform.rotation=Quaternion.Euler(0,0,s*35);}}else if(k==3){Part(r,PrimitiveType.Capsule,"Cultist",new Vector3(0,1,0),new Vector3(.6f,1,.45f),c[k],.65f);Part(r,PrimitiveType.Sphere,"Head",new Vector3(0,2.1f,0),new Vector3(.42f,.42f,.42f),new Color(.16f,.08f,.08f),.25f);}else{Part(r,PrimitiveType.Capsule,"Hound",new Vector3(0,.85f,0),new Vector3(1.1f,.55f,.65f),c[k],.5f);for(int s=-1;s<=1;s+=2)Part(r,PrimitiveType.Capsule,"Horn",new Vector3(s*.5f,1.5f,0),new Vector3(.12f,.5f,.12f),new Color(.65f,.55f,.35f),.7f);}return r;}

        void LateUpdate(){if(creatorOpen&&previewCam!=null&&previewTexture!=null)previewCam.Render();}
        void Update(){if(Input.GetKeyDown(KeyCode.F8)){PlayerPrefs.DeleteKey("ERO_V9_PRESENTATION_READY");SetCreatorVisible(true);}if(!initialized||creatorOpen)return;InputGame();Move();if(autoCombat)Auto();EnemiesUpdate();CameraUpdate();UIUpdate();}
        void InputGame(){for(int i=0;i<8;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.F1+i))){cls=i;PlayerPrefs.SetInt(ClassKey,i);RefreshPlayer();Log("Class: "+Classes[i]);}if(Input.GetKeyDown(KeyCode.C))SetCreatorVisible(true);if(Input.GetKeyDown(KeyCode.I))Open("inventory");if(Input.GetKeyDown(KeyCode.Q))Open("quest");if(Input.GetKeyDown(KeyCode.M))SelectTarget();if(Input.GetKeyDown(KeyCode.Tab))Open("map");if((Input.GetMouseButtonDown(0)||Input.GetKeyDown(KeyCode.E))&&attackCd<=0){Attack(1);attackCd=.5f;}attackCd-=Time.deltaTime;}
        void Move(){if(!cc)return;float x=Input.GetAxisRaw("Horizontal"),z=Input.GetAxisRaw("Vertical");Vector3 f=cam.transform.forward;f.y=0;f.Normalize();Vector3 r=cam.transform.right;r.y=0;r.Normalize();Vector3 d=f*z+r*x;if(d.sqrMagnitude>1)d.Normalize();float sp=Input.GetKey(KeyCode.LeftShift)?8:5;if(d.sqrMagnitude>.01f){player.transform.rotation=Quaternion.Slerp(player.transform.rotation,Quaternion.LookRotation(d),Time.deltaTime*10);cc.Move(d*sp*Time.deltaTime);}velocity.y=cc.isGrounded?-1:velocity.y-22*Time.deltaTime;cc.Move(velocity*Time.deltaTime);}
        void CameraUpdate(){if(!cam)return;if(Input.GetMouseButton(1)){yaw+=Input.GetAxis("Mouse X")*3;pitch-=Input.GetAxis("Mouse Y")*2;pitch=Mathf.Clamp(pitch,8,40);}Quaternion q=Quaternion.Euler(pitch,yaw,0);Vector3 t=player.transform.position+Vector3.up*1.3f;cam.transform.position=t+q*new Vector3(0,1,-9);cam.transform.LookAt(t);}
        void SelectTarget(){target=enemies.Where(e=>e.Alive).OrderBy(e=>(e.go.transform.position-player.transform.position).sqrMagnitude).FirstOrDefault();if(target!=null)Log("Target: "+target.Name);}
        void Attack(int skill){if(target==null||!target.Alive)SelectTarget();if(target==null)return;if(Vector3.Distance(player.transform.position,target.go.transform.position)>9){Log("Target too far");return;}int dmg=25+level*6+cls*4;if(skill==2)dmg*=2;if(skill>=3)dmg*=3;target.Damage(dmg);if(!target.Alive){xp+=55+target.level*12;gold+=target.level*9;AddItem("Rift Fragment");LevelCheck();Log("Victory • loot acquired");}}
        void Skill(int k){int cost=10+k*5;if(mp<cost){Log("Not enough Mana");return;}mp-=cost;Attack(k);}
        void Auto(){if(target==null||!target.Alive)SelectTarget();if(target!=null&&Vector3.Distance(player.transform.position,target.go.transform.position)<7&&attackCd<=0){Attack(1);attackCd=.65f;}}
        void EnemiesUpdate(){foreach(var e in enemies){if(e==null||!e.Alive)continue;float d=Vector3.Distance(player.transform.position,e.go.transform.position);if(d<13){Vector3 dir=(player.transform.position-e.go.transform.position).normalized;e.go.transform.position+=dir*e.speed*Time.deltaTime;if(d<2&&UnityEngine.Random.value<Time.deltaTime*.6f){hp=Mathf.Max(0,hp-e.damage);if(hp<=0){hp=maxHp;player.transform.position=new Vector3(0,1.1f,5);Log("Defeated • respawned");}}}}}
        void UIUpdate(){if(!pName)return;pName.text=playerName+"  •  Lv."+level+"  •  "+Classes[cls];pStats.text="HP "+hp+" / "+maxHp+"    MP "+mp+" / "+maxMp;zoneText.text=zone+" • Lv."+level;goldText.text="Gold "+gold;crystalText.text="Cristaux ERO "+crystals;long need=100L+level*level*25L;SetBar(hpBar,(float)hp/maxHp);SetBar(mpBar,(float)mp/maxMp);questText.text="RIFT ECHOES\nExplore and defeat creatures\nXP "+xp+" / "+need+"\n\nC  Character\nI  Inventory\nTAB  Map";if(target!=null&&target.Alive){targetText.text=target.Name+" • Lv."+target.level+" • "+target.hp+" / "+target.maxHp;SetBar(targetBar,(float)target.hp/target.maxHp);}else{targetText.text="No Target";SetBar(targetBar,0);}}
        void LevelCheck(){while(level<100){long need=100L+level*level*25L;if(xp<need)break;xp-=need;level++;maxHp+=42;maxMp+=24;hp=maxHp;mp=maxMp;Log("LEVEL UP "+level);}}
        void Travel(string z){int i=Array.IndexOf(Zones,z);if(i<0)return;if(level<ZoneLv[i]){Log("Zone locked • level "+ZoneLv[i]);return;}zone=z;PlayerPrefs.SetInt(ZoneKey,i);PlayerPrefs.Save();RebuildZone();}
        void RebuildZone(){EternalRealmsOnline.V538.EROLoadingScreenV538.Brief(zone,"TRAVELING");foreach(var o in world.ToArray())if(o)Destroy(o);world.Clear();foreach(var e in enemies)if(e!=null)e.Destroy();enemies.Clear();BuildZone(Array.IndexOf(Zones,zone));player.transform.position=new Vector3(0,1.1f,5);SpawnEnemies();Log("Arrived in "+zone);}
        void AddItem(string n){if(!items.ContainsKey(n))items[n]=0;items[n]++;}
        void Log(string s){if(logText)logText.text=s;}

        Image Panel(Transform p,float a,float b,float c,float d){var i=Image(p,"Panel",PanelColor);SetRect(i.rectTransform,a,b,c,d);var o=i.gameObject.AddComponent<Outline>();o.effectColor=new Color(Gold.r,Gold.g,Gold.b,.32f);o.effectDistance=new Vector2(1.5f,1.5f);return i;}
        TMP_Text Label(Transform p,string s,float size,Color c,float a,float b,float d,float e){var g=new GameObject("Text");g.transform.SetParent(p,false);var t=g.AddComponent<TextMeshProUGUI>();t.text=s;t.fontSize=size;t.color=c;t.alignment=TextAlignmentOptions.Left;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;SetRect(t.rectTransform,a,b,d,e);return t;}
        Button Button(Transform p,string s,float size,Color c,float a,float b,float d,float e){var i=Image(p,"Button",new Color(.025f,.045f,.075f,.98f));SetRect(i.rectTransform,a,b,d,e);var o=i.gameObject.AddComponent<Outline>();o.effectColor=new Color(Gold.r,Gold.g,Gold.b,.25f);o.effectDistance=new Vector2(1,1);var bt=i.gameObject.AddComponent<Button>();var cs=bt.colors;cs.highlightedColor=new Color(.18f,.20f,.30f,1);cs.pressedColor=new Color(.35f,.25f,.10f,1);bt.colors=cs;Label(i.transform,s,size,c,0,0,1,1).alignment=TextAlignmentOptions.Center;return bt;}
        TMP_InputField InputField(Transform p,float a,float b,float c,float d,string value){var i=Image(p,"Input",Panel2);SetRect(i.rectTransform,a,b,c,d);var input=i.gameObject.AddComponent<TMP_InputField>();var t=Label(i.transform,value,14,Text,.04f,.05f,.96f,.95f);input.textComponent=t;input.text=value;return input;}
        Image Bar(Transform p,float a,float b,float c,float d,Color color,string label){var bg=Image(p,"Bar",new Color(.01f,.015f,.03f,.9f));SetRect(bg.rectTransform,a,b,c,d);var fill=Image(bg.transform,"Fill",color);SetRect(fill.rectTransform,0,0,1,1);Label(bg.transform,label,8,Text,0,0,1,1).alignment=TextAlignmentOptions.Center;return fill;}
        Image Image(Transform p,string n,Color c){var g=new GameObject(n);g.transform.SetParent(p,false);var i=g.AddComponent<Image>();i.color=c;return i;}
        void SetBar(Image i,float v){if(i)i.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(v),1);}
        void Stretch(RectTransform r){SetRect(r,0,0,1,1);}void SetRect(RectTransform r,float a,float b,float c,float d){r.anchorMin=new Vector2(a,b);r.anchorMax=new Vector2(c,d);r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
        Material Mat(string n,Color c,float smooth,float emission){Shader s=null;if(n.IndexOf("Rift",StringComparison.OrdinalIgnoreCase)>=0)s=Shader.Find("ERO/Rift");else if(n.IndexOf("MoonFlower",StringComparison.OrdinalIgnoreCase)>=0)s=Shader.Find("ERO/Holographic");else if(n.IndexOf("Water",StringComparison.OrdinalIgnoreCase)>=0)s=Shader.Find("ERO/Water");if(!s)s=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");if(!s)return null;var m=new Material(s);m.name="ERO_"+n;if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);if(m.HasProperty("_Color"))m.SetColor("_Color",c);if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);if(m.HasProperty("_EmissionColor")){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*emission);}if(m.HasProperty("_Emission"))m.SetColor("_Emission",c*emission);return m;}
        sealed class Enemy{public string Name;public int level,hp,maxHp,damage;public float speed;public GameObject go;public bool Alive=>hp>0&&go;public void Damage(int d){hp=Mathf.Max(0,hp-d);if(hp==0&&go)go.SetActive(false);}public void Destroy(){if(go)UnityEngine.Object.Destroy(go);}}
    }
    public sealed class BillboardV536:MonoBehaviour{void LateUpdate(){if(Camera.main)transform.forward=Camera.main.transform.forward;}}
    internal static class PrimitiveTypeExtensions{public const PrimitiveType TorusSafe=(PrimitiveType)10001;}
}
