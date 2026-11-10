using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjectileHero : MonoBehaviour
{
    private BoundsCheck bndCheck;
    protected Renderer rend;
    public EffectsProjectile fx;

    [Header("Dynamic")]
    public Rigidbody rigid;
    [SerializeField]                                                         // a
    private eWeaponType _type;

    public delegate void ProjectileFXDelegate(Vector3 pos);
    public event ProjectileFXDelegate FireEvent;
    public event ProjectileFXDelegate HitEvent;

    // This public property masks the private field _type
    public eWeaponType type
    {                                              // c
        get { return (_type); }
        set { SetType(value); }
    }


    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponentInChildren<Renderer>();                                     // d
        rigid = GetComponentInChildren<Rigidbody>();

        TryGetComponent(out fx);
    }

    protected virtual void Start()
    {
        Init();
        //FireEvent?.Invoke(transform.position);
    }

    protected virtual void Init()
    { }

    void FixedUpdate()
    {
        Move();

        if (!bndCheck.isOnScreen)
        {
            DestroyProjectile();
        }
    }

    public virtual void Move()
    { }

    public virtual void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    public virtual float DamageHit(Vector3 point)
    {
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        float damage = def.damageOnHit;

        if (damage > 0) 
        {
            HitEvent?.Invoke(point);
        }

        if (def.damagePerSec <= 0)
        {
            DestroyProjectile();
        }

        return damage;
    }

    public virtual float DamageStay(Vector3 point)
    {
        float damage = Main.GET_WEAPON_DEFINITION(type).damagePerSec;
        float damageTick = damage * Time.fixedDeltaTime;

        if (damage > 0)
        {
            HitEvent?.Invoke(point);
        }

        return damageTick;
    }

    /// <summary>
    /// Sets the _type private field and colors this projectile to match the 
    ///   WeaponDefinition.
    /// </summary>
    /// <param name="eType">The eWeaponType to use.</param>
    public void SetType(eWeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        rend.material.color = def.projectileColor;
    }

    /// <summary>
    /// Allows Weapon to easily set the velocity of this ProjectileHero
    /// </summary>
    public Vector3 vel
    {
        get { return rigid.linearVelocity; }
        set { rigid.linearVelocity = value; }
    }

}
