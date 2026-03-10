using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 14f, -12f);
    public float positionLerp = 6f;
    public float rotationLerp = 8f;
    public bool lookAtTarget = true;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        var desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-positionLerp * Time.deltaTime));

        if (lookAtTarget)
        {
            var lookRotation = Quaternion.LookRotation(target.position + Vector3.up * 1.5f - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 1f - Mathf.Exp(-rotationLerp * Time.deltaTime));
        }
    }
}
