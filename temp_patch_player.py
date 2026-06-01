from pathlib import Path
p = Path('Assets/Screpts/Player.cs')
text = p.read_text(encoding='utf-8')
old = '''        foreach (Collider2D collider in colliders)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>()
                ?? collider.GetComponentInParent<IInteractable>()
                ?? collider.GetComponentInChildren<IInteractable>();

            if (interactable != null)
            {
                Debug.Log($"Found interactable on {collider.name} => {interactable.GetType().Name}");
                if (interactable.CanInteract())
                {
                    Debug.Log($"Interacting with {collider.name} ({interactable.GetType().Name})");
                    interactable.Interact(gameObject);
                    return;
                }
                else
                {
                    Debug.Log($"Interactable found but not ready: {collider.name}");
                }
            }
            else
            {
                Debug.Log($"No IInteractable on collider {collider.name}. Checking next.");
            }
        }
'''
new = '''        foreach (Collider2D collider in colliders)
        {
            // Ignorar el propio collider del jugador para no detectar al Player como un interactable.
            if (collider.transform.IsChildOf(transform))
            {
                Debug.Log($"Skipping own player collider {collider.name}");
                continue;
            }

            IInteractable interactable = collider.GetComponent<IInteractable>()
                ?? collider.GetComponentInParent<IInteractable>()
                ?? collider.GetComponentInChildren<IInteractable>();

            if (interactable != null)
            {
                Debug.Log($"Found interactable on {collider.name} => {interactable.GetType().Name}");
                if (interactable.CanInteract())
                {
                    Debug.Log($"Interacting with {collider.name} ({interactable.GetType().Name})");
                    interactable.Interact(gameObject);
                    return;
                }
                else
                {
                    Debug.Log($"Interactable found but not ready: {collider.name}");
                }
            }
            else
            {
                Debug.Log($"No IInteractable on collider {collider.name}. Checking next.");
            }
        }
'''
if old not in text:
    raise SystemExit('pattern not found')
text = text.replace(old, new)
p.write_text(text, encoding='utf-8')
print('patched')
