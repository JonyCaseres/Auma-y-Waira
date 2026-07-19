using UnityEngine;
using UnityEngine.Events;
using System;

public class MiningMinigame : MonoBehaviour
{
    public static MiningMinigame Instance { get; private set; }

    [Header("UI References")]
    public GameObject contenedorUI;
    public RectTransform trackArea;    // La barra de fondo
    public RectTransform successZone;  // La zona verde
    public RectTransform marker;       // La aguja/pica

    [Header("Settings")]
    public float speed = 1.5f; // Velocidad del marcador

    [Header("Efectos y 'Juice' (Asignar en Inspector)")]
    public UnityEvent OnMineSuccess; // Conecta aquí partículas o sonidos de éxito
    public UnityEvent OnMineFail;    // Conecta aquí sonidos de rebote en la piedra

    private Action<bool> onMinigameComplete;

    // Máquina de estados interna
    private enum State { Idle, Running, Result }
    private State currentState = State.Idle;

    private float t = 0f; // Posición normalizada (0 a 1)
    private int direction = 1; // 1 (derecha/arriba), -1 (izquierda/abajo)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        contenedorUI.SetActive(false);
    }

    private void Update()
    {
        if (currentState != State.Running) return;

        UpdateMarker();

        // Detectar input (puedes cambiarlo al New Input System si lo prefieres)
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            CheckResult();
        }
    }

    public void IniciarMinijuego(Action<bool> callbackResultado)
    {
        onMinigameComplete = callbackResultado;
        
        // Reiniciar variables
        t = 0f;
        direction = 1;
        
        contenedorUI.SetActive(true);
        currentState = State.Running;
    }

    private void UpdateMarker()
    {
        // Mover t entre 0 y 1
        t += speed * direction * Time.deltaTime;

        if (t >= 1f)
        {
            t = 1f;
            direction = -1;
        }
        else if (t <= 0f)
        {
            t = 0f;
            direction = 1;
        }

        // Aplicar la posición usando Lerp (asumiendo movimiento horizontal)
        // Si tu barra es vertical, cambia anchoredPosition.x por anchoredPosition.y
        float width = trackArea.rect.width;
        float xPos = Mathf.Lerp(-width / 2f, width / 2f, t);
        marker.anchoredPosition = new Vector2(xPos, marker.anchoredPosition.y);
    }

    private void CheckResult()
    {
        currentState = State.Result;

        // Lógica inspirada en Candrapota: comprobar si el marcador está dentro de los límites de la zona verde
        float markerPos = marker.anchoredPosition.x;
        float zoneMin = successZone.anchoredPosition.x - (successZone.rect.width / 2f);
        float zoneMax = successZone.anchoredPosition.x + (successZone.rect.width / 2f);

        bool isSuccess = markerPos >= zoneMin && markerPos <= zoneMax;

        if (isSuccess)
        {
            OnMineSuccess?.Invoke();
        }
        else
        {
            OnMineFail?.Invoke();
        }

        // Pequeño retraso para que el jugador vea dónde detuvo la aguja antes de cerrar
        Invoke(nameof(CloseMinigame), 0.5f);
    }

    private void CloseMinigame()
    {
        contenedorUI.SetActive(false);
        currentState = State.Idle;
        
        // Ejecutar la lógica de Mina.cs (entregar los ítems basándose en si fue un éxito)
        // Tomamos el estado del último CheckResult
        float markerPos = marker.anchoredPosition.x;
        float zoneMin = successZone.anchoredPosition.x - (successZone.rect.width / 2f);
        float zoneMax = successZone.anchoredPosition.x + (successZone.rect.width / 2f);
        bool isSuccess = markerPos >= zoneMin && markerPos <= zoneMax;

        onMinigameComplete?.Invoke(isSuccess);
    }
}