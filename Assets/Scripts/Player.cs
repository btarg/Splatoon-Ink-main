using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    PlayerControls playerInput;
    public List<Transform> splatPositions;
    public bool canFire = true;

    [SerializeField] GameObject splatColliderPrefab;
    [SerializeField] GameObject inkParticleParent;
    ParticleSystem inkParticle;

    [Header("Player")]
    public Color paintColor;

    [Header("Weapons and ammo")]
    public List<WeaponScriptableObject> weapons;
    public WeaponScriptableObject currentWeapon;
    public int currentAmmo = 100;
    GameObject spawnedWeapon;
    public int currentWeaponIndex = 0;
    bool isFiring = false;
    bool isDraining = false;
    public TextMeshProUGUI ammoText;

    Camera cam;
    GameObject playerObject;

    // Start is called before the first frame update
    void Start()
    {
        playerObject = this.transform.parent.gameObject;

        playerInput = new PlayerControls();
        playerInput.Enable();

        cam = this.GetComponent<Camera>();
        playerInput.Gameplay.Shoot.started += OnStartShoot;
        playerInput.Gameplay.Shoot.canceled += OnStopShoot;
        playerInput.Gameplay.CycleWeapons.performed += OnCycleWeapons;

    }

    private void Awake() {
        SwitchWeapon(currentWeaponIndex);
    }

    public void OnCycleWeapons(InputAction.CallbackContext value) {
        if (currentWeaponIndex == weapons.Count - 1) {
            currentWeaponIndex = 0;
        } else {
            currentWeaponIndex += 1;
        }
    }

    void SwitchWeapon(int index) {
        
        if (spawnedWeapon) {
            Destroy(spawnedWeapon);
        }

        currentWeapon = weapons.ToArray()[index];

        // Spawn the weapon object as a parent
        spawnedWeapon = Instantiate(currentWeapon.weaponPrefab);
        spawnedWeapon.transform.parent = gameObject.transform;

        ParticlesController controller = spawnedWeapon.GetComponentInChildren<ParticlesController>();
        controller.enabled = true;
        controller.player = this;

        inkParticleParent = controller.gameObject;
        inkParticle = inkParticleParent.GetComponent<ParticleSystem>();

        // Set everything to the player colour
        foreach (Renderer rend in inkParticleParent.GetComponentsInChildren<Renderer>()) {
            rend.material.color = paintColor;
        }
    

        currentWeaponIndex = index;

    }

    private void Update()
    {
        if (currentWeaponIndex != weapons.IndexOf(currentWeapon)) {
            SwitchWeapon(currentWeaponIndex);
        }

        ammoText.text = "Ammo: " + currentAmmo;

        if (playerObject.GetComponent<PlayerMultiCollision>().isCollidingWithFriendlyPaint())
            Debug.Log("fast!");
        else if (playerObject.GetComponent<PlayerMultiCollision>().isCollidingWithEnemyPaint())
            Debug.Log("slow!");

        // Move weapon in front of camera
        Vector3 inFrontOfCamera = cam.transform.position + cam.transform.forward * 1.5f;
        spawnedWeapon.transform.position = inFrontOfCamera;
        spawnedWeapon.transform.rotation = cam.transform.rotation;

        if (!isDraining && isFiring) {
            StartCoroutine(DrainAmmo());
        }
            

    }

    IEnumerator DrainAmmo() {

        // Keep draining ammo when in full auto mode
        if (currentAmmo >= currentWeapon.ammoUsedPerShot) {
            isDraining = true;
            currentAmmo -= currentWeapon.ammoUsedPerShot;
            yield return new WaitForSeconds(0.1f);
            isDraining = false;
        } else {
            isFiring = false;
            inkParticle.Stop();
        }
    }

    public void OnStartShoot(InputAction.CallbackContext value)
    {
        if (!canFire || currentAmmo < currentWeapon.ammoUsedPerShot) {
            return;
        }

        inkParticle.Play();

        if (currentWeapon.fullAuto) {
            isFiring = true;
        } else {
            currentAmmo -= currentWeapon.ammoUsedPerShot;
            StartCoroutine(WeaponCooldown());
        }
            
    }
    public void OnStopShoot(InputAction.CallbackContext value)
    {
        isFiring = false;
        inkParticle.Stop();
    }

    IEnumerator WeaponCooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(1);
        canFire = true;
    }

    private void OnDrawGizmos() {
        foreach (Transform t in splatPositions) {
            Gizmos.DrawCube(t.position, t.lossyScale);
        }
    }
}
