using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    [Header("Particle settings")]
    public ParticleController particlePrefab;
    public int particleCount = 100;
    public int maxParticleCount = 10000; // max particles to spawn at once
    private List<ParticleController> particlePool = new();
    public List<ParticleController> activeParticles = new();
    public float jitterRadius = 0.15f;   // small random offset
    public float speedMultiplier = 1f;
    private Vector2 previousAreaSize;

    [Header("Spawn area (local space)")]
    public Vector2 areaSize = new Vector2(8f, 4f);
    Dictionary<Vector2Int, List<ParticleController>> spatialGrid = new();
    float GetPrefabRadius()
    {
        if (particlePrefab == null) return 0.1f;

        var sr = particlePrefab.GetComponent<SpriteRenderer>();
        return sr ? sr.bounds.extents.x : 0.1f;
    }
    float cellSize; // ideally ~2x particle radius


    void Start()
    {
        float prefabRadius = GetPrefabRadius();
        cellSize = prefabRadius * 2.1f;    // 2 × radius  (the 0.1 keeps a tiny gap)

        CreateParticlePool();
        ActivateParticles(particleCount);
    }

    void CreateParticlePool()
    {
        for (int i = 0; i < maxParticleCount; i++)
        {
            var p = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity, transform);
            p.gameObject.SetActive(false);
            particlePool.Add(p);
        }
    }
    public void ActivateParticles(int count)
    {
        activeParticles.Clear(); // Reset active list

        int columns = Mathf.CeilToInt(Mathf.Sqrt(count));
        int rows = Mathf.CeilToInt((float)count / columns);
        float spacingX = areaSize.x / columns;
        float spacingY = areaSize.y / rows;
        Vector2 origin = (Vector2)transform.position - areaSize / 2f;

        Rect simBounds = new Rect(
            transform.position.x - areaSize.x / 2f,
            transform.position.y - areaSize.y / 2f,
            areaSize.x,
            areaSize.y
        );

        for (int i = 0; i < particlePool.Count; i++)
        {
            var p = particlePool[i];
            if (i < count)
            {
                Vector2 pos = origin + new Vector2((i % columns + 0.5f) * spacingX, (i / columns + 0.5f) * spacingY);
                p.transform.position = pos;
                p.gameObject.SetActive(true);
                Vector2 dir = Random.insideUnitCircle.normalized;
                p.Initialize(dir, speedMultiplier, simBounds);
                activeParticles.Add(p);
            }
            else
            {
                p.gameObject.SetActive(false);
            }
        }
    }
    Vector2Int WorldToCell(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x / cellSize),
            Mathf.FloorToInt(worldPos.y / cellSize)
        );
    }

    void BuildSpatialGrid()
    {
        spatialGrid.Clear();

        foreach (var p in activeParticles)
        {
            Vector2Int cell = WorldToCell(p.transform.position);
            if (!spatialGrid.TryGetValue(cell, out var list))
            {
                list = new List<ParticleController>(4); // tiny default capacity
                spatialGrid[cell] = list;
            }
            list.Add(p);
        }
    }


    void OnDrawGizmosSelected()
    {
        if (particlePrefab == null) return;

        int columns = Mathf.CeilToInt(Mathf.Sqrt(particleCount));
        int rows = Mathf.CeilToInt((float)particleCount / columns);

        float spacingX = areaSize.x / columns;
        float spacingY = areaSize.y / rows;

        Vector2 origin = (Vector2)transform.position - areaSize / 2f;

        // 🌀 Calculate radius from prefab's SpriteRenderer
        float particleRadius = 0.1f;
        var sr = particlePrefab.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            particleRadius = sr.bounds.extents.x;
        }

        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.6f);

        int shown = 0;
        for (int y = 0; y < rows && shown < particleCount; y++)
        {
            for (int x = 0; x < columns && shown < particleCount; x++)
            {
                Vector2 pos = origin + new Vector2((x + 0.5f) * spacingX, (y + 0.5f) * spacingY);
                Gizmos.DrawSphere(pos, particleRadius);
                shown++;
            }
        }

        // 🧱 Draw spawn area boundary
        Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.4f);
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
    void ResolveAllCollisions()
    {
        foreach (var cell in spatialGrid)
        {
            foreach (var a in cell.Value)
            {
                Vector2Int c = WorldToCell(a.transform.position);

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector2Int neighbor = c + new Vector2Int(dx, dy);

                        if (!spatialGrid.TryGetValue(neighbor, out var neighbors)) continue;

                        foreach (var b in neighbors)
                        {
                            if (a == b) continue;
                            a.ResolveCollision(b);
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (areaSize != previousAreaSize)
        {
            UpdateSimulationBounds();
            previousAreaSize = areaSize;

            // Optional: Update pressure tracker too
            Object.FindFirstObjectByType<PressureTracker>()?.UpdatePerimeter(areaSize.x, areaSize.y);
        }
        float dt = Time.deltaTime;

        foreach (var p in activeParticles)
        {
            p.Simulate(dt);
        }

        BuildSpatialGrid();
        ResolveAllCollisions();
    }
    public void UpdateSimulationBounds()
    {
        Rect simBounds = new Rect(
            transform.position.x - areaSize.x / 2f,
            transform.position.y - areaSize.y / 2f,
            areaSize.x,
            areaSize.y
        );

        foreach (var p in activeParticles)
        {
            p.bounds = simBounds;
        }

        Debug.Log($"[Spawner] Bounds updated to: {areaSize}");
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }

}