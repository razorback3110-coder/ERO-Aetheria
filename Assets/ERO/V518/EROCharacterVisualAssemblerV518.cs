using UnityEngine;
namespace EternalRealmsOnline.V518
{
    public static class EROCharacterVisualAssemblerV518
    {
        static readonly string[] Names={"Knight","Assassin","Ranger","Mage","Priest","Monk","Summoner","Paladin"};
        public static void Build(GameObject character,int cls,bool male,int hair)
        {
            if(character==null)return; cls=Mathf.Clamp(cls,0,7);
            var old=character.transform.Find("ERO_V518_VisualKit"); if(old!=null)Object.Destroy(old.gameObject);
            var kit=new GameObject("ERO_V518_VisualKit_"+Names[cls]); kit.transform.SetParent(character.transform,false);
            SetMaterialColor(kit.transform, ClassColor(cls));
            switch(cls){
                case 0: Sword(kit.transform,.34f,.88f,.12f,.56f); Shield(kit.transform,-.34f,.82f,.44f); Shoulder(kit.transform,.29f,1.12f); break;
                case 1: Dagger(kit.transform,.27f,.77f,-32); Dagger(kit.transform,-.27f,.77f,32); Cloak(kit.transform,0,.91f); break;
                case 2: Bow(kit.transform,.40f,.92f); Quiver(kit.transform,-.28f,1.02f); break;
                case 3: Staff(kit.transform,.42f,.92f); Orb(kit.transform,.42f,1.36f,.09f); break;
                case 4: Staff(kit.transform,.42f,.92f); Orb(kit.transform,.42f,1.36f,.08f); Halo(kit.transform,0,1.58f); break;
                case 5: Fist(kit.transform,.27f,.74f); Fist(kit.transform,-.27f,.74f); Belt(kit.transform,0,.72f); break;
                case 6: Book(kit.transform,.35f,.96f); Orb(kit.transform,-.31f,1.12f,.10f); Orb(kit.transform,.31f,1.34f,.07f); Halo(kit.transform,0,1.58f); break;
                case 7: Sword(kit.transform,.36f,.89f,.14f,.60f); Shield(kit.transform,-.36f,.82f,.46f); Halo(kit.transform,0,1.60f); break;
            }
            ApplyMaterials(kit.transform,ClassColor(cls));
        }
        static void ApplyMaterials(Transform root,Color color){var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");var mat=new Material(shader);mat.name="ERO_V518_"+root.name+"_Material";mat.color=color;foreach(var r in root.GetComponentsInChildren<Renderer>(true))r.sharedMaterial=mat;}
        static Color ClassColor(int cls){switch(cls){case 0:return new Color(.18f,.38f,.72f);case 1:return new Color(.32f,.12f,.46f);case 2:return new Color(.18f,.55f,.28f);case 3:return new Color(.45f,.20f,.80f);case 4:return new Color(.95f,.72f,.22f);case 5:return new Color(.72f,.26f,.16f);case 6:return new Color(.55f,.16f,.78f);default:return new Color(.82f,.62f,.18f);}}
        static void SetMaterialColor(Transform root,Color color){/* applied after primitives are created */}
        static GameObject P(Transform p,string n,PrimitiveType t,Vector3 pos,Vector3 scale,Vector3 rot=default){var g=GameObject.CreatePrimitive(t);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.transform.localEulerAngles=rot;var c=g.GetComponent<Collider>();if(c)Object.Destroy(c);return g;}
        static void Sword(Transform p,float x,float y,float sx,float sy){P(p,"Sword",PrimitiveType.Cube,new Vector3(x,y,.06f),new Vector3(sx,sy,.06f),new Vector3(0,0,-12));P(p,"SwordGrip",PrimitiveType.Cylinder,new Vector3(x,y-sy*.58f,.06f),new Vector3(.04f,.10f,.04f));}
        static void Dagger(Transform p,float x,float y,float r){P(p,"Dagger",PrimitiveType.Cube,new Vector3(x,y,.08f),new Vector3(.065f,.27f,.045f),new Vector3(0,0,r));}
        static void Staff(Transform p,float x,float y){P(p,"Staff",PrimitiveType.Cylinder,new Vector3(x,y,.04f),new Vector3(.035f,.48f,.035f));}
        static void Bow(Transform p,float x,float y){
            // Unity 6 PrimitiveType has no Torus. Build a lightweight bow from three cubes.
            P(p,"BowUpper",PrimitiveType.Cube,new Vector3(x-.075f,y+.12f,.04f),new Vector3(.035f,.15f,.035f),new Vector3(0,0,-25));
            P(p,"BowLower",PrimitiveType.Cube,new Vector3(x-.075f,y-.12f,.04f),new Vector3(.035f,.15f,.035f),new Vector3(0,0,25));
            P(p,"BowGrip",PrimitiveType.Cube,new Vector3(x,y,.04f),new Vector3(.045f,.13f,.05f));
            P(p,"BowString",PrimitiveType.Cube,new Vector3(x+.055f,y,.04f),new Vector3(.012f,.26f,.012f));
        }
        static void Shield(Transform p,float x,float y,float s){P(p,"Shield",PrimitiveType.Sphere,new Vector3(x,y,.03f),new Vector3(s,s,.10f));}
        static void Shoulder(Transform p,float x,float y){P(p,"Shoulder",PrimitiveType.Sphere,new Vector3(x,y,.02f),new Vector3(.24f,.13f,.20f));}
        static void Cloak(Transform p,float x,float y){P(p,"Cloak",PrimitiveType.Cube,new Vector3(x,y,-.06f),new Vector3(.45f,.68f,.07f));}
        static void Quiver(Transform p,float x,float y){P(p,"Quiver",PrimitiveType.Cylinder,new Vector3(x,y,-.10f),new Vector3(.08f,.22f,.08f),new Vector3(0,0,-15));}
        static void Fist(Transform p,float x,float y){P(p,"Gauntlet",PrimitiveType.Sphere,new Vector3(x,y,.10f),new Vector3(.13f,.10f,.13f));}
        static void Belt(Transform p,float x,float y){P(p,"Belt",PrimitiveType.Cube,new Vector3(x,y,.02f),new Vector3(.56f,.09f,.15f));}
        static void Book(Transform p,float x,float y){P(p,"Grimoire",PrimitiveType.Cube,new Vector3(x,y,.08f),new Vector3(.20f,.25f,.06f),new Vector3(0,0,-12));}
        static void Orb(Transform p,float x,float y,float s){P(p,"MagicOrb",PrimitiveType.Sphere,new Vector3(x,y,.04f),new Vector3(s,s,s));}
        static void Halo(Transform p,float x,float y){P(p,"HolyHalo",PrimitiveType.Cylinder,new Vector3(x,y,-.02f),new Vector3(.20f,.025f,.20f));}
    }
}
