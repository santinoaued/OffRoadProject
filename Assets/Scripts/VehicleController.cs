using UnityEngine;

// handles vehicle behavior: motor, steering, brakes, and wheel visuals.
// built around WheelColliders.
public class VehicleController : MonoBehaviour
{
    [Header("Engine")]
    [Range(500f, 5000f)]
    [SerializeField] private float motorTorque = 1500f;

    [Tooltip("Top speed in km/h")]
    [Range(20f, 200f)]
    [SerializeField] private float maxSpeed = 80f;

    [Header("Brakes")]
    [Tooltip("Brake force when pressing Space")]
    [Range(0f, 10000f)]
    [SerializeField] private float brakeTorque = 4000f;

    [Tooltip("Handbrake force, applied to rear wheels only")]
    [Range(0f, 10000f)]
    [SerializeField] private float handbrakeTorque = 6000f;

    [Header("Steering")]
    [Tooltip("Maximum angle the front wheels can turn")]
    [Range(10f, 45f)]
    [SerializeField] private float maxSteerAngle = 30f;

    [Header("Traction")]
    [Tooltip("4WD: all four wheels get motor torque")]
    [SerializeField] private bool is4WD = false;

    [Header("Physics")]
    [Tooltip("Lowers the center of mass. Negative Y = more stable, less rollover")]
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

    private float horizontalInput;
    private float forwardInput;
    private bool isBraking;
    private bool isHandbraking;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassOffset;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
        isHandbraking = Input.GetKey(KeyCode.Q);
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
}