with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
# inspect Background_Panel (6445101335460687743) and Accept (4424380471649371954)
for gid in ['6445101335460687743', '4424380471649371954']:
    m = re.search(r'--- !u!1 &' + gid + r'[\s\S]*?(?=--- !u!1 &|\Z)', text)
    if m:
        print(f'=== GO {gid} ===')
        print(m.group(0)[:1500])

