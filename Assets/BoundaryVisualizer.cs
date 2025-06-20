using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BoundaryVisualizer : MonoBehaviour
{
    public ParticleSpawner spawner;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.positionCount = 4;
        lineRenderer.widthMultiplier = 2f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
    }

    void Update()
    {
        if (spawner == null) return;

        Vector2 size = spawner.areaSize;
        Vector3 center = spawner.transform.position;

        Vector3 bottomLeft  = center + new Vector3(-size.x / 2f, -size.y / 2f, 0);
        Vector3 topLeft     = center + new Vector3(-size.x / 2f,  size.y / 2f, 0);
        Vector3 topRight    = center + new Vector3( size.x / 2f,  size.y / 2f, 0);
        Vector3 bottomRight = center + new Vector3( size.x / 2f, -size.y / 2f, 0);

        lineRenderer.SetPosition(0, bottomLeft);
        lineRenderer.SetPosition(1, topLeft);
        lineRenderer.SetPosition(2, topRight);
        lineRenderer.SetPosition(3, bottomRight);
    }
}
