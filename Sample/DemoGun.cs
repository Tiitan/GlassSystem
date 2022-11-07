using UnityEngine;

namespace GlassSystem.Sample
{
    public class DemoGun : MonoBehaviour
    {
        public float impactForce = 1000f;
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                var raycastDirection = transform.TransformDirection(Vector3.forward);
                if (Physics.Raycast(transform.position, raycastDirection, out hit, Mathf.Infinity))
                {
                    Debug.DrawRay(transform.position, raycastDirection * hit.distance, Color.yellow, 10);
                    var glass = hit.transform.gameObject.GetComponent<Glass>();
                    if (glass is not null)
                        glass.Break(hit.point, raycastDirection * impactForce);
                }
            }
        }

    }
}
