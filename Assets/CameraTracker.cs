using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = new Vector3(0f, player.position.y, 0f);
        }
    }
}