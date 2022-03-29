using UnityEngine;

public class PlayerMultiCollision : MonoBehaviour
{
    public int numCollisionsFriendly = 0;
    public int numCollisionsEnemy = 0;

    public bool isCollidingWithFriendlyPaint() {
        return numCollisionsFriendly > 0;
    }
    public bool isCollidingWithEnemyPaint() {
        return numCollisionsEnemy > 0;
    }
}
