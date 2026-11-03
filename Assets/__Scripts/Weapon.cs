using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


/// <summary
/// This is an enum of the various possible weapon types.
/// It also includes a "shield" type to allow a shield PowerUp.
/// Items marked [NI] below are Not Implemented in this book.
/// </summary
public enum eWeaponType
{
    none,       // The default / no weapon
    blaster,    // A simple blaster
    spread,     // Multiple shots simultaneously
    phaser,     // [NI] Shots that move in waves
    missile,    // [NI] Homing missiles

    laser,      // [NI] Damage over time
    beam,      // [NI] Damage over time
    shield      // Raise shieldLevel
}


/// <summary
/// The WeaponDefinition class allows you to set the properties
///   of a specific weapon in the Inspector. The Main class has
///   an array of WeaponDefinitions that makes this possible.
/// </summary
[System.Serializable]                                                         // a
public class WeaponDefinition
{                                               // b
    public eWeaponType type = eWeaponType.none;
    [Tooltip("Letter to show on the PowerUp Cube")]                           // c
    public string letter;
    [Tooltip("Color of PowerUp Cube")]
    public Color powerUpColor = Color.white;                           // d
    [Tooltip("Prefab of Weapon model that is attached to the Player Ship")]
    public GameObject weaponModelPrefab;
    [Tooltip("Prefab of projectile that is fired")]
    public GameObject projectilePrefab;
    [Tooltip("Color of the Projectile that is fired")]
    public Color projectileColor = Color.white;                        // d
    [Tooltip("Damage caused when a single Projectile hits an Enemy")]
    public float damageOnHit = 0;
    [Tooltip("Damage caused per second by the Laser [Not Implemented]")]
    public float damagePerSec = 0;
    [Tooltip("Seconds to delay between shots")]
    public float delayBetweenShots = 0;
    [Tooltip("Velocity of individual Projectiles")]
    public float velocity = 50;
    [Tooltip("Amount of projectiles fired at once")]
    public float projectileCount = 1;
    [Tooltip("Total spread of projectiles")]
    public float projectileSpread = 0;
    [Tooltip("Seconds to delay between Weapons of different orders")]
    public float fireOrderDelay = 0;
    [Tooltip("Extra bullets to fire as Main Weapon")]
    public float mainWeaponBonus = 1;

}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Inscribed")]
    [SerializeField]
    private float rotYaw = 0;
    [SerializeField]
    private int weaponOrder = 0;

    [Header("Dynamic")]                                                        // a
    [SerializeField]                                                           // a
    [Tooltip("Setting this manually while playing does not work properly.")]   // a
    private eWeaponType _type = eWeaponType.none;
    public WeaponDefinition def;
    public float nextShotTime = 0; // Time the Weapon will fire next
    public bool isMainWeapon = false;

    private GameObject weaponModel;
    private Transform shotPointTrans;

    public float WeaponDelay { get { return weaponOrder * def.fireOrderDelay; } }

    void Start()
    {
        // Set up PROJECTILE_ANCHOR if it has not already been done
        if (PROJECTILE_ANCHOR == null)
        {                                       // b
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild(0);                              // c

        // Call SetType() for the default _type set in the Inspector
        SetType(_type);                                                      // d

        // Find the fireEvent of a Hero Component in the parent hierarchy
        Hero hero = GetComponentInParent<Hero>();                              // e
        if (hero != null) hero.fireEvent += Fire;
    }

    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;
        if (type == eWeaponType.none)
        {                                       // f
            this.gameObject.SetActive(false);
            nextShotTime = 0;
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }
        // Get the WeaponDefinition for this type from Main
        def = Main.GET_WEAPON_DEFINITION(_type);
        // Destroy any old model and then attach a model for this weapon     // g
        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        //nextShotTime = 0; // You can fire immediately after _type is set.    // h
    }

    public bool SetLeadShotTime(float time)
    {
        float oldNextShotTime = nextShotTime;
        float delayedShotTime = time + WeaponDelay;
        nextShotTime = Mathf.Max(delayedShotTime, oldNextShotTime);
        return delayedShotTime > oldNextShotTime;
    }

    public void SetDelayedShotTime(float time)
    {
        float oldNextShotTime = nextShotTime;
        float delayedShotTime = time + WeaponDelay;

        if (oldNextShotTime > delayedShotTime + def.delayBetweenShots)
            Debug.Log($"Weapon ${transform.parent.name} early ({oldNextShotTime:F4} > {delayedShotTime:F4} + {def.delayBetweenShots:F2})");

        while (oldNextShotTime > delayedShotTime + def.delayBetweenShots)
        {
            delayedShotTime += def.delayBetweenShots;
        }
        nextShotTime = delayedShotTime;
    }

    private void Fire()
    {
        // If this.gameObject is inactive, return
        if (!gameObject.activeInHierarchy) return;                         // i
        // If it hasn’t been enough time between shots, return
        if (Time.time < nextShotTime) return;                              // j

        ProjectileHero p;
        Vector3 vel = Vector3.up * def.velocity;

        float projCount = def.projectileCount + (isMainWeapon ? def.mainWeaponBonus : 0);
        float spreadOffset = (projCount > 1) ? -def.projectileSpread * 0.5f : 0;
        float spreadIncrement = def.projectileSpread / Mathf.Max(projCount - 1, 1);

        for (int i = 0; i < projCount; i++)
        {
            switch (type)
            {                                                      // k
                case eWeaponType.blaster:
                    p = FireProjStraight(vel, spreadOffset);
                    break;

                case eWeaponType.spread:                                         // l
                    p = FireProjAngled(vel, spreadOffset + rotYaw);
                    break;

                case eWeaponType.missile:
                    p = FireProjAngled(vel, spreadOffset + rotYaw);
                    break;

                case eWeaponType.phaser:
                    p = FireProjStraight(vel, spreadOffset);
                    if (p is ProjPhaserHero phaser)
                    {
                        if (rotYaw < 0) phaser.waveWidth *= -1;
                        if (i % 2 != 0) phaser.waveWidth *= -1;
                    }
                    break;

                case eWeaponType.laser:
                    p = FireProjStraight(vel, spreadOffset);
                    float angle = rotYaw * 0.5f;
                    p.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
                    p.vel = p.transform.rotation * p.vel;
                    break;

                case eWeaponType.beam:
                    p = FireProjStraight(vel, spreadOffset);
                    if (p is ProjBeamHero beam)
                    {
                        p.vel = Vector3.zero;
                        Vector3 offset = new(spreadOffset, 0, 0);
                        beam.SetParent(shotPointTrans, offset);
                    }
                    break;
            }
            spreadOffset += spreadIncrement;
        }
    }

    private ProjectileHero FireProjStraight(Vector3 vel, float offset = 0)
    {
        ProjectileHero p = MakeProjectile();
        Vector3 pPos = p.transform.position;
        pPos.x += offset;
        p.transform.position = pPos;
        p.vel = vel;
        return p;
    }

    private ProjectileHero FireProjAngled(Vector3 vel, float angle = 0)
    {
        ProjectileHero p = MakeProjectile();
        p.transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
        p.vel = p.transform.rotation * vel;
        return p;
    }

    private ProjectileHero MakeProjectile()
    {                                 // m
        GameObject go;
        go = Instantiate<GameObject>(def.projectilePrefab, PROJECTILE_ANCHOR); // n
        ProjectileHero p = go.GetComponent<ProjectileHero>();

        Vector3 pos = shotPointTrans.position;
        pos.z = 0;                                                            // o
        p.transform.position = pos;

        p.type = type;
        nextShotTime = Time.time + def.delayBetweenShots;                    // p
        return (p);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 dir = Quaternion.AngleAxis(rotYaw, Vector3.back) * Vector3.up;
        Vector3 to = transform.position + dir;
        Gizmos.DrawLine(transform.position, to);
    }
}


