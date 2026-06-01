using UnityEngine;
using TMPro;
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

    [Header("Paneles de opciones")]
    public GameObject panelRespuesta1;
    public TMP_Text textoRespuesta1;
    public GameObject panelRespuesta2;
    public TMP_Text textoRespuesta2;
    public GameObject panelRespuesta3;
    public TMP_Text textoRespuesta3;

    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool mostrandoPreguntas;
    private bool yaRespondio;

    private string ultimoDialogo; // almacena el último diálogo mostrado

    private Coroutine typingCoroutine;
    private Coroutine respuestaCoroutine;

    private void Start()
    {
        ValidarReferencias();
    }

    private void ValidarReferencias()
    {
        if (dialogueData == null)
            Debug.LogError("[NPCEspecial] dialogueData no está asignado en el Inspector.");

        if (questionData == null)
            Debug.LogError("[NPCEspecial] questionData no está asignado en el Inspector.");

        if (dialoguePanel == null)
            Debug.LogError("[NPCEspecial] dialoguePanel no está asignado.");

        if (dialogueText == null)
            Debug.LogError("[NPCEspecial] dialogueText no está asignado.");
    }

    // --- Implementación de IInteractable ---
    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact(GameObject jugador)
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

    private void Update()
    {
        if (isDialogueActive && !mostrandoPreguntas && Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }

        if (mostrandoPreguntas && questionData != null)
        {
            if (Input.GetKeyDown(KeyCode.Z)) SeleccionarOpcion(questionData.respuesta1);
            if (Input.GetKeyDown(KeyCode.X)) SeleccionarOpcion(questionData.respuesta2);
            if (Input.GetKeyDown(KeyCode.C)) SeleccionarOpcion(questionData.respuesta3);
        }
    }

    private void StartDialogue()
    {
        if (dialogueData == null || dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            Debug.LogError("[NPCEspecial] No hay diálogos configurados.");
            return;
        }

        isDialogueActive = true;
        dialogueIndex = 0;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        if (dialogueData.dialogueLines[dialogueIndex] == null)
        {
            isTyping = false;
            yield break;
        }

        isTyping = true;
        if (dialogueText != null) dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex].ToCharArray())
        {
            if (dialogueText != null) dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

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
            if (dialogueText != null) dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (dialogueIndex + 1 < dialogueData.dialogueLines.Length)
        {
            dialogueIndex++;
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
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
        if (questionData == null)
        {
            Debug.LogError("[NPCEspecial] questionData es null, no se pueden mostrar preguntas.");
            EndDialogue();
            return;
        }

        mostrandoPreguntas = true;

        if (panelRespuesta1 != null) panelRespuesta1.SetActive(true);
        if (panelRespuesta2 != null) panelRespuesta2.SetActive(true);
        if (panelRespuesta3 != null) panelRespuesta3.SetActive(true);

        if (textoRespuesta1 != null) StartCoroutine(TypeOption(textoRespuesta1, questionData.opcion1));
        if (textoRespuesta2 != null) StartCoroutine(TypeOption(textoRespuesta2, questionData.opcion2));
        if (textoRespuesta3 != null) StartCoroutine(TypeOption(textoRespuesta3, questionData.opcion3));
    }

    private IEnumerator TypeOption(TMP_Text texto, string opcion)
    {
        if (string.IsNullOrEmpty(opcion))
            yield break;

        if (texto != null) texto.SetText("");
        foreach (char letter in opcion.ToCharArray())
        {
            if (texto != null) texto.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
    }

    private void SeleccionarOpcion(string[] respuesta)
    {
        if (respuesta == null || respuesta.Length == 0)
        {
            Debug.LogError("[NPCEspecial] Respuesta inválida o vacía.");
            EndDialogue();
            return;
        }

        mostrandoPreguntas = false;
        yaRespondio = true; // marcar que ya respondió

        if (panelRespuesta1 != null) panelRespuesta1.SetActive(false);
        if (panelRespuesta2 != null) panelRespuesta2.SetActive(false);
        if (panelRespuesta3 != null) panelRespuesta3.SetActive(false);

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (respuestaCoroutine != null) StopCoroutine(respuestaCoroutine);
        respuestaCoroutine = StartCoroutine(TypeRespuesta(dialogueText, respuesta));
    }

    private IEnumerator TypeRespuesta(TMP_Text texto, string[] respuesta)
    {
        foreach (string linea in respuesta)
        {
            if (string.IsNullOrEmpty(linea))
                continue;

            if (texto != null) texto.SetText("");
            foreach (char letter in linea.ToCharArray())
            {
                if (texto != null) texto.text += letter;
                yield return new WaitForSeconds(dialogueData.typingSpeed);
            }

            // Guardar último diálogo mostrado
            ultimoDialogo = linea;

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }
        EndDialogue();
    }

    private void MostrarUltimoDialogo()
    {
        isDialogueActive = true;
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        if (string.IsNullOrEmpty(ultimoDialogo))
            ultimoDialogo = "No hemos hablado todavía...";

        if (dialogueText != null) dialogueText.SetText(ultimoDialogo);
    }

    private void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (respuestaCoroutine != null) StopCoroutine(respuestaCoroutine);

        isDialogueActive = false;
        mostrandoPreguntas = false;
        dialogueIndex = 0;

        // Ocultar paneles siempre al terminar
        if (panelRespuesta1 != null) panelRespuesta1.SetActive(false);
        if (panelRespuesta2 != null) panelRespuesta2.SetActive(false);
        if (panelRespuesta3 != null) panelRespuesta3.SetActive(false);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}


