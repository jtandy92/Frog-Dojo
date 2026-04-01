using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform player; // The player's transform

    [Header("Camera Behavior")]
    [Range(0f, 1f)]
    public float cameraFollowFactor = 0.5f; 
    // 0 = camera stays at player, 
    // 1 = camera moves fully to mouse position.
    // Adjust this to find the sweet spot of how far towards the mouse the camera goes.

    public float smoothTime = 0.3f;  // Base smoothing time
    public float moveSpeed = 1f;      // Speed multiplier for how quickly camera reacts

    public Vector3 offset; // Offset from the player's position

    private Vector3 velocity = Vector3.zero; // Used by SmoothDamp

    void LateUpdate()
    {
        if (player == null)
            return; // If no player is assigned, exit early

        // Get the mouse position in world coordinates
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = player.position.z; // Keep the same z-level as the player

        // Determine the desired camera target position based on the player and the mouse
        Vector3 playerPositionWithOffset = player.position + offset;
        Vector3 desiredPosition = Vector3.Lerp(playerPositionWithOffset, mouseWorldPos, cameraFollowFactor);

        // Smoothly move the camera towards the desired position
        // Adjusting smoothTime by moveSpeed so that higher moveSpeed reacts faster
        float adjustedSmoothTime = smoothTime / moveSpeed;

        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, adjustedSmoothTime);

        // Lock Z-axis for a 2D game
        smoothedPosition.z = -10f; 

        // Assign the new position to the camera
        transform.position = smoothedPosition;
    }
}
