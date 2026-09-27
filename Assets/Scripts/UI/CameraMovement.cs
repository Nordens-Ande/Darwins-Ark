using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;
        
        if (Keyboard.current.wKey.isPressed)
            vertical += 1;

        if (Keyboard.current.sKey.isPressed)
            vertical -= 1;

        if (Keyboard.current.dKey.isPressed)
            horizontal += 1;

        if (Keyboard.current.aKey.isPressed)
            horizontal -= 1;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Project camera directions onto the ground, so it doesnt move up and down
        forward = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        right = Vector3.ProjectOnPlane(right, Vector3.up).normalized;

        Vector3 movement =
            forward * vertical +
            right * horizontal;

        Vector3 position = transform.position;

        position += movement.normalized * moveSpeed * Time.deltaTime;

        // Explicit keep the same height
        position.y = transform.position.y;

        transform.position = position;
    }
}
