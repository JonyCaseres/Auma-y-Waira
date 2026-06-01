using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CamaraController : MonoBehaviour
{
    public enum Mode { Cinematic, Comic }

    [Header("General")]
    public Mode mode = Mode.Cinematic;
    [Tooltip("Si true, bloqueará la secuencia hasta que el jugador pulse Interact para avanzar (modo Comic).")]
    public bool waitForInteractInComic = true;

    [Header("Referencia de cámara")]
    public Camera mainCamera; // si no se asigna, se usará Camera.main
    [Tooltip("Plano Z donde medir el tamaño del frustum (solo para cámaras en perspectiva).")]
    public float referenceZ = 0f;

    [Header("Tiempos y curva (Cinematic)")]
    public float segmentDuration = 1.2f;
    public float pauseBetweenSegments = 0.2f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Opciones Cinematic")]
    public bool playOnStart = true;
    public bool loop = false;

    [Header("Modo Comic")]
    [Tooltip("Canvas que contiene la Image full-screen para mostrar viñetas.")]
    public Canvas comicCanvas;
    [Tooltip("Image dentro del Canvas donde se mostrarán los Sprites (stretch full screen).")]
    public Image comicImage;
    [Tooltip("Sprites que forman el comic en orden.")]
    public Sprite[] comicPanels;
    [Tooltip("Duración del fade entre paneles (0 = instantáneo).")]
    public float comicFadeDuration = 0.2f;
    [Tooltip("Si true, al terminar la secuencia se cargará la siguiente escena en Build Settings; si false usa sceneName.")]
    public bool loadNextScene = true;
    [Tooltip("Nombre de la escena a cargar si loadNextScene = false.")]
    public string sceneName = "";

    [Header("Input")]
    [Tooltip("Tecla de teclado para interactuar (por defecto E).")]
    public KeyCode interactKey = KeyCode.E;
    [Tooltip("Índice de JoystickButton que quieres usar para interactuar (0..).")]
    public int joystickButtonIndex = 2;

    // Estado interno
    private bool isPlaying = false;
    private int currentPanel = -1;
    private CanvasGroup comicCanvasGroup;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        // Preparar canvas group si es necesario
        if (comicCanvas != null)
        {
            comicCanvasGroup = comicCanvas.GetComponent<CanvasGroup>();
            if (comicCanvasGroup == null)
                comicCanvasGroup = comicCanvas.gameObject.AddComponent<CanvasGroup>();

            comicCanvas.enabled = false;
            comicCanvasGroup.alpha = 0f;
        }

        if (mode == Mode.Cinematic)
        {
            if (playOnStart) StartCoroutine(CinematicSequence());
        }
        else
        {
            if (playOnStart) StartComic();
        }
    }

    void Update()
    {
        bool interactPressed = Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Space) || IsJoystickButtonDown(joystickButtonIndex);

        if (mode == Mode.Comic && isPlaying && waitForInteractInComic && interactPressed)
        {
            AdvanceComic();
        }
    }

    private bool IsJoystickButtonDown(int index)
    {
        if (index < 0) return false;
        KeyCode code = KeyCode.JoystickButton0 + index;
        return Input.GetKeyDown(code);
    }

    // Comic control
    public void StartComic()
    {
        if (comicPanels == null || comicPanels.Length == 0 || comicImage == null || comicCanvas == null)
        {
            Debug.LogWarning("CamaraController: No hay panels o comicImage/comicCanvas no asignados.");
            return;
        }

        isPlaying = true;
        currentPanel = -1;
        comicCanvas.enabled = true;
        comicCanvasGroup.alpha = 1f;
        AdvanceComic();
    }

    private void AdvanceComic()
    {
        currentPanel++;
        if (currentPanel >= comicPanels.Length)
        {
            EndComicAndLoadScene();
            return;
        }

        if (comicFadeDuration > 0f)
            StartCoroutine(FadePanelToSprite(comicPanels[currentPanel]));
        else
            comicImage.sprite = comicPanels[currentPanel];
    }

    private IEnumerator FadePanelToSprite(Sprite next)
    {
        if (comicCanvasGroup == null)
        {
            comicImage.sprite = next;
            yield break;
        }

        float half = comicFadeDuration * 0.5f;
        float t = 0f;
        float start = comicCanvasGroup.alpha;
        while (t < half)
        {
            t += Time.deltaTime;
            comicCanvasGroup.alpha = Mathf.Lerp(start, 0f, t / half);
            yield return null;
        }

        comicImage.sprite = next;

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            comicCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / half);
            yield return null;
        }
        comicCanvasGroup.alpha = 1f;
    }

    private void EndComicAndLoadScene()
    {
        isPlaying = false;
        if (comicCanvas != null)
        {
            comicCanvasGroup.alpha = 0f;
            comicCanvas.enabled = false;
        }

        if (Esenamanager.Instance != null)
        {
            Esenamanager.Instance.CheckAndLoad();
            return;
        }

        if (loadNextScene)
        {
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(nextIndex);
            else
                Debug.LogWarning("CamaraController: No hay escena siguiente en Build Settings.");
        }
        else
        {
            if (!string.IsNullOrEmpty(sceneName))
                SceneManager.LoadScene(sceneName);
            else
                Debug.LogWarning("CamaraController: sceneName vacío y loadNextScene desactivado.");
        }
    }

    // Cinematic existente
    [ContextMenu("Play Cinematic")]
    public void PlayCinematic()
    {
        StopAllCoroutines();
        StartCoroutine(CinematicSequence());
    }

    IEnumerator CinematicSequence()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("CamaraController: No hay Camera asignada ni Camera.main disponible.");
            yield break;
        }

        float viewHeight, viewWidth;
        GetCameraWorldSizeAtZ(referenceZ, out viewHeight, out viewWidth);

        Vector3 startPos = transform.position;
        Vector3 up1 = startPos + Vector3.up * viewHeight;
        Vector3 up2 = up1 + Vector3.up * viewHeight;
        Vector3 right = up2 + Vector3.right * viewWidth;
        Vector3 down1 = right + Vector3.down * viewHeight;
        Vector3 down2 = down1 + Vector3.down * viewHeight;

        isPlaying = true;
        do
        {
            yield return MoveTo(up1, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(up2, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(right, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(down1, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);

            yield return MoveTo(down2, segmentDuration);
            yield return new WaitForSeconds(pauseBetweenSegments);
        }
        while (loop);

        isPlaying = false;
    }

    IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 initial = transform.position;
        if (duration <= 0f)
        {
            transform.position = target;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            float eased = ease.Evaluate(normalized);
            transform.position = Vector3.LerpUnclamped(initial, target, eased);
            yield return null;
        }
        transform.position = target;
    }

    void GetCameraWorldSizeAtZ(float z, out float height, out float width)
    {
        if (mainCamera.orthographic)
        {
            height = mainCamera.orthographicSize * 2f;
            width = height * mainCamera.aspect;
            return;
        }

        float camZ = mainCamera.transform.position.z;
        float distance = Mathf.Abs(z - camZ);
        if (distance < 0.0001f) distance = 0.0001f;
        float frustumHeight = 2f * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        height = frustumHeight;
        width = frustumHeight * mainCamera.aspect;
    }
}

