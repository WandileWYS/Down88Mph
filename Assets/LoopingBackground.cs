using UnityEngine;

public class LoopingBackground : MonoBehaviour
{
    public float backgroundHeight = 30f;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        if (cam.position.y < transform.position.y - backgroundHeight)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (backgroundHeight * 2f), transform.position.z);
        }
    }
}