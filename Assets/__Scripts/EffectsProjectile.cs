using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.UIElements.ToolbarMenu;
using Random = UnityEngine.Random;

[RequireComponent(typeof(ProjectileHero))]
public class EffectsProjectile : MonoBehaviour
{
    [Serializable]
    public struct ProjectileEffectData
    {

        public GameObject effect;
        public bool isOverride;

        public static implicit operator GameObject(ProjectileEffectData data)
        {
            return data.effect;
        }
    }
    private ProjectileHero proj;

    [SerializeField]
    private ProjectileEffectData fireEffect;
    [SerializeField]
    private ProjectileEffectData hitEffect;
    [SerializeField]
    private float hitCooldown = 1;
    [SerializeField]
    private List<ProjectileEffectData> lingeringEffects = new();

    public Color inheritedColor;
    private float variance = 0;
    private float nextHitTime = 0;

    private readonly List<ParticleSystem> lingeringPSs = new();
    private readonly List<TrailRenderer> lingeringTRs = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        proj = GetComponentInParent<ProjectileHero>();
        if (proj != null)
        {
            WeaponDefinition def = Main.GET_WEAPON_DEFINITION(proj.type);
            inheritedColor = def.projectileColor;

            Collider col = proj.GetComponentInChildren<Collider>();
            if (col != null)
            {
                variance = col.bounds.size.x;
            }
            //proj.FireEvent += GenerateFireEffect;
            proj.HitEvent += GenerateHitEffect;

            if (fireEffect.effect != null)
            {
                //GenerateFireEffect(transform.position);
            }
        }

        foreach (ProjectileEffectData data in lingeringEffects)
        {
            GameObject go = data.effect;
            if (go.TryGetComponent(out ParticleSystem ps))
            {
                SetParticleSystemColor(ps, data);
                lingeringPSs.Add(ps);
            }
            if (go.TryGetComponent(out TrailRenderer tr))
            {
                if (data.isOverride)
                {
                    tr.startColor = inheritedColor;
                    GradientColorKey[] cKeys = tr.colorGradient.colorKeys;
                    cKeys[0].color = inheritedColor;
                    tr.colorGradient.SetKeys(cKeys, tr.colorGradient.alphaKeys);
                }
                lingeringTRs.Add(tr);
            }
        }
    }

    public GameObject GenerateEffect(ProjectileEffectData data, Transform parent)
    {
        if (data.effect == null) return null;

        GameObject fx = Instantiate(data.effect, parent);
        fx.transform.position = transform.position;

        ParticleSystem[] particleSystems = fx.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            SetParticleSystemColor(ps, data);
        }

        return fx;
    }

    public void GenerateFireEffect(Vector3 pos)
    {
        GenerateFireEffect(pos, transform.parent);
    }

    public void GenerateFireEffect(Vector3 pos, Transform parent)
    {
        GameObject fx = GenerateEffect(fireEffect, parent);
        if (fx == null) return;

        Vector3 fixPos = new(pos.x, pos.y, 0);
        fx.transform.position = fixPos;
    }

    public void GenerateHitEffect(Vector3 pos)
    {
        if (Time.time < nextHitTime) return;
        nextHitTime = Time.time + hitCooldown;

        GameObject fx = GenerateEffect(hitEffect, transform.parent);
        if (fx == null) return;

        Vector3 fixPos = new(pos.x, pos.y, transform.position.z);
        fixPos += (Vector3)Random.insideUnitCircle * variance;
        fx.transform.position = fixPos;
    }

    public void DetachLingeringEffects()
    {
        if (lingeringEffects.Count < 0) return;

        foreach (ParticleSystem ps in lingeringPSs)
        {
            ShutdownParticleSystem(ps);
        }
        foreach (TrailRenderer tr in lingeringTRs)
        {
            ShutdownTrail(tr);
        }
        foreach (GameObject effects in lingeringEffects)
        {
            effects.transform.SetParent(transform.parent);
        }
    }

    private void ShutdownParticleSystem(ParticleSystem ps)
    {
        var main = ps.main;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
    }

    private void ShutdownTrail(TrailRenderer tr)
    {
        tr.emitting = false;
        tr.autodestruct = true;
    }

    private void SetParticleSystemColor(ParticleSystem ps, ProjectileEffectData data)
    {
        var psMain = ps.main;
        if (data.isOverride)
        {
            var startColor = psMain.startColor;

            switch (startColor.mode)
            {
                case ParticleSystemGradientMode.Gradient:
                    GradientColorKey[] cKeys = startColor.gradient.colorKeys;
                    cKeys[0].color = inheritedColor;
                    startColor.gradient.SetKeys(cKeys, startColor.gradient.alphaKeys);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    cKeys = startColor.gradientMin.colorKeys;
                    cKeys[0].color = inheritedColor;
                    startColor.gradientMin.SetKeys(cKeys, startColor.gradient.alphaKeys);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    startColor.colorMin = inheritedColor;
                    break;
                case ParticleSystemGradientMode.Color:
                default:
                    startColor.color = inheritedColor;
                    break;
            }

            psMain.startColor = startColor;
        }
    }

    private void OnDisable()
    {
        if (transform.parent != null && transform.parent.gameObject.activeSelf)
        {
            DetachLingeringEffects();
        }
    }
}
