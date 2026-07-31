using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference brakeAction;
    [SerializeField] private InputActionReference handbrakeAction;

    [Header("Engine")]
    [Range(500f, 5000f)]
    [SerializeField] private float motorTorque = 1500f;

    [Range(20f, 200f)]
    [SerializeField] private float maxSpeed = 80f;

    [Header("Brakes")]
    [Range(0f, 10000f)]
    [SerializeField] private float brakeTorque = 4000f;

    [Range(0f, 10000f)]
    [SerializeField] private float handbrakeTorque = 6000f;

    [Header("Steering")]
    [Range(10f, 45f)]
    [SerializeField] private float maxSteerAngle = 30f;

    [Header("Traction")]
    [SerializeField] private bool is4WD = false;

    [Header("Physics")]
    [Tooltip("lowers the center of mass. Negative Y = more stable, less rollover")]
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0f, -0.5f, 0f);

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider WC_FrontLeft;
    [SerializeField] private WheelCollider WC_FrontRight;
    [SerializeField] private WheelCollider WC_RearLeft;
    [SerializeField] private WheelCollider WC_RearRight;

    [Header("Wheel Meshes")]
    [SerializeField] private Transform Mesh_FrontLeft;
    [SerializeField] private Transform Mesh_FrontRight;
    [SerializeField] private Transform Mesh_RearLeft;
    [SerializeField] private Transform Mesh_RearRight;

    [Header("Engine RPM")]
    [SerializeField] private float idleRPM = 800f;
    [SerializeField] private float maxRPM = 8000f;
    [SerializeField] private float rpmSmoothness = 5f;
    [Header("Gears (Simple Auto)")]
    [SerializeField] private float[] gearTopSpeeds = { 20f, 40f, 60f, 80f };
    private int currentGear = 0;

    private float currentRPM = 0f;
    private float horizontalInput;
    private float forwardInput;
    private bool isBraking;
    private bool isHandbraking;
    private Rigidbody rb;

    private void OnEnable()
    {
        moveAction.action.Enable();
        brakeAction.action.Enable();
        handbrakeAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        brakeAction.action.Disable();
        handbrakeAction.action.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassOffset;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Vector2 move = moveAction.action.ReadValue<Vector2>();
        horizontalInput = move.x;
        forwardInput = move.y;
        isBraking = brakeAction.action.IsPressed();
        isHandbraking = handbrakeAction.action.IsPressed();

        UpdateRPM();
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBrakes();
        UpdateWheelMeshes();
    }

    private void HandleMotor()
    {
        bool overMaxSpeed = rb.linearVelocity.magnitude > maxSpeed / 3.6f;
        float torque = overMaxSpeed ? 0f : forwardInput * motorTorque;

        WC_RearLeft.motorTorque = torque;
        WC_RearRight.motorTorque = torque;

        WC_FrontLeft.motorTorque = is4WD ? torque : 0f;
        WC_FrontRight.motorTorque = is4WD ? torque : 0f;
    }

    private void HandleSteering()
    {
        float steerAngle = horizontalInput * maxSteerAngle;
        WC_FrontLeft.steerAngle = steerAngle;
        WC_FrontRight.steerAngle = steerAngle;
    }

    private void HandleBrakes()
    {
        float frontBrake = 0f;
        float rearBrake = 0f;

        if (isBraking)
        {
            frontBrake = brakeTorque;
            rearBrake = brakeTorque;
        }
        else if (isHandbraking)
        {
            rearBrake = handbrakeTorque;
        }

        WC_FrontLeft.brakeTorque = frontBrake;
        WC_FrontRight.brakeTorque = frontBrake;
        WC_RearLeft.brakeTorque = rearBrake;
        WC_RearRight.brakeTorque = rearBrake;
    }

    private void UpdateWheelMeshes()
    {
        UpdateMesh(WC_FrontLeft, Mesh_FrontLeft);
        UpdateMesh(WC_FrontRight, Mesh_FrontRight);
        UpdateMesh(WC_RearLeft, Mesh_RearLeft);
        UpdateMesh(WC_RearRight, Mesh_RearRight);
    }

    private void UpdateMesh(WheelCollider col, Transform mesh)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }

    private void UpdateRPM()
    {
#if UNITY_6000_0_OR_NEWER
        float speedKmh = rb.linearVelocity.magnitude * 3.6f;
#else
        float speedKmh = rb.velocity.magnitude * 3.6f;
#endif

        currentGear = 0;
        for (int i = 0; i < gearTopSpeeds.Length; i++)
        {
            if (speedKmh < gearTopSpeeds[i])
            {
                currentGear = i;
                break;
            }

            if (i == gearTopSpeeds.Length - 1) currentGear = i;
        }

        float minGearSpeed = (currentGear == 0) ? 0f : gearTopSpeeds[currentGear - 1];
        float maxGearSpeed = gearTopSpeeds[currentGear];

        float gearFactor = Mathf.InverseLerp(minGearSpeed, maxGearSpeed, speedKmh);

        float targetRPM = Mathf.Lerp(idleRPM, maxRPM, gearFactor);

        if (forwardInput > 0)
        {
            float extraRev = (maxRPM - idleRPM) * 0.2f * forwardInput;
            targetRPM += extraRev;
        }
        else if (forwardInput == 0 && speedKmh < 1f)
        {
            targetRPM = idleRPM;
        }

        targetRPM = Mathf.Clamp(targetRPM, idleRPM, maxRPM);

        float rpmDamping = 1f - Mathf.Exp(-rpmSmoothness * Time.deltaTime);
        currentRPM = Mathf.Lerp(currentRPM, targetRPM, rpmDamping);
    }

    public float GetRPM()
    {
        return currentRPM;
    }
}