using UnityEngine;

namespace EternalRealmsOnline.V536
{
    public sealed class V536ProceduralAnimator : MonoBehaviour
    {
        Transform[] arms, legs, hair;
        Vector3[] armPos, legPos, hairPos;
        float phase;
        void Awake()
        {
            arms = FindParts("Arm"); legs = FindParts("Leg"); hair = FindParts("HairLock");
            armPos = Save(arms); legPos = Save(legs); hairPos = Save(hair);
            phase = Random.Range(0f, 10f);
        }
        void Update()
        {
            float t = Time.time * 2.0f + phase;
            float idle = Mathf.Sin(t) * 2.5f;
            for (int i=0;i<arms.Length;i++) arms[i].localRotation = Quaternion.Euler(0,0,(i%2==0?1:-1)*(10f+idle));
            for (int i=0;i<legs.Length;i++) legs[i].localRotation = Quaternion.Euler((i%2==0?1:-1)*Mathf.Sin(t*.55f)*3f,0,0);
            for (int i=0;i<hair.Length;i++) hair[i].localRotation = Quaternion.Euler(10f,0,(i-1.5f)*10f+Mathf.Sin(t*.7f)*3f);
        }
        Transform[] FindParts(string token)
        {
            var list=new System.Collections.Generic.List<Transform>();
            foreach(var t in GetComponentsInChildren<Transform>(true)) if(t.name.StartsWith(token)) list.Add(t);
            return list.ToArray();
        }
        Vector3[] Save(Transform[] ts){var a=new Vector3[ts.Length];for(int i=0;i<ts.Length;i++)a[i]=ts[i].localPosition;return a;}
    }
}
