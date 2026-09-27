using UnityEngine;

public class BillBoardUI : MonoBehaviour
{
    // Mood indicator always facing player

    void LateUpdate()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;

        transform.forward = cam.transform.forward;
    }
}
