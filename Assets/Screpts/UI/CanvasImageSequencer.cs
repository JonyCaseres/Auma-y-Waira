using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CanvasImageSequencer : MonoBehaviour
{
    public enum TransitionMode { Instant, Fade }

    [Header("UI")]
    public Image targetImage; // Image del Canvas donde se mostrarán las viñetas

    [Header("Panels (sprites)")]
    public List<Sprite> sprites = new List<Sprite>();

    [Header("Control")]
    public bool playOnStart = false;
    public bool waitForInput = true;           // si true espera tecla/botón; si false avanza automáticamente
    public KeyCode advanceKey = KeyCode.E;     // tecla para avanzar (apoya Input Manager)
    public int joystickButtonIndex = 0;        // joystick button index (0 = A/Cross)
    public float autoAdvanceDelay = 1.5f;      // retraso entre paneles en modo automático
    public bool loop = false;                  // repetir al final

    [Header("Visual")]
    public TransitionMode transition = TransitionMode.Instant;
    public float fadeDuration = 0.2f;          // para TransitionMode.Fade (0 = instantáneo)

    // Estado interno
    private int index = -1;
    private bool playing = false;
    private Coroutine autoCoroutine;
    private Coroutine transitionCoroutine;

    private void Start()
    {
        if (targetImage == null)
        {
            Debug.LogWarning("CanvasImageSequencer: targetImage no asignada.");
            return;
        }

        // Asegurar alpha inicial
        var c = targetImage.color;
        c.a = 1f;
        targetImage.color = c;

        if (playOnStart)
            StartSequence();
    }

    private void Update()
    {
        if (!playing) return;

        if (waitForInput)
        {
            if (IsAdvancePressed())
            {
                Next();
            }
        }
    }

    private bool IsAdvancePressed()
    {
        // Priorizar nuevo Input System si está activo en el proyecto
#if ENABLE_INPUT_SYSTEM
        // Try keyboard with new Input System
        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame) return true;
            if (Keyboard.current.spaceKey.wasPressedThisFrame) return true;
        }
        // Try gamepad button south (A/Cross) or mapped joystick index
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame) return true;
        }
        // Also fallback to legacy checks below if needed
#endif
        // Legacy Input
        if (Input.GetKeyDown(advanceKey)) return true;
        if (Input.GetKeyDown(KeyCode.Space)) return true;

        // Joystick button by index (legacy)
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

        // Si hay una transición en curso y queremos cambiar ahora, interrumpirla
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

        ShowSprite(sprites[index]);

        if (!waitForInput)
        {
            if (autoCoroutine != null) StopCoroutine(autoCoroutine);
            autoCoroutine = StartCoroutine(AutoAdvanceCoroutine());
        }
    }

    // Retrocede (opcional)
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

        ShowSprite(sprites[index]);
    }

    private IEnumerator AutoAdvanceCoroutine()
    {
        yield return new WaitForSeconds(autoAdvanceDelay);
        Next();
    }

    private void ShowSprite(Sprite s)
    {
        if (targetImage == null) return;

        if (transition == TransitionMode.Instant || fadeDuration <= 0f)
        {
            // Cambio instantáneo
            targetImage.sprite = s;
            // Asegurar alpha visible
            var col = targetImage.color;
            col.a = 1f;
            targetImage.color = col;
        }
        else
        {
            // Fade transition, interrumpible
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(FadeToSprite(s, fadeDuration));
        }
    }

    private IEnumerator FadeToSprite(Sprite next, float duration)
    {
        // Guardar color
        Color col = targetImage.color;
        float half = duration * 0.5f;
        float t = 0f;
        float startA = col.a;

        // Fade out (si ya está visible)
        while (t < half)
        {
            t += Time.deltaTime;
            col.a = Mathf.Lerp(startA, 0f, t / half);
            targetImage.color = col;
            yield return null;
        }

        // Cambiar sprite inmediatamente
        targetImage.sprite = next;

        // Fade in
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

    // Finaliza la secuencia
    public void EndSequence()
    {
        playing = false;
        if (autoCoroutine != null) { StopCoroutine(autoCoroutine); autoCoroutine = null; }
        if (transitionCoroutine != null) { StopCoroutine(transitionCoroutine); transitionCoroutine = null; }
    }

    // Reemplazar la lista de sprites en tiempo de ejecución
    public void SetSprites(List<Sprite> newSprites)
    {
        sprites = newSprites ?? new List<Sprite>();
        index = -1;
    }

    // Añadir un sprite al final
    public void AddSprite(Sprite s)
    {
        if (sprites == null) sprites = new List<Sprite>();
        sprites.Add(s);
    }
}