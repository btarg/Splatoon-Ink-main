using System.Collections.Generic;
using UnityEngine;

public class PaintProjectile : MonoBehaviour {
    
    public float damage;
    public List<Transform> splatPositions;
    public Color paintColor;
    Object splatPrefab;

    private void Start() {
        splatPrefab = Resources.Load("Prefabs/SplatTrigger", typeof(GameObject));
    }
    
    void OnCollisionEnter(Collision other) {
        
        foreach (var contact in other.contacts)
        {
            CreateSplatTrigger(contact);
        }
        Destroy(gameObject);
    
    }

    public void CreateSplatTrigger(ContactPoint point)
    {
        Vector3 pos = point.point;
        // Don't spawn a new one if there is already one nearby
        foreach (Transform oldSplat in splatPositions)
        {
            var distance = Vector3.Distance(oldSplat.position, pos);
            if (distance < 0.75)
            {
                SplatTrigger trigger = oldSplat.gameObject.GetComponentInChildren<SplatTrigger>();
                // Set paint colour of enemy to the friendly player's colour
                if (!trigger.isFriendly(paintColor)) {
                    trigger.paintColor = paintColor;
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
        var q = Quaternion.FromToRotation(splat.transform.up, point.normal);
        splat.transform.rotation = q * splat.transform.rotation;
        splat.transform.position += (point.normal * 0.25f);
        splat.GetComponentInChildren<SplatTrigger>().paintColor = paintColor;

        splatPositions.Add(splat.transform);
        Debug.Log("splatted with color: " + paintColor);

    }
}