using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using TMPro;

public class CanvasImageSequencer : MonoBehaviour
{
    public enum TransitionMode { Instant, Fade }

    [Header("UI")]
    public Image targetImage; // Image del Canvas donde se mostrarán las viñetas

    [Header("Panels (sprites)")]
    public List<Sprite> sprites = new List<Sprite>();

    [Header("Captions")]
    [Tooltip("Texto para cada sprite en el mismo orden. Si la lista es más corta, los captions faltantes quedarán vacíos.")]
    public List<string> captions = new List<string>();
    [Tooltip("Texto UI (legacy).")]
    public Text uiCaptionText;
    [Tooltip("Texto TMP (preferible).")]
    public TMP_Text tmpCaptionText;

    [Header("Last panel button")]
    [Tooltip("Botón que aparecerá solo en la última viñeta. Configura su OnClick desde el Inspector.")]
    public Button lastPanelButton;
    [Tooltip("Si quieres, texto que reemplaza el texto del botón asignado (opcional).")]
    public string lastButtonLabel;

    [Header("Control")]
    public bool playOnStart = false;
    public bool waitForInput = true;           // si true espera tecla/botón; si false avanza automáticamente
    public KeyCode advanceKey = KeyCode.E;     // tecla para avanzar (apoya Input Manager)
    public int joystickButtonIndex = 0;        // joystick button index (0 = A/Cross)
    public float autoAdvanceDelay = 1.5f;      // retraso entre paneles en modo automático
    public bool loop = false;                  // repetir al final
    [Tooltip("Si true, ignorará si hay un elemento UI seleccionado y aún así aceptará la tecla.")]
    public bool ignoreUIFocus = false;

    [Header("Visual")]
    public TransitionMode transition = TransitionMode.Instant;
    public float fadeDuration = 0.2f;          // para TransitionMode.Fade (0 = instantáneo)

    [Header("Final action")]
    [Tooltip("Nombre de la escena a cargar cuando el usuario pulse E tras terminar la secuencia.")]
    public string finalSceneName = "Pueblo";

    [Header("Debug")]
    public bool debugMode = false;

    // Estado interno
    private int index = -1;
    private bool playing = false;
    private bool sequenceCompleted = false; // nueva flag: secuencia completa
    private Coroutine autoCoroutine;
    private Coroutine transitionCoroutine;

    private void Start()
    {
        if (targetImage == null)
        {
            Debug.LogWarning("CanvasImageSequencer: targetImage no asignada.");
            return;
        }

        // Inicializar alpha y ocultar botón de último panel
        var c = targetImage.color;
        c.a = 1f;
        targetImage.color = c;

        if (lastPanelButton != null)
        {
            lastPanelButton.gameObject.SetActive(false);
            // Opcional: cambiar label del botón si existe Text/TMP en su hijo
            if (!string.IsNullOrEmpty(lastButtonLabel))
            {
                var t = lastPanelButton.GetComponentInChildren<Text>();
                if (t != null) t.text = lastButtonLabel;
                var tt = lastPanelButton.GetComponentInChildren<TMP_Text>();
                if (tt != null) tt.text = lastButtonLabel;
            }
        }

        // Avisa si captions y sprites no coinciden (solo como ayuda)
        if (captions != null && captions.Count > 0 && captions.Count != sprites.Count)
        {
            Debug.LogWarning($"CanvasImageSequencer: cantidad de captions ({captions.Count}) no coincide con sprites ({sprites.Count}).");
        }

        if (playOnStart)
            StartSequence();
    }

    private void Update()
    {
        if (!playing && !sequenceCompleted) return;

        // Si la secuencia ya terminó: esperar pulsación para cargar escena
        if (sequenceCompleted)
        {
            if (IsAdvancePressed())
            {
                if (debugMode) Debug.Log("CanvasImageSequencer: Advance detected after sequence -> loading scene.");
                TryLoadFinalScene();
            }
            return;
        }

        if (waitForInput)
        {
            if (!ignoreUIFocus && EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                if (debugMode) Debug.Log($"CanvasImageSequencer: UI focus on {EventSystem.current.currentSelectedGameObject.name}, ignoring input.");
            }
            else
            {
                if (IsAdvancePressed())
                {
                    if (debugMode) Debug.Log("CanvasImageSequencer: Advance detected in Update.");
                    Next();
                }
            }
        }
    }

    private void OnGUI()
    {
        if ((!playing && !sequenceCompleted) || !waitForInput) return;

        Event e = Event.current;
        if (e != null && e.type == EventType.KeyDown)
        {
            if (e.keyCode == advanceKey || e.keyCode == KeyCode.Space)
            {
                if (!ignoreUIFocus && EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                {
                    if (debugMode) Debug.Log("CanvasImageSequencer: OnGUI key detected but UI has focus; ignoring.");
                    return;
                }

                if (debugMode) Debug.Log($"CanvasImageSequencer: OnGUI detected key {e.keyCode}");
                if (sequenceCompleted)
                    TryLoadFinalScene();
                else
                    Next();
                e.Use();
            }
        }
    }

    private bool IsAdvancePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame) return true;
            if (Keyboard.current.spaceKey.wasPressedThisFrame) return true;
        }
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame) return true;
        }
