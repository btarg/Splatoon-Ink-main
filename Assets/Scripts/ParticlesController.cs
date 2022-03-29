using System.Collections.Generic;
using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    Object splatPrefab;
    public Player player = null;
    float damagePerProjectileHit;

    public float minRadius = 0.05f;
    public float maxRadius = 0.2f;
    public float strength = 1;
    public float hardness = 1;
    [Space]
    ParticleSystem particles;
    List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        collisionEvents = new List<ParticleCollisionEvent>();
        splatPrefab = Resources.Load("Prefabs/SplatTrigger", typeof(GameObject));
    }

    private void Awake() {
        particles = GetComponent<ParticleSystem>();
    }

    public Color paintColor() {
        return player.paintColor;
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.GetComponentInChildren<Damageable>()) {
            // do damage to a player if there is one
            damagePerProjectileHit = player.currentWeapon.damagePerProjectileHit;
            var dmg = other.GetComponentInChildren<Damageable>();
            dmg.TakeDamage(player.currentWeapon.damagePerProjectileHit);
            return;
        }

        int numCollisionEvents = particles.GetCollisionEvents(other, collisionEvents);

        Paintable p = other.GetComponent<Paintable>();
        if (p != null)
        {
            for (int i = 0; i < numCollisionEvents; i++)
            {
                Vector3 pos = collisionEvents[i].intersection;
                float radius = Random.Range(minRadius, maxRadius);
                PaintManager.instance.paint(p, pos, radius, hardness, strength, player.paintColor);

                CreateSplatTrigger(collisionEvents[i]);
            }
        }
    }

    public void CreateSplatTrigger(ParticleCollisionEvent other)
    {
        Vector3 pos = other.intersection;
        // Don't spawn a new one if there is already one nearby
        foreach (Transform oldSplat in player.splatPositions)
        {
            var distance = Vector3.Distance(oldSplat.position, pos);
            if (distance < 0.75)
            {
                SplatTrigger trigger = oldSplat.gameObject.GetComponentInChildren<SplatTrigger>();
                // Set paint colour of enemy to the friendly player's colour
                if (!trigger.isFriendly(paintColor())) {
                    trigger.paintColor = paintColor();
                    Debug.Log("reset colour of " + trigger.gameObject);
                } else {
                    Debug.Log("friendly splat nearby already");
                }
                return;
            }
        }

        GameObject splat = Instantiate(splatPrefab) as GameObject;
        splat.transform.position = pos;

        // Align collider to walls
        var q = Quaternion.FromToRotation(splat.transform.up, other.normal);
        splat.transform.rotation = q * splat.transform.rotation;
        splat.transform.position += (other.normal * 0.25f);
        splat.GetComponentInChildren<SplatTrigger>().paintColor = paintColor();

        player.splatPositions.Add(splat.transform);
        Debug.Log("splatted with color: " + paintColor());

    }
}
