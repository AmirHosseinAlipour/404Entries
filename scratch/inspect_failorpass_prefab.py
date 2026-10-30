with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
gos = re.findall(r'--- !u!1 &(\d+)\nGameObject:\n[\s\S]*?m_Name: (.*?)\n', text)
with open('scratch/failorpass_objects.txt', 'w', encoding='utf-8') as out:
    for gid, name in gos:
        out.write(f'{gid}: {name}\n')

print(f'Done, {len(gos)} objects in FailOrPass.prefab')

