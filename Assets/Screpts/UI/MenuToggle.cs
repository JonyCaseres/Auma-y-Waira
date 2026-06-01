using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggle : MonoBehaviour
{
    [Header("Panel principal del menú")]
    public GameObject menuPanel;

    private PlayerInput playerInput;

    private void Awake()
    {
        // Como el script está en el Player, tomamos el PlayerInput del mismo objeto
        playerInput = GetComponent<PlayerInput>();

        // Suscribir la acción "Menu" del mapa Player
        playerInput.actions["Menu"].performed += ctx => AbrirMenu();
        playerInput.actions["Menu"].canceled += ctx => CerrarMenu();
    }

    private void AbrirMenu()
    {
        menuPanel.SetActive(true);
        playerInput.SwitchCurrentActionMap("UI"); // cambiar al mapa UI
    }

    private void CerrarMenu()
    {
        menuPanel.SetActive(false);
        playerInput.SwitchCurrentActionMap("Player"); // volver al mapa Player
    }
}


