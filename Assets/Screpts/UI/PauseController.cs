using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if TMP_PRESENT
using TMPro;
#endif

public class PauseController : MonoBehaviour
{
    [Header("Referencias UI")]
    [Tooltip("Canvas (GameObject) que contiene la UI de pausa. Debe estar desactivado al inicio.")]
    public GameObject pauseCanvas;

    [Tooltip("RawImage dentro del canvas que mostrará la captura de la pantalla (fondo difuminado).")]
    public RawImage backgroundRawImage;

    [Tooltip("Panel oscuro encima del RawImage para atenuar el fondo (Image).")]
    public Image overlayImage;

    [Tooltip("Texto central que mostrará 'PAUSE' (Text o TMP).")]
    public Text pauseTextLegacy;
#if TMP_PRESENT
    public TMP_Text pauseTextTMP;
#endif

    [Header("Opciones de difuminado")]
    [Tooltip("Material con shader de blur (opcional). Si es null se usará solo la atenuación.")]
    public Material blurMaterial;

    [Tooltip("Intensidad de oscuridad encima del fondo (0..1).")]
    [Range(0f, 1f)]
    public float overlayAlpha = 0.45f;

    [Header("Comportamiento")]
    [Tooltip("Si true, pondrá Time.timeScale = 0 al pausar.")]
    public bool freezeTime = true;

    // Internals
    private RenderTexture captureRT;
    private bool isPaused = false;

    private void Awake()
    {
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (overlayImage != null) SetOverlayAlpha(overlayAlpha);
        SetPauseText("PAUSE");
    }

    private void Update()
    {
        // Toggle con tecla Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (isPaused) return;

        // Capturar pantalla y aplicar al RawImage
        StartCoroutine(CaptureAndShowBackground());

        if (freezeTime) Time.timeScale = 0f;
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        isPaused = true;
    }

    public void Resume()
    {
        if (!isPaused) return;

        // Ocultar canvas y liberar RT
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (backgroundRawImage != null)
        {
            if (backgroundRawImage.texture is RenderTexture rt)
            {
                backgroundRawImage.texture = null;
                rt.Release();
                Destroy(rt);
            }
            backgroundRawImage.material = null;
        }

        if (freezeTime) Time.timeScale = 1f;
        isPaused = false;
    }

    private IEnumerator CaptureAndShowBackground()
    {
        // Esperar fin de frame para capturar lo que se ve
        yield return new WaitForEndOfFrame();

        int w = Screen.width;
        int h = Screen.height;
        captureRT = new RenderTexture(w, h, 0);
        captureRT.Create();

        // Renderizar la cámara principal a RenderTexture
        Camera cam = Camera.main;
        if (cam != null)
        {
            var prevTarget = cam.targetTexture;
            cam.targetTexture = captureRT;
            cam.Render();
            cam.targetTexture = prevTarget;
        }
        else
        {
            // Fallback: ReadPixels (más lento)
            Texture2D snap = new Texture2D(w, h, TextureFormat.RGB24, false);
            snap.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            snap.Apply();
            Graphics.Blit(snap, captureRT);
            Destroy(snap);
        }

        // Asignar material de blur si hay
        if (backgroundRawImage != null)
        {
            backgroundRawImage.texture = captureRT;
            if (blurMaterial != null)
                backgroundRawImage.material = blurMaterial;
            else
                backgroundRawImage.material = null;
        }

        // Asegurar overlay alpha y activar canvas
        if (overlayImage != null) SetOverlayAlpha(overlayAlpha);
        if (pauseCanvas != null) pauseCanvas.SetActive(true);
    }

    private void SetOverlayAlpha(float a)
    {
        if (overlayImage == null) return;
        Color c = overlayImage.color;
        c.a = Mathf.Clamp01(a);
        overlayImage.color = c;
    }

    private void SetPauseText(string text)
    {
#if TMP_PRESENT
        if (pauseTextTMP != null) pauseTextTMP.text = text;
        else
#endif
        if (pauseTextLegacy != null) pauseTextLegacy.text = text;
    }

    private void OnDisable()
    {
        // Asegurar que restauramos timeScale si el objeto se destruye
        if (freezeTime) Time.timeScale = 1f;
    }
}