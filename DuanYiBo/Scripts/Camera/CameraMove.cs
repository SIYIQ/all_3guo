using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Dead Zone")]
    [SerializeField] private Vector2 deadZoneSize = new Vector2(2f, 1.5f);

    [Header("Follow Smoothing")]
    [SerializeField] private float smoothTimeX = 0.15f;
    [SerializeField] private float smoothTimeY = 0.3f;

    [Header("Bounds")]
    [SerializeField] private bool limitToBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(10f, 5f);

    private UnityEngine.Camera cam;
    private Vector2 smoothVelocity;
    private Vector2 focusPosition;
    private bool hasFocusPosition;

    private void Awake()
    {
        cam = GetComponent<UnityEngine.Camera>();
        ResetFocus();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        ResetFocus();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (!hasFocusPosition)
        {
            ResetFocus();
        }

        Vector2 targetPosition = target.position;
        focusPosition = ApplyDeadZone(focusPosition, targetPosition);

        Vector3 current = transform.position;
        Vector3 desired = current;

        desired.x = Mathf.SmoothDamp(current.x, focusPosition.x, ref smoothVelocity.x, smoothTimeX);
        desired.y = Mathf.SmoothDamp(current.y, focusPosition.y, ref smoothVelocity.y, smoothTimeY);

        if (limitToBounds)
        {
            ClampToBounds(ref desired);
        }

        desired.z = current.z;
        transform.position = desired;
    }

    private void ResetFocus()
    {
        if (target == null)
        {
            hasFocusPosition = false;
            return;
        }

        focusPosition = target.position;
        smoothVelocity = Vector2.zero;
        hasFocusPosition = true;
    }

    private Vector2 ApplyDeadZone(Vector2 focus, Vector2 targetPosition)
    {
        float halfDeadX = Mathf.Max(0f, deadZoneSize.x) * 0.5f;
        float halfDeadY = Mathf.Max(0f, deadZoneSize.y) * 0.5f;

        float deltaX = targetPosition.x - focus.x;
        if (deltaX > halfDeadX)
        {
            focus.x = targetPosition.x - halfDeadX;
        }
        else if (deltaX < -halfDeadX)
        {
            focus.x = targetPosition.x + halfDeadX;
        }

        float deltaY = targetPosition.y - focus.y;
        if (deltaY > halfDeadY)
        {
            focus.y = targetPosition.y - halfDeadY;
        }
        else if (deltaY < -halfDeadY)
        {
            focus.y = targetPosition.y + halfDeadY;
        }

        return focus;
    }

    private void ClampToBounds(ref Vector3 position)
    {
        if (cam == null || !cam.orthographic)
        {
            position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
            position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
            return;
        }

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = minBounds.x + halfWidth;
        float maxX = maxBounds.x - halfWidth;
        float minY = minBounds.y + halfHeight;
        float maxY = maxBounds.y - halfHeight;

        if (minX > maxX)
        {
            position.x = (minBounds.x + maxBounds.x) * 0.5f;
        }
        else
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }

        if (minY > maxY)
        {
            position.y = (minBounds.y + maxBounds.y) * 0.5f;
        }
        else
        {
            position.y = Mathf.Clamp(position.y, minY, maxY);
        }
    }
}
