import re, sys, os
sys.stdout.reconfigure(encoding='utf-8')

# Let's inspect FailOrPass.prefab texts
with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    c = f.read()

# find all MonoBehaviour 5f7201a12d95ffc409449d95f23cf332
blocks = c.split('--- !u!114 &')
for b in blocks[1:]:
    if 'guid: 5f7201a12d95ffc409449d95f23cf332' in b:
        fid = b.split('\n')[0].strip()
        fs_m = re.search(r'm_FontSize:\s*(\d+)', b)
        fs = fs_m.group(1) if fs_m else "?"
        txt_m = re.search(r'm_Text:\s*"(.*?)"', b, re.DOTALL)
        txt = txt_m.group(1) if txt_m else ""
        txt = txt[:30].replace('\n', ' ')
        goid_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
        goid = goid_m.group(1) if goid_m else ""
        go_name = ""
        if goid:
            gm = re.search(r'&' + goid + r'\nGameObject:.*?m_Name:\s*([^\n]+)', c)
            if gm: go_name = gm.group(1)
        print(f"FailOrPass Text: [{go_name}] size={fs} text={txt}")

