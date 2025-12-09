using UnityEngine;

public class targetmanager : MonoBehaviour
{
        [Header("Scene Objects")]
        public GameObject orb_middle;

        [Header("Target Prefab")]
        public GameObject targetPrefab;

        [Header("Circle Layout Settings")]
        public float circleRadius = 5f;
        public Color normalColor = Color.blue;
        public Color delayedColor = Color.red;

        private GameObject activeTarget;

        public void ShowMiddleOrb()
        {
                orb_middle.SetActive(true);
        }

        public void ShowCircularTarget(int position, bool isDelayed)
        {
                // Destroy the previous target if it exists
                if (activeTarget != null)
                {
                        Destroy(activeTarget);
                }

                // Calculate the angle for the given position to be clockwise, with 0 at the top.
                // 12 o'clock (pos 0) is 90 degrees. 3 o'clock (pos 3) is 0 degrees.
                float angle = 90f - (position * (360f / 12f));

                // Calculate the position on the circle in the X-Y plane (vertical)
                float x = transform.position.x + circleRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = transform.position.y + circleRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
                // Keep the Z position the same as the manager's to form a vertical circle in front.
                float z = transform.position.z;
                
                Vector3 targetPosition = new Vector3(x, y, z);

                // Instantiate the prefab
                activeTarget = Instantiate(targetPrefab, targetPosition, Quaternion.identity);
                activeTarget.tag = "orb"; // Ensure the new target has the correct tag

                // Set the color based on the delay
                Renderer targetRenderer = activeTarget.GetComponent<Renderer>();
                if (targetRenderer != null)
                {
                        targetRenderer.material.color = isDelayed ? delayedColor : normalColor;
                }
        }

        public void HideAllOrbs()
        {
                orb_middle.SetActive(false);
                
                if (activeTarget != null)
                {
                        Destroy(activeTarget);
                        activeTarget = null;
                }
        }
}