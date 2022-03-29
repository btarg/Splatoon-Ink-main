using UnityEngine;

public class SplatTrigger : MonoBehaviour
{
    public Color paintColor;

    public bool isFriendly(Color comparedTo) {
        return paintColor.Equals(comparedTo);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            PlayerMultiCollision mc = other.gameObject.GetComponent<PlayerMultiCollision>();
            ParticlesController pc = other.gameObject.GetComponentInChildren<ParticlesController>();

            if (isFriendly(pc.paintColor())) {
                mc.numCollisionsFriendly += 1;
            } else {
                mc.numCollisionsEnemy += 1;
            }

        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Player") {
            PlayerMultiCollision mc = other.gameObject.GetComponent<PlayerMultiCollision>();
            ParticlesController pc = other.gameObject.GetComponentInChildren<ParticlesController>();

            if (isFriendly(pc.paintColor())) {
                mc.numCollisionsFriendly -= 1;
            } else {
                mc.numCollisionsEnemy -= 1;
            }
            
        }
    }
}
