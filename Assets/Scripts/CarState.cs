using UnityEngine;

public class CarState : MonoBehaviour
{
    [Tooltip("Maximum detection distance")]
    public float rayDistance = 35f;

    [Tooltip("Layers that count as obstacles (exclude the car's own layer)")]
    public LayerMask obstacleLayers;

    // Normalised distances for the 5 rays: [0]=front, [1]=left30, [2]=right30, [3]=left60, [4]=right60
    [HideInInspector] public float[] rayDistances = new float[5];

    void FixedUpdate()
    {
        // Ray origin – slightly above the car's base to avoid self?collision
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // Directions: front, left -30°, right +30°, left -60°, right +60°
        Vector3[] directions =
        {
            transform.forward,                                  // 0°  (forward)

            Quaternion.Euler(0, -30f, 0) * transform.forward,   // left 30°
            Quaternion.Euler(0,  30f, 0) * transform.forward,   // right 30°

            Quaternion.Euler(0, -60f, 0) * transform.forward,   // left 60°
            Quaternion.Euler(0,  60f, 0) * transform.forward,   // right 60°

            Quaternion.Euler(0, -90f, 0) * transform.forward,   // left 90° (perpendicular)
            Quaternion.Euler(0,  90f, 0) * transform.forward    // right 90° (perpendicular)
        };

        for (int i = 0; i < directions.Length; i++)
        {
            if (Physics.Raycast(origin, directions[i], out RaycastHit hit, rayDistance, obstacleLayers))
            {
                // Hit – store normalised distance and draw red ray
                rayDistances[i] = hit.distance / rayDistance;
                Debug.DrawRay(origin, directions[i] * hit.distance, Color.red, 0.1f);
            }
            else
            {
                // No hit – store 1.0 (clear) and draw green ray
                rayDistances[i] = 1f;
                Debug.DrawRay(origin, directions[i] * rayDistance, Color.green, 0.1f);
            }
        }
    }
}