public class Camaras : MonoBehaviour
{
    [Tooltip("Lista de cámaras a alternar")]
    public Camera[] cameras;

    [Tooltip("Índice inicial (0 = primera cámara)")]
    public int startIndex = 0;

    // Índice actual de cámara activa
    private int currentIndex = 0;

    // Índice de botón de joystick para alternar (opcional)
    public int joystickButtonIndex = 0;

    private void Awake()
    {
        if (cameras == null || cameras.Length == 0)
        {
            var found = FindObjectsOfType<Camera>();
            cameras = found;
        }

        currentIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, cameras.Length - 1));
        ApplyActiveCamera();
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                NextCamera();
                return;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Tab)) { NextCamera(); return; }
        }

        if (IsJoystickButtonDown(joystickButtonIndex))
        {
            NextCamera();
        }
    }

    private void NextCamera()
    {
        if (cameras == null || cameras.Length == 0) return;
        currentIndex = (currentIndex + 1) % cameras.Length;
        ApplyActiveCamera();
    }

    private void ApplyActiveCamera()
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] == null) continue;
            cameras[i].gameObject.SetActive(i == currentIndex);
        }
    }

    private bool IsJoystickButtonDown(int index)
    {
        if (index < 0) return false;
        KeyCode code = KeyCode.JoystickButton0 + index;
        return Input.GetKeyDown(code);
    }
}