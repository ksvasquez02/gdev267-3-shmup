using UnityEngine;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
    [Header("Dynamic")]
    [SerializeField]
    private Slider hpSlider;
    [SerializeField]
    private Enemy enemy;
    private float maxHealth = 0;
    private Vector2 relativePos = Vector2.zero;

    public float HealthValue { get { return enemy.Health / maxHealth; } }
    public float HealthPercent { get { return Mathf.Ceil(HealthValue * 100); } }
    public string HealthText { get { return $"{HealthPercent}%"; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpSlider = GetComponentInChildren<Slider>();
        enemy = GetComponentInParent<Enemy>();
        maxHealth = enemy.Health;
        hpSlider.value = HealthValue;
        relativePos = new(transform.localPosition.x, transform.localPosition.y);
        hpSlider.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (hpSlider == null || enemy == null) return;
        if (enemy.Health >= maxHealth) return;

        if (!hpSlider.isActiveAndEnabled && HealthValue < 1)
            hpSlider.gameObject.SetActive(true);

        hpSlider.value = HealthValue;
    }

    private void LateUpdate()
    {
        Quaternion parentRot = transform.parent.rotation;
        transform.localRotation = Quaternion.Inverse(parentRot);

        Vector3 pos = transform.localRotation * relativePos;
        transform.localPosition = pos;
    }
}
