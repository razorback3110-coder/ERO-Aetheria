using UnityEngine;

namespace EternalRealmsOnline.V1
{
    public sealed class EROV1CinematicCamera : MonoBehaviour
    {
        Camera cam;
        Transform target;

        void Start()
        {
            cam = GetComponent<Camera>();
            var p = GameObject.Find("V1_Player_NoHealNoGame");
            if (p != null) target = p.transform;
        }

        void LateUpdate()
        {
            if (target == null)
            {
                var fallback = GameObject.Find("V1_Player_NoHealNoGame");
                if (fallback != null) target = fallback.transform;
                if (target == null) return;
            }

            Vector3 desired = target.position + new Vector3(0f, 3.1f, -6.2f);
            transform.position = Vector3.Lerp(transform.position, desired, 5f * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 1.0f);
        }
    }
}
