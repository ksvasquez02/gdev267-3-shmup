using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;

public class MissileHero : ProjectileHero
{
    [Header("Inscribed")]
    public float lockOnRadius = 1f;
    public float maxLockOnDistance = 10;
    public float minTurnSpeed = 0;
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
        turnSpeed = minTurnSpeed;
        turnRadiusDeg = maxTurnRadius;
        SetTarget(true);
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
        if (turnRadiusDeg <= 0)
        {
            rend.material.color = Color.Lerp(rend.material.color, Color.white, 0.1f);
        }
    }

    public void SetTarget(bool freeAim = false)
    {
        target = FindTarget();

        if (target == null) return;

        //if (freeAim)
        //{
        //    float deg = maxTurnSpeed * 2f * Time.deltaTime;
        //    TrackTarget(deg);
        //}

        if (prevTargets > 0)
        {
            turnSpeed = 0;
        }
        prevTargets++;
    }

    private GameObject FindTarget()
    {
        return FindTarget(gameObject);
    }

    private GameObject FindTarget(GameObject source)
    {
        Vector3 center = source.transform.position;
        Vector3 direction = source.transform.TransformDirection(Vector3.up);

        Collider[] enemiesAround = Physics.OverlapSphere(center, lockOnRadius, lockonLayerMask);
        if (enemiesAround.Length > 0)
        {
            GameObject closest = enemiesAround[0].gameObject;
            float closestDist = Vector3.SqrMagnitude(closest.transform.position - center);
            foreach (Collider col in enemiesAround)
            {
                GameObject enemy = col.gameObject;
                float dist = Vector3.SqrMagnitude(enemy.transform.position - center);
                if (dist < closestDist)
                {
                    closest = enemy;
                    closestDist = dist;
                }
            }
            return closest;
        }

        bool enemyInFront = Physics.Raycast(center, direction, out RaycastHit hit, maxLockOnDistance, lockonLayerMask);
        if (enemyInFront)
        {
            return hit.collider.gameObject;
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

        if (toFace.sqrMagnitude >= maxLockOnDistance * maxLockOnDistance)
        {
            target = null;
            return;
        }

        vel = Vector3.RotateTowards(vel, toFace, Mathf.Deg2Rad * degreesToTurn, 0.0f);

        float angle = Vector3.SignedAngle(Vector3.up, vel, Vector3.back);
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
    }

    private void OnDrawGizmos()
    {
        if (target != null) return;
        Gizmos.color = Color.red;

        float rad = lockOnRadius;
        Vector3 _vel = Vector3.up;
        if (rigid != null) _vel = vel;
        Vector3 pos = transform.position;
        Vector3 max = pos + _vel.normalized * maxLockOnDistance;

        Gizmos.DrawWireSphere(pos, rad);
        Gizmos.DrawLine(pos, max);
    }
}
