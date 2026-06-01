using UnityEngine;
using TMPro;
using System.Collections;

public class NPCEspecial : MonoBehaviour
{
    [Header("Datos del diálogo inicial")]
    public NPCDialogue dialogueData;

    [Header("Datos de preguntas/respuestas")]
    public NPCCuestion questionData;

    [Header("UI Referencias")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Paneles de opciones")]
    public GameObject panelRespuesta1;
    public TMP_Text textoRespuesta1;
    public GameObject panelRespuesta2;
    public TMP_Text textoRespuesta2;
    public GameObject panelRespuesta3;
    public TMP_Text textoRespuesta3;

    [Header("Audio")]
    public AudioSource voiceSource; // opcional, si no se asigna se crea uno en Awake
    public AudioClip voiceClip;     // arrastra aquí tu .mp3/.wav/.ogg directamente
    [Range(0f, 1f)]
    public float voiceVolume = 1f;

    [Header("Save")]
    [Tooltip("Índice (1..3) de la opción que significa 'aceptar misión'. Ajusta en el Inspector.")]
    public int acceptOptionIndex = 1;

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool jugadorCerca;
    private bool mostrandoPreguntas;
    private bool yaRespondio;

    private string ultimoDialogo; // almacena el último diálogo mostrado

    private Coroutine typingCoroutine;
    private Coroutine respuestaCoroutine;

    private void Awake()
    {
        if (voiceSource == null)
        {
            voiceSource = GetComponent<AudioSource>();
            if (voiceSource == null)
            {
                voiceSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Controlaremos el loop y reproducción desde código
        voiceSource.loop = true;
        voiceSource.playOnAwake = false;
    }

    private void Start()
    {
        // Si SaveManager existe y ya habló antes, reflejarlo
        if (SaveManager.Instance != null && SaveManager.Instance.GetTalkedToGrandma())
        {
            yaRespondio = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jugadorCerca = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            jugadorCerca = false;
    }

    private void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            if (!isDialogueActive)
            {
                if (yaRespondio)
                {
                    MostrarUltimoDialogo();
                }
                else
                {
                    StartDialogue();
                }
            }
        }

        if (isDialogueActive && !mostrandoPreguntas && Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }

        if (mostrandoPreguntas)
        {
            if (Input.GetKeyDown(KeyCode.Z)) SeleccionarOpcion(1, questionData.respuesta1);
            if (Input.GetKeyDown(KeyCode.X)) SeleccionarOpcion(2, questionData.respuesta2);
            if (Input.GetKeyDown(KeyCode.C)) SeleccionarOpcion(3, questionData.respuesta3);
        }
    }

    private void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        dialoguePanel.SetActive(true);
        typingCoroutine = StartCoroutine(TypeLine());

        // Guardar que habló con la abuela en cuanto empiece el diálogo
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

        // Iniciar sonido mientras se escribe la línea
        StartVoice();

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        // Al completar la línea, detener sonido
        StopVoice();

        isTyping = false;

        // Guardar último diálogo mostrado
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

        panelRespuesta1.SetActive(true);
        panelRespuesta2.SetActive(true);
        panelRespuesta3.SetActive(true);

        StartCoroutine(TypeOption(textoRespuesta1, questionData.opcion1));
        StartCoroutine(TypeOption(textoRespuesta2, questionData.opcion2));
        StartCoroutine(TypeOption(textoRespuesta3, questionData.opcion3));
    }

    private IEnumerator TypeOption(TMP_Text texto, string opcion)
    {
        texto.SetText("");

        // Reproducir sonido mientras aparece la opción
        StartVoice();

        foreach (char letter in opcion.ToCharArray())
        {
            texto.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        StopVoice();
    }

    // Ahora recibe el índice de la opción seleccionada (1..3)
    private void SeleccionarOpcion(int optionIndex, string[] respuesta)
    {
        mostrandoPreguntas = false;
        yaRespondio = true; // marcar que ya respondió

        // Guardar que habló con la abuela
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetTalkedToGrandma(true);
            Debug.Log("NPCEspecial: guardado talkedToGrandma true al seleccionar opción.");

            // Si la opción seleccionada es la de aceptar misión, guardarlo
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

        panelRespuesta1.SetActive(false);
        panelRespuesta2.SetActive(false);
        panelRespuesta3.SetActive(false);

        dialoguePanel.SetActive(true);
        respuestaCoroutine = StartCoroutine(TypeRespuesta(dialogueText, respuesta));

        // Pedir comprobación de escena inmediatamente (si existe Esenamanager)
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

            // Iniciar sonido antes de escribir la línea de respuesta
            StartVoice();

            foreach (char letter in linea.ToCharArray())
            {
                texto.text += letter;
                yield return new WaitForSeconds(dialogueData.typingSpeed);
            }

            // Detener sonido al terminar la línea
            StopVoice();

            // Guardar último diálogo mostrado
            ultimoDialogo = linea;

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }
        EndDialogue();
    }

    private void MostrarUltimoDialogo()
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true);

        if (string.IsNullOrEmpty(ultimoDialogo))
            ultimoDialogo = "No hemos hablado todavía...";

        dialogueText.SetText(ultimoDialogo);
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (respuestaCoroutine != null) StopCoroutine(respuestaCoroutine);

        // Asegurarse de detener cualquier sonido pendiente
        StopVoice();

        isDialogueActive = false;
        mostrandoPreguntas = false;
        dialogueIndex = 0;

        // Ocultar paneles siempre al terminar
        panelRespuesta1.SetActive(false);
        panelRespuesta2.SetActive(false);
        panelRespuesta3.SetActive(false);

        dialoguePanel.SetActive(false);
    }

    // Reproducir sonido en bucle para la duración de la escritura de la línea
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

        if (voiceSource.isPlaying)
            voiceSource.Stop();

        voiceSource.clip = null;
    }
}




