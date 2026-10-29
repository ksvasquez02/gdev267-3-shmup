using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Hero : MonoBehaviour
{

    static public Hero S { get; private set; }  // Singleton property    // a

    [Header("Inscribed")]
    // These fields control the movement of the ship
    public float speed = 30;
    public float rollMult = -45;
    public float pitchMult = 30;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;
    public eWeaponType startingWeapon = eWeaponType.blaster;

    [Header("Dynamic")]
    [Range(0, 4)]
    [SerializeField]                                        // b
    private float _shieldLevel = 1;

    [Tooltip("This field holds a reference to the last triggering GameObject")]
    private GameObject lastTriggerGo = null;

    private bool wasFiringLastFrame = false;

    // Declare a new delegate type WeaponFireDelegate
    public delegate void WeaponFireDelegate();                                // a     // Create a WeaponFireDelegate event named fireEvent.
    public event WeaponFireDelegate fireEvent;



    void Awake()
    {
        if (S == null)
        {
            S = this; // Set the Singleton only if it’s null                  // c
        }
        else
        {
            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
        }
        //fireEvent += TempFire;

        // Reset the weapons to start _Hero with 1 blaster
        ClearWeapons();
        weapons[0].SetType(startingWeapon);
    }

    void Update()
    {
        // Pull in information from the Input class
        float hAxis = Input.GetAxis("Horizontal");                            // d
        float vAxis = Input.GetAxis("Vertical");                              // d

        // Change transform.position based on the axes
        Vector3 pos = transform.position;
        Vector2 vel = new Vector2(hAxis, vAxis);
        vel = Vector2.ClampMagnitude(vel, 1);
        pos.x += vel.x * speed * Time.deltaTime;
        pos.y += vel.y * speed * Time.deltaTime;
        transform.position = pos;

        // Rotate the ship to make it feel more dynamic                       // e
        transform.rotation = Quaternion.Euler(vAxis * pitchMult, hAxis * rollMult, 0);

        // Allow the ship to fire
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    TempFire();
        //}

        // Use the fireEvent to fire Weapons when the Spacebar is pressed.
        if (Input.GetAxis("Jump") == 1 && fireEvent != null)
        {
            if (!wasFiringLastFrame) CoordinateWeapons();
            fireEvent();
            wasFiringLastFrame = true;
        }
        else
        {
            wasFiringLastFrame = false;
        }

    }


    //void TempFire()
    //{
    //    GameObject projGO = Instantiate<GameObject>(projectilePrefab);
    //    projGO.transform.position = transform.position;
    //    Rigidbody rigidB = projGO.GetComponent<Rigidbody>();
    //    //rigidB.velocity = Vector3.up * projectileSpeed;

    //    ProjectileHero proj = projGO.GetComponent<ProjectileHero>();         // h
    //    proj.type = eWeaponType.blaster;
    //    float tSpeed = Main.GET_WEAPON_DEFINITION(proj.type).velocity;
    //    rigidB.velocity = Vector3.up * tSpeed;

    //}

    void OnTriggerEnter(Collider other)
    {
        Transform rootT = other.gameObject.transform.root;                    // a
        GameObject go = rootT.gameObject;
        //Debug.Log("Shield trigger hit by: " + go.gameObject.name);

        // Make sure it’s not the same triggering go as last time
        if (go == lastTriggerGo) return;                                    // c
        lastTriggerGo = go;                                                   // d

        Enemy enemy = go.GetComponent<Enemy>();                               // e
        PowerUp pUp = go.GetComponent<PowerUp>();

        if (enemy != null)
        {  // If the shield was triggered by an enemy
            shieldLevel--;        // Decrease the level of the shield by 1
            Destroy(go);          // … and Destroy the enemy                  // f
        }
        else if (pUp != null)
        {
            AbsorbPowerUp(pUp);
        }
        else
        {
            Debug.LogWarning("Shield trigger hit by non-Enemy: " + go.name);    // g
        }
    }

    public float shieldLevel
    {
        get { return (_shieldLevel); }                                      // b
        private set
        {                                                         // c
            _shieldLevel = Mathf.Min(value, 4);                             // d
            // If the shield is going to be set to less than zero…
            if (value < 0)
            {                                                  // e
                Destroy(this.gameObject);  // Destroy the Hero
                Main.HERO_DIED();
            }
        }
    }

    /// <summary>
    /// Finds the first empty Weapon slot (i.e., type=none) and returns it.
    /// </summary>
    /// <returnsThe first empty Weapon slot or null if none are empty</returns
    Weapon GetEmptyWeaponSlot()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].type == eWeaponType.none)
            {
                return (weapons[i]);
            }
        }
        return (null);
    }

    /// <summary>
    /// Sets the type of all Weapon slots to none
    /// </summary>
    void ClearWeapons()
    {
        foreach (Weapon w in weapons)
        {
            w.SetType(eWeaponType.none);
        }
    }

    void CoordinateWeapons(float time)
    {
        Weapon main = weapons[0];
        main.SetDelayedShotTime(time);

        for (int i = 1; i < weapons.Length; i++)
        {
            Weapon w = weapons[i];
            if (w.type == eWeaponType.none) continue;
            w.SetDelayedShotTime(main.nextShotTime);
        }
    }
    void CoordinateWeapons()
    {
        CoordinateWeapons(Time.time);
    }

    public void AbsorbPowerUp(PowerUp pUp)
    {
        Debug.Log("Absorbed PowerUp: " + pUp.type);                         // b
        switch (pUp.type)
        {
            case eWeaponType.shield:                                              // a 
                shieldLevel++;
                break;

            default:                                                             // b
                if (pUp.type == weapons[0].type)
                { // If it is the same type     // c
                    Weapon weap = GetEmptyWeaponSlot();
                    if (weap != null)
                    {
                        // Set it to pUp.type
                        weap.SetType(pUp.type);
                        CoordinateWeapons(weapons[0].nextShotTime);
                    }
                }
                else
                { // If this is a different weapon type                   // d
                    ClearWeapons();
                    weapons[0].SetType(pUp.type);
                    weapons[0].nextShotTime = 0;
                }
                break;

        }
        pUp.AbsorbedBy(this.gameObject);
    }

}
