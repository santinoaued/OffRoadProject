using UnityEngine;
using UnityEngine.Events;

// manages vehicle health: collision damage, engine overrev damage
// attach to the same GameObject as VehicleController
[RequireComponent(typeof(VehicleController))]
[RequireComponent(typeof(Rigidbody))]
public class VehicleHealth : MonoBehaviour
{
    [Header("Health")]
    [Range(0f, 100f)]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Collision Damage")]
    [Tooltip("Minimum impact magnitude to start dealing damage")]
    [SerializeField] private float minImpactThreshold = 3f;
    [Tooltip("Multiplier applied to impact magnitude to calculate damage")]
    [SerializeField] private float impactDamageMultiplier = 2f;
    [Tooltip("Max damage from a single collision")]
    [SerializeField] private float maxCollisionDamage = 25f;

    [Header("Engine Overrev Damage")]
    [Tooltip("RPM above which the engine starts taking damage")]
    [SerializeField] private float overrevThreshold = 7200f;
    [Tooltip("Damage per second while overreving")]
    [SerializeField] private float overrevDamagePerSecond = 3f;
    [Tooltip("Seconds the engine must be overreving before damage starts")]
    [SerializeField] private float overrevGracePeriod = 2f;
    private float overrevTimer = 0f;

    [Header("Events")]
    public UnityEvent onVehicleDestroyed;
    public UnityEvent<float> onHealthChanged; // passes 0-1 normalized

    private VehicleController vehicleController;
    private bool isDestroyed = false;

    private void Awake()
    {
        vehicleController = GetComponent<VehicleController>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDestroyed) return;
        HandleOverrevDamage();
    }

    private void HandleOverrevDamage()
    {
        float rpm = vehicleController.GetRPM();

        if (rpm >= overrevThreshold)
        {
            overrevTimer += Time.deltaTime;

            if (overrevTimer >= overrevGracePeriod)
            {
                float damage = overrevDamagePerSecond * Time.deltaTime;
                ApplyDamage(damage);
            }
        }
        else
        {
            overrevTimer = Mathf.Max(0f, overrevTimer - Time.deltaTime * 2f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed) return;

        float impact = collision.relativeVelocity.magnitude;

        if (impact < minImpactThreshold) return;

        float damage = Mathf.Clamp(
            (impact - minImpactThreshold) * impactDamageMultiplier,
            0f,
            maxCollisionDamage
        );

        ApplyDamage(damage);
    }

    private void ApplyDamage(float amount)
    {
        if (isDestroyed) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0f)
        {
            isDestroyed = true;
            onVehicleDestroyed?.Invoke();
        }
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }

    public float GetHealthNormalized() => currentHealth / maxHealth;
    public float GetHealth() => currentHealth;
    public bool IsDestroyed() => isDestroyed;
}
