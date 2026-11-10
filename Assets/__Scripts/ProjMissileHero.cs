using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class ProjMissileHero : ProjectileHero
{
    [Header("Missile Inscribed")]
    public float lockOnRadius = 1f;
    public Vector2 lockOnOffset = Vector3.zero;
    public float baseTurnSpeed = 0;
    public float maxTurnSpeed = 60;
    public float turnAccel = 180;
    public float maxTurnDeg = 360;
    public LayerMask lockonLayerMask;

    [Header("Missile Dynamic")]
    public GameObject target;
    private int prevTargets = 0;
    [SerializeField]
    private float turnSpeed = 0;
    [SerializeField]
    private float remainingTurnDeg = 0;

    protected override void Init()
    {
        turnSpeed = baseTurnSpeed;
        remainingTurnDeg = maxTurnDeg;
        SetTarget();
    }

    public override void Move()
    {
        if (remainingTurnDeg <= 0) return;

        if (target == null)
        {
            SetTarget();
            return;
        }

        Vector3 oldVel = vel;
        TrackTarget();
        remainingTurnDeg -= Mathf.Abs(Vector3.Angle(oldVel, vel));
    }

    public void SetTarget()
    {
        target = FindTarget();

        if (target == null) return;

        if (prevTargets > 0)
        {
            turnSpeed = baseTurnSpeed;
        }
        prevTargets++;
    }

    private GameObject FindTarget()
    {
        return FindTarget(gameObject);
    }

    private GameObject FindTarget(GameObject source)
    {
        Vector3 pos = source.transform.position;
        Vector3 center = pos + (Vector3)lockOnOffset;

        // Find all enemies within lockOnRadius of missile
        Collider[] targets = Physics.OverlapSphere(center, lockOnRadius, lockonLayerMask);
        if (targets.Length > 0)
        {
            // Compare the distances of each target to find the closest
            GameObject closestEnemy = targets[0].gameObject;
            float closestDist = Vector3.SqrMagnitude(closestEnemy.transform.position - pos);
            foreach (Collider col in targets)
            {
                GameObject enemy = col.gameObject;
                float dist = Vector3.SqrMagnitude(enemy.transform.position - pos);
                if (dist < closestDist)
                {
                    closestEnemy = enemy;
                    closestDist = dist;
                }
            }
            return closestEnemy;
        }

        return null;
    }

    private void TrackTarget()
    {
        turnSpeed = Mathf.Min(turnSpeed + turnAccel * Time.fixedDeltaTime, maxTurnSpeed);
        Turn(turnSpeed * Time.fixedDeltaTime);
    }

    private void Turn(float degreesToTurn)
    {
        Vector3 targetPos = target.transform.position;
        Vector3 toFace = targetPos - transform.position;

        vel = Vector3.RotateTowards(vel, toFace, Mathf.Deg2Rad * degreesToTurn, 0.0f);

        float angle = Vector3.SignedAngle(Vector3.up, vel, Vector3.back);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
    }

    private void OnDrawGizmos()
    {
        if (target != null) return;
        Gizmos.color = Color.red;

        float rad = lockOnRadius;
        Vector3 cen = transform.position + (Vector3)lockOnOffset;
        Gizmos.DrawWireSphere(cen, rad);
    }
}
