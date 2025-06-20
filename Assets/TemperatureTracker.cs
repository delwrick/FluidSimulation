using UnityEngine;
using TMPro;

public class TemperatureTracker : MonoBehaviour
{
    public ParticleSpawner spawner;
    public TextMeshProUGUI uiText;

    private float currentTemperature;

    void Update()
    {
        if (spawner == null || spawner.activeParticles.Count == 0) return;

        float totalKinetic = 0f;

        foreach (var p in spawner.activeParticles)
        {
            totalKinetic += p.velocity.sqrMagnitude;
        }

        currentTemperature = totalKinetic / spawner.activeParticles.Count;

        if (uiText != null)
        {
            uiText.text = $"Temperature: {currentTemperature:F2}";
        }
    }

    public float GetTemperature()
    {
        return currentTemperature;
    }
}
