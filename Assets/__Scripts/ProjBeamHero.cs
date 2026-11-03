using UnityEngine;

public class ProjBeamHero : ProjectileHero
{
    [Header("Beam: Inscribed")]
    [SerializeField]
    private float lifespan = 1;

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

        Vector3 scale = transform.localScale;
        scale.y = GetViewHeight();
        transform.localScale = scale;
    }

    private float GetViewHeight()
    {
        return Camera.main.orthographicSize * 2;
    }
}
