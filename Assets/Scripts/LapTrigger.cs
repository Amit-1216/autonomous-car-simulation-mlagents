using UnityEngine;

public class LapTrigger : MonoBehaviour
{
    [Header("References")]
    public CarHUD carHUD;

    // Prevent double-counting if car lingers on trigger
    private float cooldown = 2f;
    private float lastLapTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastLapTime < cooldown) return;

        if (other.CompareTag("Player"))
        {
            carHUD.AddLap();
            lastLapTime = Time.time;
            Debug.Log("Lap counted!");
        }
    }
}