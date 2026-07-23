using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuracion del Timer")]
    [Tooltip("Tiempo total de la partida en segundos")]
    [SerializeField] private float tiempoLimite = 120f;

    [Header("Referencias")]
    [Tooltip("Vehiculo del jugador")]
    [SerializeField] private GameObject vehiculoJugador;

    [Tooltip("Texto tiempo restante")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Eventos (escalables desde el Inspector)")]
    [Tooltip("Se dispara cuando el tiempo llega a 0 sin haber alcanzado la meta")]
    public UnityEvent onTiempoAgotado;

    [Tooltip("Se dispara cuando el jugador alcanza la meta antes de que se acabe el tiempo")]
    public UnityEvent onMetaAlcanzada;

    private float tiempoRestante;
    private bool partidaEnCurso;

    private void Awake()
    {
        // basic singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        IniciarPartida();
    }

    private void Update()
    {
        if (!partidaEnCurso) return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarUI();
            TiempoAgotado();
            return;
        }

        ActualizarUI();
    }
    public void IniciarPartida()
    {
        tiempoRestante = tiempoLimite;
        partidaEnCurso = true;
        ActualizarUI();
    }
    public void AlcanzarMeta()
    {
        if (!partidaEnCurso) return;

        partidaEnCurso = false;
        onMetaAlcanzada?.Invoke();
    }
    private void TiempoAgotado()
    {
        if (!partidaEnCurso) return;

        partidaEnCurso = false;
        onTiempoAgotado?.Invoke();
    }
    public void DestruirVehiculo()
    {
        if (vehiculoJugador != null)
        {
            Destroy(vehiculoJugador);
        }
    }
    private void ActualizarUI()
    {
        if (timerText == null) return;

        int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }
    public float TiempoRestante => tiempoRestante;
    public bool PartidaEnCurso => partidaEnCurso;
}