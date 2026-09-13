using UnityEngine;

namespace EternalRealmsOnline.V1
{
    public sealed class EROV1ThirdPersonController : MonoBehaviour
    {
        public float moveSpeed = 4.2f;
        public float cameraDistance = 6.2f;
        public float rotationSpeed = 12f;

        void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(h, 0f, v);
            if (input.sqrMagnitude > 1f) input.Normalize();

            if (input.sqrMagnitude > 0.001f)
            {
                transform.position += input * moveSpeed * Time.deltaTime;
                Quaternion desired = Quaternion.LookRotation(input, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desired, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
