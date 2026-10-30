with open('Assets/Scenes/Ending.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
objects = re.findall(r'--- !u!1 &(\d+)\nGameObject:\n[\s\S]*?m_Name: (.*?)\n', text)
with open('scratch/ending_objects.txt', 'w', encoding='utf-8') as out:
    for oid, name in objects:
        out.write(f'{oid}: {name}\n')

print(f'Done, {len(objects)} objects written.')

