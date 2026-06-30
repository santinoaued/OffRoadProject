using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Vehicle")]
    [SerializeField] private Rigidbody vehicleRigidbody;

    // speedometer
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

    // rpm
    [Header("RPM Gauge")]
    [SerializeField] private VehicleController vehicleController;

    [Tooltip("Use the needle pivot object here")]
    [SerializeField] private RectTransform rpmNeedle;

    [Tooltip("RPM shown when the needle is at the minimum angle")]
    [SerializeField] private float minRpmValue = 0f;

    [Tooltip("RPM shown when the needle is at the maximum angle")]
    [SerializeField] private float maxRpmValue = 8000f;

    [Tooltip("Needle angle at 0 RPM")]
    [SerializeField] private float minRpmNeedleAngle = 130f;

    [Tooltip("Needle angle at max RPM")]
    [SerializeField] private float maxRpmNeedleAngle = -130f;

    [Tooltip("Higher values make the needle react faster")]
    [SerializeField] private float rpmNeedleSmoothness = 8f;

    [Header("Vehicle Health")]
    [SerializeField] private VehicleHealth vehicleHealth;
    [SerializeField] private Slider healthSlider;

    private float currentRpmNeedleAngle;
    private float currentNeedleAngle;

    private void Awake()
    {
        if (needle != null)
        {
            currentNeedleAngle = needle.localEulerAngles.z;
        }

        if (rpmNeedle != null)
        {
            currentRpmNeedleAngle = rpmNeedle.localEulerAngles.z;
        }

        if (healthSlider != null)
            healthSlider.value = 1f;

        if (vehicleHealth != null)
            vehicleHealth.onHealthChanged.AddListener(OnHealthChanged);
    }

    private void OnHealthChanged(float normalized)
    {
        if (healthSlider != null)
            healthSlider.value = normalized;
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

        // rpm
        if (vehicleController != null && rpmNeedle != null)
        {
        float currentRpm = vehicleController.GetRPM();
        float rpmPercent = Mathf.InverseLerp(minRpmValue, maxRpmValue, currentRpm);
        float targetRpmAngle = Mathf.Lerp(minRpmNeedleAngle, maxRpmNeedleAngle, rpmPercent);

        float rpmSmoothStep = 1f - Mathf.Exp(-rpmNeedleSmoothness * Time.deltaTime);
        currentRpmNeedleAngle = Mathf.LerpAngle(currentRpmNeedleAngle, targetRpmAngle, rpmSmoothStep);

        rpmNeedle.localRotation = Quaternion.Euler(0f, 0f, currentRpmNeedleAngle);
        }
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