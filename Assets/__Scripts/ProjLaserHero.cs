using UnityEngine;

public class ProjLaserHero : ProjectileHero
{
    [Header("Laser: Inscribed")]
    public Vector2 minMaxSize = new(1, 10);
    public Vector2 minMaxGrowSpeed = new(10, 20);
    public float growAccel = 100;

    [Header("Laser: Dynamic")]
    private Transform resizing;
    private float growSpeed = 0;

    protected override void Init()
    {
        growSpeed = growAccel >= 0 ? minMaxGrowSpeed.x : minMaxGrowSpeed.y;

        resizing = rend.transform.parent;

        Vector3 scale = resizing.localScale;
        scale.y = minMaxSize.x;
        resizing.localScale = scale;
    }

    public override void Move()
    {
        Vector3 scale = resizing.localScale;

        if (scale.y < minMaxSize.y)
        {
            growSpeed += growAccel * Time.fixedDeltaTime;
            growSpeed = Mathf.Clamp(growSpeed, minMaxGrowSpeed.x, minMaxGrowSpeed.y);

            scale.y += growSpeed * Time.fixedDeltaTime;
            scale.y = Mathf.Min(scale.y, minMaxSize.y);

            resizing.localScale = scale;
        }
    }
}
