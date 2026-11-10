using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ProjPhaserHero : ProjectileHero
{
    [Header("Inscribed")]
    [Tooltip("# of seconds for a full sine wave")]                            // b 
    public float waveFrequency = 2;
    [Tooltip("Sine wave width in meters")]
    public float waveWidth = 4;
    //[Tooltip("Amount the projectile will roll left and right with the sine wave")]
    //public float waveRotY = 45;

    [Header("Dynamic")]
    private float x0;
    private float birthTime;

    protected override void Init()
    {
        x0 = transform.position.x;
        birthTime = Time.time;
    }

    public override void Move()
    {
        Vector3 tempPos = transform.position;

        float age = Time.time - birthTime;
        float theta = Mathf.PI * 2 * age / waveFrequency;
        float sin = Mathf.Sin(theta);
        tempPos.x = x0 + waveWidth * sin;

        float deltaX = tempPos.x - transform.position.x;
        float pseudoVelX = deltaX / Time.fixedDeltaTime;
        Vector3 pseudoVel = new(pseudoVelX, vel.y);

        float angle = Vector3.SignedAngle(Vector3.up, pseudoVel, Vector3.back);

        //Vector3 rot = new(0, sin * waveRotY, 0);
        //Vector3 rot = new(0, sin * waveRotY, 0);
        //transform.rotation = Quaternion.Euler(rot);

        transform.SetPositionAndRotation(tempPos, Quaternion.AngleAxis(angle, Vector3.back));
    }
}
