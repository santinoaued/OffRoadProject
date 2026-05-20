using UnityEngine;

// handles all vehicle behavior: motor, steering, brakes, and wheel visuals.
// built around WheelColliders
public class VehicleController : MonoBehaviour
{
    [Header("Engine")]
    [Range(500f, 5000f)]
    [SerializeField] private float motorTorque = 1500f;

    // internally converted to m/s when checking against rigidbody velocity
    [Tooltip("Top speed in km/h")]
    [Range(20f, 200f)]
    [SerializeField] private float maxSpeed = 80f;

    [Header("Brakes")]
    [Tooltip("Brake force when releasing the accelerator")]
    [Range(0f, 3000f)]
    [SerializeField] private float brakeTorque = 1000f;

    [Tooltip("Brake force when pressing Space")]
    [Range(0f, 10000f)]
    [SerializeField] private float hardBrakeTorque = 4000f;

    // only locks rear wheels useful for drifting or tight turns
    [Tooltip("Handbrake force (rear wheels only) good for tight turns")]
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
    public Rigidbody rb;
    public float lastUpdate;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassOffset;

        // lock cursor so it doesn't wander around during play
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // read inputs every frame — FixedUpdate will consume them
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
        isHandbraking = Input.GetKey(KeyCode.Q);
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBrakes();
        UpdateWheelMeshes();
    }

    void HandleMotor()
    {
        // cut power once we hit the speed cap simple but effective
        bool overMaxSpeed = rb.linearVelocity.magnitude > maxSpeed / 3.6f;
        float torque = overMaxSpeed ? 0f : forwardInput * motorTorque;

        WC_RearLeft.motorTorque = torque;
        WC_RearRight.motorTorque = torque;

        // front wheels only get torque in 4WD mode
        WC_FrontLeft.motorTorque = is4WD ? torque : 0f;
        WC_FrontRight.motorTorque = is4WD ? torque : 0f;
    }

    void HandleSteering()
    {
        float steerAngle = horizontalInput * maxSteerAngle;
        WC_FrontLeft.steerAngle = steerAngle;
        WC_FrontRight.steerAngle = steerAngle;
    }

    void HandleBrakes()
    {
        bool noInput = Mathf.Approximately(forwardInput, 0f);

        float frontBrake = 0f;
        float rearBrake = 0f;

        if (isBraking)
        {
            // hard stop with both axles locked
            frontBrake = hardBrakeTorque;
            rearBrake = hardBrakeTorque;
        }
        else if (isHandbraking)
        {
            // rear-only lock, front wheels stay free to steer
            frontBrake = 0f;
            rearBrake = handbrakeTorque;
        }
        else if (noInput)
        {
            // gentle engine braking when the player lets go of the throttle
            frontBrake = brakeTorque;
            rearBrake = brakeTorque;
        }

        WC_FrontLeft.brakeTorque = frontBrake;
        WC_FrontRight.brakeTorque = frontBrake;
        WC_RearLeft.brakeTorque = rearBrake;
        WC_RearRight.brakeTorque = rearBrake;
    }

    void UpdateWheelMeshes()
    {
        UpdateMesh(WC_FrontLeft, Mesh_FrontLeft);
        UpdateMesh(WC_FrontRight, Mesh_FrontRight);
        UpdateMesh(WC_RearLeft, Mesh_RearLeft);
        UpdateMesh(WC_RearRight, Mesh_RearRight);
    }

    // syncs the visual mesh to wherever the WheelCollider says the wheel actually is
    void UpdateMesh(WheelCollider col, Transform mesh)
    {
        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
}