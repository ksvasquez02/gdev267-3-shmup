using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class Enemy : Hitable
{


    [Header("Inscribed")]
    public float speed = 10f;   // The movement speed is 10m/s
    public float fireRate = 0.3f;  // Seconds/shot (Unused)
    public float health = 10;    // Damage needed to destroy this enemy
    public int score = 100;   // Points earned for destroying this
    public float powerUpDropChance = 1f;


    // private BoundsCheck bndCheck;                                             // b
    protected BoundsCheck bndCheck;
    protected bool calledShipDestroyed = false;

    public virtual float Health { get { return health; } }

    protected virtual void Awake()
    {                                                            // c
        bndCheck = GetComponent<BoundsCheck>();
    }

    // This is a Property: A method that acts like a field
    public Vector3 pos
    {                                                       // a
        get
        {
            return this.transform.position;
        }
        set
        {
            this.transform.position = value;
        }
    }

    void Update()
    {
        Move();

        // Check whether this Enemy has gone off the bottom of the screen
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offDown))
        {
            Destroy(gameObject);


        }
    }

    public virtual void Move()
    { // c
        Vector3 tempPos = pos;
        tempPos.y -= speed * Time.deltaTime;
        pos = tempPos;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            if (!calledShipDestroyed)
            {
                calledShipDestroyed = true;
                Main.SHIP_DESTROYED(this);
            }
            // Destroy this Enemy
            Destroy(this.gameObject);
        }
    }

    public void HandleProjectileHit(GameObject go, float damage)
    {
        // Only damage this Enemy if it’s on screen
        if (bndCheck.isOnScreen)
        {
            // Get the damage amount from the Main WEAP_DICT.
            TakeDamage(damage);
        }
        // Destroy the ProjectileHero otherwise
        else
        {
            Destroy(go);                                               // e
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        GameObject otherGO = GetColliderGameObject(coll);
        Vector2 point = coll.ClosestPoint(transform.position);

        // Check for collisions with ProjectileHero
        if (otherGO.TryGetComponent(out ProjectileHero p))
        {
            HandleProjectileHit(otherGO, p.DamageHit(point));
        }
        else
        {
            print("Enemy hit by non-ProjectileHero: " + otherGO.name);      // f
        }
    }

    void OnTriggerStay(Collider coll)
    {
        GameObject otherGO = GetColliderGameObject(coll);
        Vector2 point = coll.ClosestPoint(transform.position);

        // Check for collisions with ProjectileHero
        if (otherGO.TryGetComponent(out ProjectileHero p))
        {
            HandleProjectileHit(otherGO, p.DamageStay(point));
        }
        else
        {
            print("Enemy hit by non-ProjectileHero: " + otherGO.name);      // f
        }
    }

}