using UnityEngine;

public class ProjBeamHero : ProjectileHero
{
    [Header("Beam: Inscribed")]
    [SerializeField]
    private float lifespan = 1;
    [SerializeField]
    private float growSpeed = 20;

    [Header("Beam: Dynamic")]
    [SerializeField]
    private Vector3 localPos;
    private Transform parentTrans;
    private float killTime = 0;

    private void Start()
    {
        killTime = Time.time + lifespan;
    }

    public void SetParent(Transform transform, Vector3 relPos)
    {
        parentTrans = transform;
        localPos = relPos;
    }

    public override void Move()
    {
        if (Time.time > killTime)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 pos = localPos + parentTrans.position;
        pos.z = localPos.z;
        transform.position = pos;

        float height = transform.localScale.y + growSpeed * Time.fixedDeltaTime;
        SetHeight(height);
    }

    private void SetHeight(float height)
    {
        Vector3 scale = transform.localScale;
        scale.y = Mathf.Min(height, GetViewHeight());
        transform.localScale = scale;
    }

    private float GetViewHeight()
    {
        return Camera.main.orthographicSize * 2;
    }

    public override float DamageStay(GameObject enemy)
    {
        float dist = enemy.transform.position.y - transform.position.y;
        SetHeight(dist);
        return base.DamageStay(enemy);
    }
}
