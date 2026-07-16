using UnityEngine;
using System; // Necesario para los Callbacks (Action)

public class MiningMinigame : MonoBehaviour
{
    // Singleton para acceder fácilmente desde cualquier Mina
    public static MiningMinigame Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject contenedorUI; // El panel principal que contiene el minijuego
    public RectTransform needle;
    public RectTransform targetArea;
    public RectTransform backgroundBar;

    [Header("Ajustes del Juego")]
    public float needleSpeed = 400f;
    
    private int direction = 1;
    private float halfBarWidth;
    private bool juegoActivo = false;

    // Esta variable guardará la función que le pasemos desde Mina.cs
    private Action<bool> onMinigameComplete;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        contenedorUI.SetActive(false); // Ocultar al inicio
    }

    void Start()
    {
        halfBarWidth = backgroundBar.rect.width / 2f;
    }

    void Update()
    {
        if (!juegoActivo) return;

        MoveNeedle();

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            CheckMiningResult();
        }
    }

    // Función que llama Mina.cs para empezar
    public void IniciarMinijuego(Action<bool> callbackResultado)
    {
        onMinigameComplete = callbackResultado;
        
        // Reiniciar posición de la aguja y activar UI
        needle.anchoredPosition = new Vector2(-halfBarWidth, needle.anchoredPosition.y);
        direction = 1;
        
        contenedorUI.SetActive(true);
        juegoActivo = true;
    }

    private void MoveNeedle()
    {
        needle.anchoredPosition += new Vector2(needleSpeed * direction * Time.deltaTime, 0);

        if (needle.anchoredPosition.x >= halfBarWidth)
        {
            needle.anchoredPosition = new Vector2(halfBarWidth, needle.anchoredPosition.y);
            direction = -1;
        }
        else if (needle.anchoredPosition.x <= -halfBarWidth)
        {
            needle.anchoredPosition = new Vector2(-halfBarWidth, needle.anchoredPosition.y);
            direction = 1;
        }
    }

    private void CheckMiningResult()
    {
        juegoActivo = false;
        float needlePos = needle.anchoredPosition.x;
        float targetMinX = targetArea.anchoredPosition.x - (targetArea.rect.width / 2f);
        float targetMaxX = targetArea.anchoredPosition.x + (targetArea.rect.width / 2f);

        bool exito = (needlePos >= targetMinX && needlePos <= targetMaxX);

        // Ocultar UI y enviar el resultado de vuelta a Mina.cs
        contenedorUI.SetActive(false);
        onMinigameComplete?.Invoke(exito);
    }
}