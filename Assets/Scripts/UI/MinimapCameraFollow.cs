using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform target;
    public float height = 52f;
    public float positionLerp = 10f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        var desiredPosition = new Vector3(target.position.x, target.position.y + height, target.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-positionLerp * Time.deltaTime));
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
