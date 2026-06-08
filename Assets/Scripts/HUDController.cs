using UnityEngine;

// simple dashboard UI.
public class HUDController : MonoBehaviour
{
    [Header("Vehicle")]
    [SerializeField] private Rigidbody vehicleRigidbody;

    [Header("Speedometer")]
    [Tooltip("Use the needle pivot object here")]
    [SerializeField] private RectTransform needle;

    [Tooltip("Speed shown when the needle is at the minimum angle")]
    [SerializeField] private float minSpeedKmh = 0f;

    [Tooltip("Speed shown when the needle is at the maximum angle")]
    [SerializeField] private float maxSpeedKmh = 160f;

    [Tooltip("Needle angle at 0 km/h")]
    [SerializeField] private float minNeedleAngle = 130f;

    [Tooltip("Needle angle at max speed")]
    [SerializeField] private float maxNeedleAngle = -130f;

    [Tooltip("Higher values make the needle react faster")]
    [SerializeField] private float needleSmoothness = 8f;

    private float currentNeedleAngle;

    private void Awake()
    {
        if (needle != null)
        {
            currentNeedleAngle = needle.localEulerAngles.z;
        }
    }

    private void Update()
    {
        if (vehicleRigidbody == null || needle == null)
        {
            return;
        }

        float speedKmh = GetSpeedKmh();
        float speedPercent = Mathf.InverseLerp(minSpeedKmh, maxSpeedKmh, speedKmh);
        float targetAngle = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, speedPercent);

        float smoothStep = 1f - Mathf.Exp(-needleSmoothness * Time.deltaTime);
        currentNeedleAngle = Mathf.LerpAngle(currentNeedleAngle, targetAngle, smoothStep);

        needle.localRotation = Quaternion.Euler(0f, 0f, currentNeedleAngle);
    }

    private float GetSpeedKmh()
    {
#if UNITY_6000_0_OR_NEWER
        return vehicleRigidbody.linearVelocity.magnitude * 3.6f;
#else
        return vehicleRigidbody.velocity.magnitude * 3.6f;
#endif
    }
}