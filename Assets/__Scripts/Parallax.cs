using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Parallax : MonoBehaviour
{
    [Header("Inscribed")]
    public Transform playerTrans;
    public Transform[] panels;
    public float scrollSpeed = -30f;
    public Vector2 motionMulti = new(0.25f, 0);

    private float panelHeight;
    private float depth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (panels == null || panels.Length <= 1)
        {
            Debug.LogWarning("Script Parallax requires at least 2 panels");
            return;
        }

        panelHeight = panels[0].localScale.y;
        depth = panels[0].position.z;

        panels[0].position = new Vector3(0, 0, depth);
        panels[1].position = new Vector3(0, panelHeight, depth);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 t = Vector2.zero;
        float ySpeed = scrollSpeed;

        if (playerTrans != null)
        {
            t.x = -playerTrans.position.x * motionMulti.x;
        }

        t.y = (Time.time * ySpeed) % panelHeight + (panelHeight * 0.5f);

        panels[0].position = new Vector3(t.x, t.y, depth);

        float p1Y = t.y + panelHeight * (t.y >= 0 ? -1 : 1);
        panels[1].position = new Vector3(t.x, p1Y, depth);
    }
}
