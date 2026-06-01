using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CamaraControllerNuevo : MonoBehaviour
{
    [Header("Comic data (ScriptableObject)")]
    public ComicSequence comicData;                // Asigna aquí el ScriptableObject con las viñetas
    public Canvas comicCanvas;
    public Image comicImage;

    [Header("Opciones")]
    [Tooltip("Si true, la secuencia comenzará automáticamente cuando la escena se muestre (Start).")]
    public bool playOnSceneVisible = true;

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;
    [Tooltip("Índice de JoystickButton (0 = A/Cross, 1 = B/Circle, 2 = X/Square, ...).")]
    public int joystickButtonIndex = 0;

    [Header("Opcional: desactivar Player")]
    public MonoBehaviour playerScriptToDisable;

    int index = -1;
    bool playing = false;
    CanvasGroup canvasGroup;

    private void Start()
    {
        if (comicCanvas != null)
        {
            canvasGroup = comicCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = comicCanvas.gameObject.AddComponent<CanvasGroup>();
            comicCanvas.enabled = false;
            canvasGroup.alpha = 0f;
        }

        if (playOnSceneVisible && comicData != null)
        {
            StartComic();
        }
    }

    private void Update()
    {
        if (!playing) return;

        bool interact = Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Space) || IsJoystickButtonDown(joystickButtonIndex);

        if (comicData != null && comicData.waitForInteract && interact)
        {
            AdvancePanel();
        }
    }

    bool IsJoystickButtonDown(int idx)
    {
        if (idx < 0) return false;
        KeyCode code = KeyCode.JoystickButton0 + idx;
        return Input.GetKeyDown(code);
    }

    // Llamar para iniciar la secuencia (por ejemplo desde UI de Nuevo Juego)
    public void StartComic()
    {
        if (comicData == null || comicData.panels == null || comicData.panels.Count == 0 || comicCanvas == null || comicImage == null)
        {
            Debug.LogWarning("CamaraControllerNuevo: faltan asignaciones (comicData/comicCanvas/comicImage) o lista vacía.");
            return;
        }

        if (playerScriptToDisable != null) playerScriptToDisable.enabled = false;

        playing = true;
        index = -1;
        comicCanvas.enabled = true;
        canvasGroup.alpha = 1f;
        AdvancePanel();
    }

    void AdvancePanel()
    {
        index++;
        if (index >= comicData.panels.Count)
        {
            EndComic();
            return;
        }

        Sprite next = comicData.panels[index];
        if (comicData.fadeDuration > 0f)
            StartCoroutine(FadeToSprite(next, comicData.fadeDuration));
        else
            comicImage.sprite = next;

        if (!comicData.waitForInteract)
        {
            StopCoroutine(nameof(AutoAdvanceCoroutine));
            StartCoroutine(AutoAdvanceCoroutine());
        }
    }

    IEnumerator AutoAdvanceCoroutine()
    {
        yield return new WaitForSeconds(comicData.autoAdvanceDelay);
        AdvancePanel();
    }

    IEnumerator FadeToSprite(Sprite next, float fadeDuration)
    {
        if (canvasGroup == null)
        {
            comicImage.sprite = next;
            yield break;
        }

        float half = fadeDuration * 0.5f;
        float t = 0f;
        float start = canvasGroup.alpha;

        // fade out
        while (t < half)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, 0f, t / half);
            yield return null;
        }

        comicImage.sprite = next;

        // fade in
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / half);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    void EndComic()
    {
        playing = false;
        if (playerScriptToDisable != null) playerScriptToDisable.enabled = true;
        if (comicCanvas != null)
        {
            canvasGroup.alpha = 0f;
            comicCanvas.enabled = false;
        }

        // Usar Esenamanager si existe, si no fallback a cargar escena
        if (Esenamanager.Instance != null)
        {
            Esenamanager.Instance.CheckAndLoad();
            return;
        }

        if (comicData.loadNextScene)
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(next);
            else Debug.LogWarning("CamaraControllerNuevo: no hay escena siguiente en Build Settings.");
        }
        else
        {
            if (!string.IsNullOrEmpty(comicData.sceneName)) SceneManager.LoadScene(comicData.sceneName);
            else Debug.LogWarning("CamaraControllerNuevo: sceneName vacío y loadNextScene desactivado.");
        }
    }

    // Métodos públicos de utilidad
    public void ForceAdvance() => AdvancePanel();
    public void ForceEnd() => EndComic();
}