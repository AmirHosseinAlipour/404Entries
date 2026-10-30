with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
print('=== KetabkhonehDialoge ===')
m = re.search(r'm_Name: KetabkhonehDialoge[\s\S]*?(?=--- !u!1 &|\Z)', text)
if m:
    pos = re.search(r'm_LocalPosition: \{x: (.*?), y: (.*?), z: (.*?)\}', m.group(0))
    col = re.search(r'BoxCollider2D:[\s\S]*?m_Size: \{x: (.*?), y: (.*?)\}[\s\S]*?m_Offset: \{x: (.*?), y: (.*?)\}', m.group(0))
    if pos: print('Position:', pos.groups())
    if col: print('Collider Size:', col.groups()[:2], 'Offset:', col.groups()[2:])

print('=== FailOrPass in scene ===')
# search for FailOrPass prefab instance or GameObject
m2 = re.findall(r'--- !u!1001 &\d+[\s\S]*?m_SourcePrefab: \{fileID: 100100000, guid: 28497963c24c35245991bbdb6e4577ea[\s\S]*?(?=--- !u!1001|\Z)', text)
if m2:
    print('Found FailOrPass PrefabInstance:')
    pos2 = re.findall(r'propertyPath: m_LocalPosition\.(x|y|z)\n\s+value: (.*?)\n', m2[0])
    print('Position mods:', pos2)

