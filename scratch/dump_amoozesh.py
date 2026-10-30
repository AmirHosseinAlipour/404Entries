with open('Assets/Prefabs/Amoozesh.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
lines = text.splitlines()
phase = -1
in_dialogues = False
with open('scratch/amoozesh_dialogues.txt', 'w', encoding='utf-8') as out:
    for line in lines:
        if 'dialoguePhases:' in line:
            in_dialogues = True
        elif in_dialogues:
            if 'dialogues:' in line:
                phase += 1
                out.write(f'--- Phase {phase} ---\n')
            elif line.strip().startswith('- '):
                raw = line.strip()[2:]
                try:
                    # decode unicode escape if present
                    if '\\u' in raw:
                        # strip quotes
                        raw = raw.strip('"')
                        decoded = raw.encode('utf-8').decode('unicode_escape')
                        out.write(f'  {decoded}\n')
                    else:
                        out.write(f'  {raw}\n')
                except Exception as e:
                    out.write(f'  {raw}\n')
            elif 'listOfTexts:' in line:
                break
print(f'Done, dumped phases up to {phase}')

