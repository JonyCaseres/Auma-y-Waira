using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    [SerializeField] private Rigidbody2D rbJugador;

    [SerializeField] private float velocidad = 5f;

    [Header("Dash")]
    [SerializeField] private float fuerzaDash = 20f;
    [SerializeField] private float duracionDash = 0.2f;
    [SerializeField] private float cooldownDash = 1f;

    private InputSystem_Actions inputActions;
    private bool haciendoDash = false;
    private bool puedeDash = true;

    private Vector2 moveInput;
    private Vector2 ultimaDireccion;

    private void Awake()
    {
        // Inicializar el sistema de input
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        // Habilitar las acciones y registrar los callbacks
        inputActions.Player.SetCallbacks(this);
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        // Deshabilitar las acciones y remover los callbacks
        inputActions.Player.Disable();
        inputActions.Player.RemoveCallbacks(this);
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
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        puedeDash = false;
        haciendoDash = true;

        rbJugador.linearVelocity = ultimaDireccion * fuerzaDash;

        yield return new WaitForSeconds(duracionDash);

        haciendoDash = false;

        yield return new WaitForSeconds(cooldownDash);

        puedeDash = true;
    }

    // Implementar métodos no utilizados de la interfaz
    public void OnLook(InputAction.CallbackContext context) { }
    public void OnAttack(InputAction.CallbackContext context) { }
    public void OnInteract(InputAction.CallbackContext context) { }
    public void OnCrouch(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }
    public void OnPrevious(InputAction.CallbackContext context) { }
    public void OnNext(InputAction.CallbackContext context) { }
}