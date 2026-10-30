import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Amoozesh.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

idx_start = text.find('dialoguePhases:')
idx_end = text.find('listOfTexts:', idx_start)
block = text[idx_start:idx_end]

# split by '- dialogues:'
phases = block.split('- dialogues:')
for p_idx, phase in enumerate(phases[1:]):
    print(f"\n==================== PHASE {p_idx} ====================")
    # find lines starting with - " or - 
    # Items in YAML list:
    items = re.split(r'\n\s*-\s*', '\n' + phase)
    for item in items[1:]:
        # unescape
        clean = item.strip()
        # remove surrounding quotes if present
        if clean.startswith('"') and clean.endswith('"'):
            clean = clean[1:-1]
        # replace escaped newlines
        clean = clean.replace('\\n', '\n').replace('\\r', '')
        # decode unicode escapes
        try:
            decoded = clean.encode('utf-8').decode('unicode_escape')
        except:
            decoded = clean
        print(f"--- DIALOGUE LINE ---:\n{decoded}\n")

