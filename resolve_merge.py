from pathlib import Path
p = Path(r'Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset')
text = p.read_text(errors='replace')

# Remove the entire conflict block and replace with empty lists
conflict_start = text.find('<<<<<<< Updated upstream')
conflict_end = text.find('>>>>>>> Stashed changes')

if conflict_start != -1 and conflict_end != -1:
    before = text[:conflict_start]
    after = text[conflict_end + len('>>>>>>> Stashed changes'):]
    
    # Replace the whole conflict with the simple version (empty lists)
    new_text = before + '  m_GlyphTable: []\n  m_CharacterTable: []' + after
    p.write_text(new_text, encoding='utf-8')
    print('Conflict resolved! Replaced with empty tables.')
else:
    print('No conflict markers found or incomplete conflict block.')
