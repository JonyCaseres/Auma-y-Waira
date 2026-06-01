using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class NPCEspecial : MonoBehaviour, IInteractable                 
{
    [Header("Datos del diálogo inicial")]
    public NPCDialogue dialogueData;

    [Header("Datos de preguntas/respuestas")]
    public NPCCuestion questionData;

    [Header("UI Referencias")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Paneles de opciones (cada uno debe contener un Button)")]
    public GameObject panelRespuesta1;
    public TMP_Text textoRespuesta1;
    public Button buttonRespuesta1;

    public GameObject panelRespuesta2;
    public TMP_Text textoRespuesta2;
    public Button buttonRespuesta2;

    [Header("Audio")]
    public AudioSource voiceSource;
    public AudioClip voiceClip;
    [Range(0f, 1f)]
    public float voiceVolume = 1f;

    [Header("Save")]
    [Tooltip("Índice (1..2) de la opción que significa 'aceptar misión'.")]
    public int acceptOptionIndex = 1;

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool jugadorCerca;
    private bool mostrandoPreguntas;
    private bool yaRespondio;

    private string ultimoDialogo;

    private Coroutine typingCoroutine;
    private Coroutine respuestaCoroutine;

    // Navegación manual fallback
    private Button[] optionButtons;
    private int selectedIndex = 0;
    private float navCooldown = 0.18f;
    private float lastNavTime;

    private void Awake()
    {
        if (voiceSource == null)
        {
            voiceSource = GetComponent<AudioSource>();
            if (voiceSource == null)
                voiceSource = gameObject.AddComponent<AudioSource>();
        }

        voiceSource.loop = true;
        voiceSource.playOnAwake = false;

        // Ocultar panels al inicio si están asignados
        if (panelRespuesta1 != null) panelRespuesta1.SetActive(false);
        if (panelRespuesta2 != null) panelRespuesta2.SetActive(false);

        optionButtons = new Button[] { buttonRespuesta1, buttonRespuesta2 };
    }

    private void Start()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.GetTalkedToGrandma())
        {
            yaRespondio = true;
        }
    }

    private void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            if (!isDialogueActive)
            {
                if (yaRespondio) MostrarUltimoDialogo();
                else StartDialogue();
            }
        }

        if (isDialogueActive && !mostrandoPreguntas && Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }

        if (mostrandoPreguntas)
        {
            HandleControllerAndKeyboardSelection();
        }
    }

    private void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        dialoguePanel.SetActive(true);
        typingCoroutine = StartCoroutine(TypeLine());

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetTalkedToGrandma(true);
            yaRespondio = true;
            Debug.Log("NPCEspecial: marcado talkedToGrandma = true y guardado.");
        }
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText("");

        StartVoice();

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        StopVoice();

        isTyping = false;
        ultimoDialogo = dialogueData.dialogueLines[dialogueIndex];

        if (dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    private void NextLine()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            StopVoice();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (dialogueIndex + 1 < dialogueData.dialogueLines.Length)
        {
            dialogueIndex++;
            typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            if (!yaRespondio) MostrarPreguntas();
            else EndDialogue();
        }
    }

    private void MostrarPreguntas()
    {
        mostrandoPreguntas = true;

        if (panelRespuesta1 != null) panelRespuesta1.SetActive(true);
        if (panelRespuesta2 != null) panelRespuesta2.SetActive(true);

        if (textoRespuesta1 != null) textoRespuesta1.text = questionData.opcion1;
        if (textoRespuesta2 != null) textoRespuesta2.text = questionData.opcion2;

        // Assign listeners
        if (buttonRespuesta1 != null)
        {
            buttonRespuesta1.onClick.RemoveAllListeners();
            buttonRespuesta1.onClick.AddListener(() => SeleccionarOpcion(1, questionData.respuesta1));
        }
        if (buttonRespuesta2 != null)
        {
            buttonRespuesta2.onClick.RemoveAllListeners();
            buttonRespuesta2.onClick.AddListener(() => SeleccionarOpcion(2, questionData.respuesta2));
        }

        // Seleccionar primer botón para permitir navegación con mando
        selectedIndex = 0;
        SelectButtonByIndex(selectedIndex);

        // Opcional: empezar effect de escritura en opciones (no bloqueante)
        StartCoroutine(TypeOption(textoRespuesta1, questionData.opcion1));
        StartCoroutine(TypeOption(textoRespuesta2, questionData.opcion2));
    }

    private IEnumerator TypeOption(TMP_Text texto, string opcion)
    {
        if (texto == null) yield break;
        texto.SetText("");
        StartVoice();
        foreach (char letter in opcion.ToCharArray())
        {
            texto.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        StopVoice();
    }

    private void SeleccionarOpcion(int optionIndex, string[] respuesta)
    {
        mostrandoPreguntas = false;
        yaRespondio = true;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetTalkedToGrandma(true);
            Debug.Log("NPCEspecial: guardado talkedToGrandma true al seleccionar opción.");

            if (optionIndex == acceptOptionIndex)
            {
                SaveManager.Instance.SetMissionAccepted(true);
                Debug.Log("NPCEspecial: guardado missionAccepted true.");
            }
        }
        else
        {
            Debug.LogWarning("NPCEspecial: SaveManager no encontrado al seleccionar opción.");
        }

        if (panelRespuesta1 != null) panelRespuesta1.SetActive(false);
        if (panelRespuesta2 != null) panelRespuesta2.SetActive(false);

        if (buttonRespuesta1 != null) buttonRespuesta1.onClick.RemoveAllListeners();
        if (buttonRespuesta2 != null) buttonRespuesta2.onClick.RemoveAllListeners();

        dialoguePanel.SetActive(true);
        respuestaCoroutine = StartCoroutine(TypeRespuesta(dialogueText, respuesta));

        if (Esenamanager.Instance != null)
        {
            Esenamanager.Instance.CheckAndLoad();
        }
    }

    private IEnumerator TypeRespuesta(TMP_Text texto, string[] respuesta)
    {
        foreach (string linea in respuesta)
        {
            texto.SetText("");
            StartVoice();
            foreach (char letter in linea.ToCharArray())
            {
                texto.text += letter;
                yield return new WaitForSeconds(dialogueData.typingSpeed);
            }
            StopVoice();
            ultimoDialogo = linea;
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }
        EndDialogue();
    }

    private void MostrarUltimoDialogo()
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);
        if (string.IsNullOrEmpty(ultimoDialogo)) ultimoDialogo = "No hemos hablado todavía...";
        dialogueText.SetText(ultimoDialogo);
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (respuestaCoroutine != null) StopCoroutine(respuestaCoroutine);
        StopVoice();
        isDialogueActive = false;
        mostrandoPreguntas = false;
        dialogueIndex = 0;
        if (panelRespuesta1 != null) panelRespuesta1.SetActive(false);
        if (panelRespuesta2 != null) panelRespuesta2.SetActive(false);
        dialoguePanel.SetActive(false);
    }

    private void StartVoice()
    {
        if (voiceSource == null) return;
        AudioClip clipToPlay = (voiceClip != null) ? voiceClip : (dialogueData != null ? dialogueData.voiceSound : null);
        if (clipToPlay == null) return;
        voiceSource.clip = clipToPlay;
        voiceSource.pitch = (dialogueData != null) ? dialogueData.voicePitch : 1f;
        voiceSource.volume = voiceVolume;
        voiceSource.loop = true;
        voiceSource.Play();
    }

    private void StopVoice()
    {
        if (voiceSource == null) return;
        if (voiceSource.isPlaying) voiceSource.Stop();
        voiceSource.clip = null;
    }

    // -------------------------
    // Navegación manual fallback
    // -------------------------
    private void HandleControllerAndKeyboardSelection()
    {
        bool submit = Input.GetKeyDown(KeyCode.JoystickButton0)
                      || Input.GetKeyDown(KeyCode.Return)
                      || Input.GetKeyDown(KeyCode.KeypadEnter)
                      || Input.GetKeyDown(KeyCode.Space)
                      || Input.GetButtonDown("Submit");

        if (submit)
        {
            if (EventSystem.current != null)
            {
                var sel = EventSystem.current.currentSelectedGameObject;
                if (sel != null)
                {
                    var btn = sel.GetComponent<Button>();
                    if (btn != null && btn.interactable) { btn.onClick.Invoke(); return; }
                }
            }
            InvokeSelectedIndex();
            return;
        }

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        if (Time.time - lastNavTime < navCooldown) return;

        if (Mathf.Abs(v) > 0.5f)
        {
            if (v > 0f) MoveSelection(-1);
            else MoveSelection(1);
            lastNavTime = Time.time;
        }
        else if (Mathf.Abs(h) > 0.5f)
        {
            if (h < 0f) MoveSelection(-1); else MoveSelection(1);
            lastNavTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.Z)) InvokeButtonAtIndex(0);
        if (Input.GetKeyDown(KeyCode.X)) InvokeButtonAtIndex(1);
    }

    private void MoveSelection(int delta)
    {
        int count = optionButtons.Length;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            selectedIndex = (selectedIndex + delta + count) % count;
            if (optionButtons[selectedIndex] != null && optionButtons[selectedIndex].interactable) break;
        }

        SelectButtonByIndex(selectedIndex);
    }

    private void SelectButtonByIndex(int idx)
    {
        if (idx < 0 || idx >= optionButtons.Length) return;
        var btn = optionButtons[idx];
        if (btn == null) return;
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(btn.gameObject);
        }
    }

    private void InvokeSelectedIndex()
    {
        if (selectedIndex >= 0 && selectedIndex < optionButtons.Length)
            InvokeButtonAtIndex(selectedIndex);
    }

    private void InvokeButtonAtIndex(int idx)
    {
        if (idx < 0 || idx >= optionButtons.Length) return;
        var btn = optionButtons[idx];
        if (btn != null && btn.interactable)
            btn.onClick.Invoke();
    }

    // --- IInteractable ---
    public bool CanInteract()
    {
        return !isDialogueActive && !mostrandoPreguntas;
    }

    public void Interact(GameObject jugador)
    {
        if (!isDialogueActive)
        {
            if (yaRespondio) MostrarUltimoDialogo();
            else StartDialogue();
        }
        else
        {
            if (!mostrandoPreguntas) NextLine();
        }
    }
}




