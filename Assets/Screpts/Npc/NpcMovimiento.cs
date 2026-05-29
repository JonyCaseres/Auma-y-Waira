using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NpcMovimiento : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float tiempoEspera = 2f;
    [SerializeField] private bool loop = true;

    [Header("Dialogo")]
    [SerializeField] public bool dialogando = false;

    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints; // ahora es una lista editable en el inspector

    private int indexActual = 0;
    private bool esperando = false;

    private NavMeshAgent agente;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = velocidad;
        agente.updateRotation = false; // útil en 2D
        agente.updateUpAxis = false;

        if (waypoints.Length > 0)
        {
            agente.SetDestination(waypoints[indexActual].position);
        }
    }

    void Update()
    {
        if (esperando || dialogando || waypoints.Length == 0)
            return;

        if (!agente.pathPending && agente.remainingDistance < 0.1f)
        {
            StartCoroutine(Esperar());
        }
    }

    private IEnumerator Esperar()
    {
        esperando = true;
        yield return new WaitForSeconds(tiempoEspera);

        indexActual++;
        if (indexActual >= waypoints.Length)
        {
            if (loop)
                indexActual = 0;
            else
            {
                enabled = false;
                yield break;
            }
        }

        agente.SetDestination(waypoints[indexActual].position);
        esperando = false;
    }
}
