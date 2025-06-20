using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PressureTracker : MonoBehaviour
{
    [Header("References")]
    public ParticleSpawner spawner;  // Assign in Inspector
    public TextMeshProUGUI uiText;              // Optional: assign a UI Text element

    [Header("Live Output")]
    public float pressurePerSecond = 0f;

    private float impulseSum = 0f;
    private float timer = 0f;
    private float containerPerimeter = 1f;  // Will be auto-calculated

    private bool listenersHooked = false;

    void Start()
    {
        // Calculate container perimeter from areaSize (width + height) × 2
        if (spawner != null)
        {
            Vector2 size = spawner.areaSize;
            containerPerimeter = 2f * (size.x + size.y);
            Debug.Log($"[PressureTracker] Container perimeter set to {containerPerimeter}");
        }
        else
        {
            Debug.LogError("[PressureTracker] Spawner reference is missing!");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // ⬇️ Hook into particles only once after they are spawned
        if (!listenersHooked && spawner != null && spawner.activeParticles.Count > 0)
        {
            foreach (var p in spawner.activeParticles)
            {
                p.onWallCollision += RecordImpulse;
            }

            listenersHooked = true;
            Debug.Log("[PressureTracker] Hooked into particle wall collision events!");
        }

        // ⬇️ Update pressure once per second
        if (timer >= 1f)
        {
            pressurePerSecond = impulseSum / containerPerimeter;

            if (uiText != null)
            {
                uiText.text = $"Pressure: {pressurePerSecond:F2}";
            }

            Debug.Log($"[PressureTracker] Pressure: {pressurePerSecond:F2}");

            // Reset for the next second
            impulseSum = 0f;
            timer = 0f;
        }
    }

    void RecordImpulse(float impulse)
    {
        impulseSum += impulse;
        // Debug.Log($"[PressureTracker] Recorded impulse: {impulse:F3}");
    }
    public void UpdatePerimeter(float width, float height)
    {
        containerPerimeter = 2f * (width + height);
        Debug.Log($"[PressureTracker] Perimeter updated: {containerPerimeter}");
    }

}
