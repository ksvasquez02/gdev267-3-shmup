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
    private Transform resizing;
    private Transform parentTrans;
    private float killTime = 0;

    protected override void Init()
    {
        resizing = rend.transform.parent;
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

        if (parentTrans == null)
        {
            return;
        }

        Vector3 pos = localPos + parentTrans.position;
        pos.z = localPos.z;
        transform.position = pos;

        float height = resizing.localScale.y + growSpeed * Time.fixedDeltaTime;
        SetHeight(height);
    }

    private void SetHeight(float height)
    {
        Vector3 scale = resizing.localScale;
        scale.y = Mathf.Min(height, GetViewHeight());
        resizing.localScale = scale;
    }

    private float GetViewHeight()
    {
        return Camera.main.orthographicSize * 2;
    }

    public override float DamageStay(Vector3 point)
    {
        float dist = point.y - transform.position.y - 2f;
        SetHeight(dist);
        return base.DamageStay(point);
    }
}
