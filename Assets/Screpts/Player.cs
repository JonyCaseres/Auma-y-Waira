using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

public class Player : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    [SerializeField] private Rigidbody2D rbJugador;

    [SerializeField] private float velocidad = 5f;

    [Header("Interacción")]
    [SerializeField] private float radioInteraccion = 1f;
    [SerializeField] private LayerMask capasInteraccion = ~0;

    [Header("Dash")]
    [SerializeField] private float fuerzaDash = 20f;
    [SerializeField] private float duracionDash = 0.2f;
    [SerializeField] private float cooldownDash = 1f;

    [Header("Stamina")]
    [SerializeField] private barradeestamina staminaBar; // referencia al slider de estamina
    [SerializeField] private float dashStaminaCost = 25f; // coste de stamina por dash

    private InputSystem_Actions inputActions;
    private bool haciendoDash = false;
    private bool puedeDash = true;

    private Vector2 moveInput;
    private Vector2 ultimaDireccion;

    private void Awake()
    {
        // Inicializar el sistema de input
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
        }
    }

    private void Start()
    {
        // Si no asignaste la barra en el Inspector, intenta encontrarla en la escena
        if (staminaBar == null)
        {
            staminaBar = FindObjectOfType<barradeestamina>();
        }
    }

    private void OnEnable()
    {
        // Habilitar las acciones y registrar los callbacks
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
        }

        if (inputActions.Player == null)
        {
            Debug.LogError("[Player] inputActions.Player es null en OnEnable.");
            return;
        }

        inputActions.Player.SetCallbacks(this);
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        // Deshabilitar las acciones y remover los callbacks
        if (inputActions == null)
        {
            Debug.LogWarning("[Player] inputActions es null en OnDisable.");
            return;
        }

        if (inputActions.Player != null)
        {
            inputActions.Player.Disable();
            inputActions.Player.RemoveCallbacks(this);
        }
        else
        {
            Debug.LogWarning("[Player] inputActions.Player es null en OnDisable.");
        }
    }

    private void FixedUpdate()
    {
        if (!haciendoDash)
        {
            rbJugador.linearVelocity = moveInput * velocidad;
        }
    }

    /// <summary>
    /// Callback para la acción Move del Input System
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        // Guarda la última dirección en la que se movió
        if (moveInput != Vector2.zero)
        {
            ultimaDireccion = moveInput;
        }
    }

    /// <summary>
    /// Callback para la acción Sprint del Input System (Dash)
    /// </summary>
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started && !haciendoDash && puedeDash)
        {
            // Comprueba y gasta stamina antes de dash. Si no hay barra asignada, permite dash.
            if (staminaBar == null || staminaBar.TryUse(dashStaminaCost))
            {
                StartCoroutine(Dash());
            }
            else
            {
                Debug.Log("No hay suficiente estamina para dash.");
            }
        }
    }

    private IEnumerator Dash()
    {
        puedeDash = false;
        haciendoDash = true;

        rbJugador.linearVelocity = ultimaDireccion.normalized * fuerzaDash;

        yield return new WaitForSeconds(duracionDash);

        haciendoDash = false;

        yield return new WaitForSeconds(cooldownDash);

        puedeDash = true;
    }

    // Implementar métodos no utilizados de la interfaz
    public void OnLook(InputAction.CallbackContext context) { }
    public void OnAttack(InputAction.CallbackContext context) { }
    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log($"OnInteract callback phase: {context.phase}");
        if (context.started)
        {
            InteractNearby();
        }
    }
    public void OnCrouch(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }
    public void OnPrevious(InputAction.CallbackContext context) { }
    public void OnNext(InputAction.CallbackContext context) { }

    // Métodos compatibles con PlayerInput/SendMessages si Unity invoca mediante nombre.
    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveInput = input;
        if (moveInput != Vector2.zero)
        {
            ultimaDireccion = moveInput;
        }
    }

    public void OnSprint(InputValue value)
    {
        if (value.isPressed && !haciendoDash && puedeDash)
        {
            if (staminaBar == null || staminaBar.TryUse(dashStaminaCost))
            {
                StartCoroutine(Dash());
            }
            else
            {
                Debug.Log("No hay suficiente estamina para dash.");
            }
        }
    }

    public void OnInteract(InputValue value)
    {
        Debug.Log($"OnInteract InputValue pressed: {value.isPressed}");
        if (value.isPressed)
        {
            InteractNearby();
        }
    }

    public void OnAttack(InputValue value) { }
    public void OnLook(InputValue value) { }
    public void OnCrouch(InputValue value) { }
    public void OnJump(InputValue value) { }
    public void OnPrevious(InputValue value) { }
    public void OnNext(InputValue value) { }

    private void InteractNearby()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radioInteraccion, capasInteraccion);
        Debug.Log($"InteractNearby: found {colliders.Length} colliders within radius {radioInteraccion}");

        foreach (Collider2D collider in colliders)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = collider.GetComponentInParent<IInteractable>();
            }

            if (interactable != null && interactable.CanInteract())
            {
                Debug.Log($"Interacting with {collider.name} ({interactable.GetType().Name})");
                interactable.Interact(gameObject);
                return;
            }
        }
        Debug.Log("InteractNearby: no interactable found or none were ready.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioInteraccion);
    }
}