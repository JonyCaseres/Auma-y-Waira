using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance;

    [Header("UI Diálogo")]
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TMP_Text textoDialogo;

    [Header("UI Elecciones")]
    [SerializeField] private GameObject panelElecciones;
    [SerializeField] private List<GameObject> botonesElecciones; // asignados en inspector

    private List<string> dialogoActual;
    private int indexDialogo = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- Mostrar diálogo lineal ---
    public void IniciarDialogo(List<string> lineas)
    {
        dialogoActual = lineas;
        indexDialogo = 0;
        panelDialogo.SetActive(true);
        panelElecciones.SetActive(false);
        MostrarLinea();
    }

    void Update()
    {
        if (panelDialogo.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return)) // avanzar
            {
                indexDialogo++;
                if (indexDialogo < dialogoActual.Count)
                    MostrarLinea();
                else
                    TerminarDialogo();
            }
            else if (Input.GetKeyDown(KeyCode.Z)) // retroceder
            {
                indexDialogo = Mathf.Max(0, indexDialogo - 1);
                MostrarLinea();
            }
        }
    }

    private void MostrarLinea()
    {
        textoDialogo.text = dialogoActual[indexDialogo];
    }

    private void TerminarDialogo()
    {
        panelDialogo.SetActive(false);
        dialogoActual = null;
    }

    // --- Mostrar elecciones ---
    public void MostrarElecciones(string npcId, List<string> opciones, System.Action<string> callback)
    {
        panelDialogo.SetActive(false);
        panelElecciones.SetActive(true);

        // desactivar todos los botones primero
        foreach (var boton in botonesElecciones)
            boton.SetActive(false);

        // activar según cantidad de opciones
        for (int i = 0; i < opciones.Count && i < botonesElecciones.Count; i++)
        {
            botonesElecciones[i].SetActive(true);
            TMP_Text texto = botonesElecciones[i].GetComponentInChildren<TMP_Text>();
            texto.text = opciones[i];

            int index = i;
            botonesElecciones[i].GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
            botonesElecciones[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                panelElecciones.SetActive(false);
                callback(opciones[index]); // devolvemos la decisión al GameManager
            });
        }
    }
}
