using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(EnemyShield))]
public class Enemy_4 : Enemy
{
    private EnemyShield[] allShields;
    private EnemyShield mainShield;

    public override float Health
    {
        get
        {
            if (mainShield != null)
            {
                return mainShield.health;
            }
            return health;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        allShields = GetComponentsInChildren<EnemyShield>();
        mainShield = GetComponent<EnemyShield>();
    }

    //public override void Move()
    //{
        
    //}

    void OnTriggerEnter(Collider coll)
    {
        GameObject otherGO = coll.gameObject;

        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();
        if (p != null)
        {
            if (mainShield.IsActive) return;
            HandleProjectileHit(otherGO, p.DamageHit(gameObject));
        }
        else
        {
            print("Enemy hit by non-ProjectileHero: " + otherGO.name);      // f
        }
    }

    void OnTriggerStay(Collider coll)
    {
        GameObject otherGO = coll.gameObject;

        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();
        if (p != null)
        {
            if (mainShield.IsActive) return;
            HandleProjectileHit(otherGO, p.DamageStay(gameObject));
        }
        else
        {
            print("Enemy hit by non-ProjectileHero: " + otherGO.name);      // f
        }
    }
}