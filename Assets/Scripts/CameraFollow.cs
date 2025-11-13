using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothSpeed = 0.125f;
    [SerializeField] Vector3 offset = new Vector3(0, 0, -10); // Offset Z para cámara 2D

    private void LateUpdate()  // ← Cambiado a LateUpdate
    {
        if (target == null) return;  // ← Seguridad por si el target se destruye

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}