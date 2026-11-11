using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BlinkColorOnHit))]
public class EnemyShield : Hitable
{
    [Header("Inscribed")]
    public float health = 10;
    [SerializeField]
    private List<EnemyShield> protectors = new List<EnemyShield>();
    private BlinkColorOnHit blinker;

    public bool IsActive
    {
        get { return gameObject.activeInHierarchy; }
        private set { gameObject.SetActive(value); }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        blinker = GetComponent<BlinkColorOnHit>();
        blinker.ignoreOnCollisionEnter = true;

        if (transform.parent == null) return;

        EnemyShield shieldParent = transform.parent.GetComponentInParent<EnemyShield>();
        if (shieldParent != null)
        {
            shieldParent.AddProtector(this);
        }
    }

    public void AddProtector(EnemyShield shield)
    {
        protectors.Add(shield);
    }

    public float TakeDamage(float dmg)
    {
        foreach (EnemyShield es in protectors)
        {
            if (es.IsActive)
            {
                dmg = es.TakeDamage(dmg);
                if (dmg <= 0) return 0;
            }
        }

        blinker.SetColors();
        health -= dmg;

        if (health <= 0)
        {
            IsActive = false;
            return -health;
        }

        return 0;
    }

    public bool HasActiveProtector()
    {
        foreach (EnemyShield es in protectors)
        {
            if (es.IsActive) return true;
        }
        return false;
    }

    void OnTriggerEnter(Collider coll)
    {
        GameObject otherGO = GetColliderGameObject(coll);
        Vector2 point = coll.ClosestPoint(transform.position);

        ProjectileHero p = otherGO.GetComponent<ProjectileHero>();
        if (p != null)
        {
            //if (protectors.Count <= 0) return;

            bool activeProtector = HasActiveProtector();

            if (!activeProtector)
            {
                float hit = p.DamageHit(point);
                TakeDamage(hit);
            }
        }
        else
        {
            Debug.Log("EnemyShield hit by non-ProjectileHero: " + otherGO.name);
        }
    }

    void OnTriggerStay(Collider coll)
    {
        GameObject otherGO = GetColliderGameObject(coll);
        Vector2 point = coll.ClosestPoint(transform.position);

        if (otherGO.TryGetComponent(out ProjectileHero p))
        {
            //if (protectors.Count <= 0) return;

            bool activeProtector = HasActiveProtector();

            if (!activeProtector )
            {
                float hit = p.DamageStay(point);
                TakeDamage(hit);
            }
            else
            {
                float hit = p.DamageHit(point);
                TakeDamage(hit);
                Debug.Log($"{gameObject.name}: Stay turned to Hit");
            }
        }
        else
        {
            Debug.Log("EnemyShield hit by non-ProjectileHero: " + otherGO.name);
        }
    }
}
