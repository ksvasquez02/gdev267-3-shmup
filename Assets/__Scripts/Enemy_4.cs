using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(EnemyShield))]
public class Enemy_4 : Enemy
{
    [Header("Enemy_4 Inscribed Fields")]
    public float duration = 4;

    private EnemyShield mainShield;
    private Vector3 p0, p1;
    private float timeStart;

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
        mainShield = GetComponent<EnemyShield>();
    }

    private void Start()
    {
        p0 = p1 = pos;
        InitMovement();
    }

    private void InitMovement()
    {
        p0 = p1;
        float winMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-winMinRad, winMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        if (p0.x * p1.x > 0 && p0.y * p1.y > 0)
        {
            if (Mathf.Abs(p0.x) > Mathf.Abs(p0.y))
            {
                p1.x *= -1;
            }
            else
            {
                p1.y *= -1;
            }
        }

        timeStart = Time.time;
    }

    public override void Move()
    {
        float u = (Time.time - timeStart) / duration;

        if (u >= 1)
        {
            InitMovement();
            u = 0;
        }

        u = u - 0.15f * Mathf.Sin(u * 2 * Mathf.PI);
        pos = ((1 - u) * p0) + (u * p1);
    }

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