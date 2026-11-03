using UnityEngine;

public class ProjLaserHero : ProjectileHero
{
    [Header("Laser: Inscribed")]
    public Vector2 minMaxSize = new(1, 10);
    public Vector2 minMaxGrowSpeed = new(10, 20);
    public float growAccel = 100;

    [Header("Laser: Dynamic")]
    private float growSpeed = 0;

    void Start()
    {
        growSpeed = growAccel >= 0 ? minMaxGrowSpeed.x : minMaxGrowSpeed.y;
        Vector3 scale = transform.localScale;
        scale.y = minMaxSize.x;
        transform.localScale = scale;
    }

    public override void Move()
    {
        Vector3 scale = transform.localScale;

        if (scale.y < minMaxSize.y)
        {
            growSpeed += growAccel * Time.fixedDeltaTime;
            growSpeed = Mathf.Clamp(growSpeed, minMaxGrowSpeed.x, minMaxGrowSpeed.y);

            scale.y += growSpeed * Time.fixedDeltaTime;
            scale.y = Mathf.Min(scale.y, minMaxSize.y);

            transform.localScale = scale;
        }
    }
}
