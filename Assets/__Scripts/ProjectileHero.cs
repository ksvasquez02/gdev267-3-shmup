using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjectileHero : MonoBehaviour
{
    private BoundsCheck bndCheck;
    protected Renderer rend;

    [Header("Dynamic")]
    public Rigidbody rigid;
    [SerializeField]                                                         // a
    private eWeaponType _type;


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
    }

    void FixedUpdate()
    {
        Move();

        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offUp))
        {
            Destroy(gameObject);
        }
    }

    public virtual void Move()
    { }

    public virtual float DamageHit()
    {
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        float damage = def.damageOnHit;
        if (def.damagePerSec <= 0)
        {
            Destroy(gameObject);
        }
        return damage;
    }

    public virtual float DamageStay()
    {
        float damage = Main.GET_WEAPON_DEFINITION(type).damagePerSec;
        float damageTick = damage * Time.fixedDeltaTime;
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
