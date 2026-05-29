using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rbJugador;

    [SerializeField] private float velocidad = 5f;

    [Header("Dash")]
    [SerializeField] private float fuerzaDash = 20f;
    [SerializeField] private float duracionDash = 0.2f;
    [SerializeField] private float cooldownDash = 1f;

    private bool haciendoDash = false;
    private bool puedeDash = true;

    private Vector2 Movinput;
    private Vector2 ultimaDireccion;

    void Update()
    {
        if (haciendoDash) return;

        Movinput.x = Input.GetAxisRaw("Horizontal");
        Movinput.y = Input.GetAxisRaw("Vertical");

        Movinput = Movinput.normalized;

        // Guarda la última dirección en la que se movió
        if (Movinput != Vector2.zero)
        {
            ultimaDireccion = Movinput;
        }

        // Dash con la tecla E
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (puedeDash)
            {
                StartCoroutine(Dash());
            }
        }
    }

    private void FixedUpdate()
    {
        if (!haciendoDash)
        {
            rbJugador.linearVelocity = Movinput * velocidad;
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
}