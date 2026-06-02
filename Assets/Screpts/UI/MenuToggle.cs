using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggle : MonoBehaviour
{
    [Header("Panel principal del menú")]
    public GameObject menuPanel;

    private PlayerInput playerInput;

    private void Awake()
    {

        playerInput = FindObjectOfType<PlayerInput>();
    }

    public void AbrirMenu()
    {
        menuPanel.SetActive(true);
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("UI"); // cambiar al mapa UI
    }

    public void CerrarMenu()
    {
        menuPanel.SetActive(false);
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");
    }
}



