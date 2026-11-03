using UnityEngine;

public class HealthIndicator : MonoBehaviour
{
    [Header("Dynamic")]
    [SerializeField]
    private TextMesh hpText;
    [SerializeField]
    private Enemy enemy;
    private float maxHealth = 0;
    private Vector2 relativePos = Vector2.zero;

    public string HealthText { get { return enemy.health.ToString("F1"); } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpText = GetComponent<TextMesh>();
        enemy = GetComponentInParent<Enemy>();
        maxHealth = enemy.health;
        hpText.text = HealthText;
        relativePos = new(transform.localPosition.x, transform.localPosition.y);

    }

    // Update is called once per frame
    void Update()
    {
        if (hpText == null || enemy == null) return;
        if (enemy.health >= maxHealth) return;
        hpText.text = HealthText;
    }

    private void LateUpdate()
    {
        Quaternion parentRot = transform.parent.rotation;
        transform.localRotation = Quaternion.Inverse(parentRot);

        Vector3 pos = transform.localRotation * relativePos;
        transform.localPosition = pos;
    }
}
