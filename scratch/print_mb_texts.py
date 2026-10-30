with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
for gid in ['3286973504314529637', '5713036354495635418']:
    m = re.findall(r'--- !u!114 &\d+[\s\S]*?m_GameObject: \{fileID: ' + gid + r'\}[\s\S]*?(?=--- !u!|\Z)', text)
    print(f'=== GO {gid} ===')
    for block in m:
        for line in block.splitlines():
            if any(k in line for k in ['m_Text:', 'rawFarsiText', 'farsiText']):
                print('  ', line)