#endif
        if (Input.GetKeyDown(advanceKey)) return true;
        if (Input.GetKeyDown(KeyCode.Space)) return true;
        if (joystickButtonIndex >= 0)
        {
            KeyCode code = KeyCode.JoystickButton0 + joystickButtonIndex;
            if (Input.GetKeyDown(code)) return true;
        }
        return false;
    }

    // Inicia la secuencia desde el primer panel
    public void StartSequence()
    {
        sequenceCompleted = false;
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogWarning("CanvasImageSequencer: lista de sprites vacía.");
            return;
        }

        playing = true;
        index = -1;
        if (autoCoroutine != null) { StopCoroutine(autoCoroutine); autoCoroutine = null; }
        Next();
    }

    // Avanza al siguiente panel
    public void Next()
    {
        if (sprites == null || sprites.Count == 0) return;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        index++;
        if (index >= sprites.Count)
        {
            if (loop) index = 0;
            else { EndSequence(); return; }
        }

        ShowSpriteAndCaption(sprites[index], index);

        if (!waitForInput)
        {
            if (autoCoroutine != null) StopCoroutine(autoCoroutine);
            autoCoroutine = StartCoroutine(AutoAdvanceCoroutine());
        }
    }

    public void Prev()
    {
        if (sprites == null || sprites.Count == 0) return;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        index--;
        if (index < 0)
        {
            if (loop) index = sprites.Count - 1;
            else { index = 0; }
        }

        ShowSpriteAndCaption(sprites[index], index);
    }

    private IEnumerator AutoAdvanceCoroutine()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        Next();
    }

    private void ShowSpriteAndCaption(Sprite s, int idx)
    {
        ShowSprite(s);

        // Caption
        string caption = "";
        if (captions != null && idx >= 0 && idx < captions.Count) caption = captions[idx];

        if (tmpCaptionText != null)
        {
            tmpCaptionText.gameObject.SetActive(!string.IsNullOrEmpty(caption));
            tmpCaptionText.text = caption;
        }
        if (uiCaptionText != null)
        {
            uiCaptionText.gameObject.SetActive(!string.IsNullOrEmpty(caption));
            uiCaptionText.text = caption;
        }

        // Botón en último panel
        bool isLast = (idx == sprites.Count - 1);
        if (lastPanelButton != null)
        {
            lastPanelButton.gameObject.SetActive(isLast);
        }
    }

    private void ShowSprite(Sprite s)
    {
        if (targetImage == null) return;

        if (transition == TransitionMode.Instant || fadeDuration <= 0f)
        {
            targetImage.sprite = s;
            var col = targetImage.color;
            col.a = 1f;
            targetImage.color = col;
        }
        else
        {
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(FadeToSprite(s, fadeDuration));
        }
    }

    private IEnumerator FadeToSprite(Sprite next, float duration)
    {
        Color col = targetImage.color;
        float half = duration * 0.5f;
        float t = 0f;
        float startA = col.a;

        while (t < half)
        {
            t += Time.deltaTime;
            col.a = Mathf.Lerp(startA, 0f, t / half);
            targetImage.color = col;
            yield return null;
        }

        targetImage.sprite = next;

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            col.a = Mathf.Lerp(0f, 1f, t / half);
            targetImage.color = col;
            yield return null;
        }

        col.a = 1f;
        targetImage.color = col;
        transitionCoroutine = null;
    }

    public void EndSequence()
    {
        playing = false;
        if (autoCoroutine != null) { StopCoroutine(autoCoroutine); autoCoroutine = null; }
        if (transitionCoroutine != null) { StopCoroutine(transitionCoroutine); transitionCoroutine = null; }

        // Ocultar/limpiar UI si corresponde
        if (targetImage != null)
        {
            var col = targetImage.color;
            col.a = 1f;
            targetImage.color = col;
        }
        if (uiCaptionText != null) uiCaptionText.gameObject.SetActive(false);
        if (tmpCaptionText != null) tmpCaptionText.gameObject.SetActive(false);
        if (lastPanelButton != null) lastPanelButton.gameObject.SetActive(false);

        // Marcar secuencia completada: la próxima pulsación de E cargará la escena final
        sequenceCompleted = true;
    }

    // Lógica para cargar la escena final: primero intenta llamar a iniciodeecena.IniciarJuego(), si no existe carga finalSceneName
    private void TryLoadFinalScene()
    {
        // evitar llamadas repetidas
        sequenceCompleted = false;

        var starter = FindObjectOfType<iniciodeecena>();
        if (starter != null)
        {
            starter.IniciarJuego();
            return;
        }

        if (!string.IsNullOrEmpty(finalSceneName))
        {
            SceneManager.LoadScene(finalSceneName);
        }
        else
        {
            Debug.LogWarning("CanvasImageSequencer: finalSceneName vacío y no se encontró iniciodeecena.");
        }
    }

    // Reemplazar la lista de sprites en tiempo de ejecución
    public void SetSprites(List<Sprite> newSprites)
    {
        sprites = newSprites ?? new List<Sprite>();
        index = -1;
    }

    public void AddSprite(Sprite s)
    {
        if (sprites == null) sprites = new List<Sprite>();
        sprites.Add(s);
    }

    // Método público para forzar avanzar desde otros scripts / botones
    public void Advance() => Next();
}