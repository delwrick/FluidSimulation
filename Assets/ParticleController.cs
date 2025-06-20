using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public Vector2 velocity;
    public float speed = 5f;
    public Rect bounds;
    public float radius = 0.5f;
    [Range(0f, 1f)]
    public float elasticity = 1f; // 1 = perfectly elastic, 0 = completely inelastic
    public Vector2 gravity = new Vector2(0, -9.81f); // adjustable!
    public System.Action<float> onWallCollision;
    public void Initialize(Vector2 direction, float speedMultiplier, Rect simBounds)
    {
        velocity = direction.normalized * speed * speedMultiplier;
        bounds = simBounds;
    }

    public void Simulate(float dt)
    {
        // Gravity & speed cap
        velocity += gravity * dt;
        velocity  = Vector2.ClampMagnitude(velocity, 10000f);

        Vector2 pos = (Vector2)transform.position + velocity * dt;

        // X bounce
        if (pos.x < bounds.xMin + radius || pos.x > bounds.xMax - radius)
        {
            float impulse = Mathf.Abs(velocity.x); // change in momentum
            velocity.x *= -elasticity;
            pos.x = Mathf.Clamp(pos.x, bounds.xMin + radius, bounds.xMax - radius);
            onWallCollision?.Invoke(impulse);
        }

        // Y bounce
        if (pos.y < bounds.yMin + radius || pos.y > bounds.yMax - radius)
        {
            float impulse = Mathf.Abs(velocity.y);
            velocity.y *= -elasticity;
            pos.y = Mathf.Clamp(pos.y, bounds.yMin + radius, bounds.yMax - radius);
            onWallCollision?.Invoke(impulse);
        }

        transform.position = pos;

        // Ground settle & friction
/*        bool onFloor = transform.position.y <= bounds.yMin + radius + 0.01f;
        if (onFloor && Mathf.Abs(velocity.y) < 0.05f)
            velocity = Vector2.zero;
        if (onFloor && velocity != Vector2.zero)
            velocity.x *= 0.95f;        // horizontal friction */
    }

    public void ResolveCollision(ParticleController other)
    {
        Vector2 delta = other.transform.position - transform.position;
        float dist = delta.magnitude;
        float minDist = radius + other.radius;

        if (dist == 0 || dist >= minDist) return; // No collision or already resolved

        Vector2 normal = delta.normalized;
        Vector2 tangent = new Vector2(-normal.y, normal.x);

        // Decompose velocities into normal and tangent components
        float v1n = Vector2.Dot(velocity, normal);
        float v2n = Vector2.Dot(other.velocity, normal);

        float v1t = Vector2.Dot(velocity, tangent);
        float v2t = Vector2.Dot(other.velocity, tangent);

        // Swap normal components (equal mass elastic collision)
        float v1nAfter = v2n;
        float v2nAfter = v1n;

        // Reconstruct velocities
        velocity = (v1nAfter * normal) + (v1t * tangent);
        other.velocity = (v2nAfter * normal) + (v2t * tangent);

        // Slight overlap correction (optional, helps avoid sticking)
        float penetration = minDist - dist;
        Vector2 correction = normal * (penetration / 2f);
        transform.position -= (Vector3)correction;
        other.transform.position += (Vector3)correction;
    }

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            radius = sr.bounds.extents.x; // or use extents.x if circular
        }
    }
}
