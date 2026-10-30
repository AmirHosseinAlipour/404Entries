import sys, re, os
sys.stdout.reconfigure(encoding='utf-8')

files = [
    'Assets/Prefabs/Amoozesh.prefab',
    'Assets/Prefabs/DialogueSample.prefab',
    'Assets/Prefabs/DialoguesAfterTaskCompletion.prefab',
    'Assets/Prefabs/DialoguesAfterTaskNotCompletion.prefab',
    'Assets/Scenes/Prototype.unity'
]

for p in files:
    if not os.path.exists(p): continue
    with open(p, 'r', encoding='utf-8') as f:
        text = f.read()

    blocks = text.split('--- !u!')
    go_names = {}
    for b in blocks:
        if b.startswith('1 &'):
            goid = b.split()[1][1:]
            m = re.search(r'm_Name:\s*([^\n]+)', b)
            if m: go_names[goid] = m.group(1)

    rects = {}
    for b in blocks:
        if b.startswith('224 &'):
            rid = b.split()[1][1:]
            goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', b)
            fid = re.search(r'm_Father: \{fileID: (\d+)\}', b)
            pos = re.search(r'm_AnchoredPosition: \{x: ([^,]+), y: ([^}]+)\}', b)
            piv = re.search(r'm_Pivot: \{x: ([^,]+), y: ([^}]+)\}', b)
            sc = re.search(r'm_LocalScale: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}', b)
            ch_match = re.search(r'm_Children:\n((?:  - \{fileID: \d+\}\n)*)', b)
            ch = re.findall(r'- \{fileID: (\d+)\}', ch_match.group(1)) if ch_match else []
            rects[rid] = {
                'goid': goid.group(1) if goid else '',
                'fid': fid.group(1) if fid else '',
                'children': ch,
                'pos': (pos.group(1), pos.group(2)) if pos else None,
                'piv': (piv.group(1), piv.group(2)) if piv else None,
                'sc': (sc.group(1), sc.group(2)) if sc else None
            }

    print(f'=== {p} ===')
    for rid, rdata in rects.items():
        gname = go_names.get(rdata['goid'], '')
        if 'Person' in gname:
            print(f"{gname} (rect:{rid}):")
            for cid in rdata['children']:
                cdata = rects.get(cid)
                if cdata:
                    cname = go_names.get(cdata['goid'], '')
                    print(f"   child {cname} (rect:{cid}): pos={cdata['pos']} piv={cdata['piv']} sc={cdata['sc']}")
