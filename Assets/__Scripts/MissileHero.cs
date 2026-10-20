using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;

public class MissileHero : ProjectileHero
{
    [Header("Inscribed")]
    public float lockOnRadius = 1f;
    public Vector2 lockOnOffset = Vector3.zero;
    public float baseTurnSpeed = 0;
    public float maxTurnSpeed = 60;
    public float turnAccel = 180;
    public float maxTurnRadius = 360;
    public LayerMask lockonLayerMask;

    [Header("Dynamic")]
    public GameObject target;
    private int prevTargets = 0;
    [SerializeField]
    private float turnSpeed = 0;
    [SerializeField]
    private float turnRadiusDeg = 0;

    void Start()
    {
        turnSpeed = baseTurnSpeed;
        turnRadiusDeg = maxTurnRadius;
        SetTarget();
    }

    public override void Move()
    {
        if (turnRadiusDeg <= 0) return;

        if (target == null)
        {
            SetTarget();
            return;
        }

        Vector3 oldVel = vel;

        TrackTarget();

        turnRadiusDeg -= Mathf.Abs(Vector3.Angle(oldVel, vel));
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
        Vector3 position = source.transform.position;
        Vector3 center = position + (Vector3)lockOnOffset;

        Collider[] enemiesAround = Physics.OverlapSphere(center, lockOnRadius, lockonLayerMask);
        if (enemiesAround.Length > 0)
        {
            GameObject closestEnemy = enemiesAround[0].gameObject;
            float closestDist = Vector3.SqrMagnitude(closestEnemy.transform.position - position);
            foreach (Collider col in enemiesAround)
            {
                GameObject enemy = col.gameObject;
                float dist = Vector3.SqrMagnitude(enemy.transform.position - position);
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
        turnSpeed = Mathf.Min(turnSpeed + turnAccel * Time.deltaTime, maxTurnSpeed);
        float degreesToTurn = turnSpeed * Time.deltaTime;

        TrackTarget(degreesToTurn);
    }

    private void TrackTarget(float degreesToTurn)
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
