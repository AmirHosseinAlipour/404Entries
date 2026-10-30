import sys
sys.stdout.reconfigure(encoding='utf-8')
with open('Assets/Prefabs/Amoozesh.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

lines = text.splitlines()
phase = -1
in_dialogues = False
for line in lines:
    if 'dialoguePhases:' in line:
        in_dialogues = True
    elif in_dialogues:
        if 'dialogues:' in line:
            phase += 1
            if phase >= 6:
                print(f'=== Phase {phase} ===')
        elif line.strip().startswith('- ') and phase >= 6:
            raw = line.strip()[2:].strip('"')
            try:
                decoded = raw.encode('utf-8').decode('unicode_escape')
                print(f'  {decoded}')
            except Exception as e:
                print(f'  {raw}')
        elif 'listOfTexts:' in line:
            break

