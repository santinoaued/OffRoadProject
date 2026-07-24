using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalArea : MonoBehaviour
{
    [Tooltip("Tag que debe tener el vehiculo del jugador.")]
    [SerializeField] private string tagJugador = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagJugador)) return;

        GameManager.Instance.ReachGoal();
    }
}