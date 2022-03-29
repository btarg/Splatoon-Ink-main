using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/WeaponScriptableObject", order = 1)]
public class WeaponScriptableObject : ScriptableObject
{
    public string friendlyName;
    public GameObject weaponPrefab;
    public float fireCooldown;
    public int ammoUsedPerShot;
    public float damagePerProjectileHit;
    public bool fullAuto;
}