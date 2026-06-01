from pathlib import Path
p = Path('Assets/Screpts/Npc/Comerciante.cs')
text = p.read_text(encoding='utf-8')
old = '''                slotUI.Configurar(item);

                var boton = slotGO.GetComponent<Button>();
                if (boton == null)
                {
                    Debug.LogError("[Comerciante] El prefab de slot no tiene un componente Button.");
                    continue;
                }

                boton.onClick.AddListener(() => ComprarItem(item));'''
new = '''                slotUI.Configurar(item);

                var boton = slotGO.GetComponent<Button>() ?? slotGO.GetComponentInChildren<Button>();
                if (boton == null)
                {
                    Debug.LogError("[Comerciante] El prefab de slot no tiene un componente Button (ni en raíz ni en hijos).");
                    continue;
                }

                boton.onClick.AddListener(() => ComprarItem(item));'''
if old not in text:
    raise SystemExit('pattern not found')
text = text.replace(old, new)
p.write_text(text, encoding='utf-8')
print('patched Comerciante.cs')
