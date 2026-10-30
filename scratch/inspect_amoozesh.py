with open('Assets/Prefabs/Amoozesh.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
game_objects = re.findall(r'--- !u!1 &(\d+)\nGameObject:\n[\s\S]*?m_Name: (.*?)\n', text)
print(f'Total GameObjects in Amoozesh.prefab: {len(game_objects)}')
for gid, name in game_objects:
    if any(k in name.lower() for k in ['dialogue', 'switch', 'canvas', 'button', 'next']):
        print(f'{gid}: {name}')

# also find SwitchWindow components
sw = re.findall(r'm_Script: \{fileID: 11500000, guid: [0-9a-fA-F]+, type: 3\}[\s\S]*?(?=--- !u!|\Z)', text)
print(f'Scripts count: {len(sw)}')

