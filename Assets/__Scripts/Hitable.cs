using UnityEngine;

public abstract class Hitable : MonoBehaviour
{
    protected GameObject GetColliderGameObject(Collider coll)
    {
        if (coll.attachedRigidbody != null)
        {
            return coll.attachedRigidbody.gameObject;
        }
        return coll.gameObject;
    }
}
