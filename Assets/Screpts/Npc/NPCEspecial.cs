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
            if (Input.GetKeyDown(KeyCode.Z)) SeleccionarOpcion(questionData.respuesta1);
            if (Input.GetKeyDown(KeyCode.X)) SeleccionarOpcion(questionData.respuesta2);
            if (Input.GetKeyDown(KeyCode.C)) SeleccionarOpcion(questionData.respuesta3);
        }
    }

    private void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        dialoguePanel.SetActive(true);
        typingCoroutine = StartCoroutine(TypeLine());
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
            // Si se salta la escritura, detener la coroutine y el sonido inmediatamente
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

    private void SeleccionarOpcion(string[] respuesta)
    {
        mostrandoPreguntas = false;
        yaRespondio = true; // marcar que ya respondió

        // Guardar que habló con la abuela (NPC especial)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetTalkedToGrandma(true);
        }

        // Ejemplo: si la opción 1 es "Aceptar misión", detectamos y guardamos
        // Ajusta la comprobación según cómo tengas definidos los datos en questionData
        if (questionData != null && questionData.respuesta1 == respuesta)
        {
            // guardar que aceptó la misión
            if (SaveManager.Instance != null)
                SaveManager.Instance.SetMissionAccepted(true);
        }

        panelRespuesta1.SetActive(false);
        panelRespuesta2.SetActive(false);
        panelRespuesta3.SetActive(false);

        dialoguePanel.SetActive(true);
        respuestaCoroutine = StartCoroutine(TypeRespuesta(dialogueText, respuesta));
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

        // Prioriza el clip arrastrado en este componente; si no existe, usa el del ScriptableObject
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




