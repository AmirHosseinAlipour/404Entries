import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

blocks = text.split('--- !u!')

go_names = {}
for b in blocks[1:]:
    if b.startswith('1 &'):
        goid = b.split()[1][1:]
        m = re.search(r'm_Name:\s*([^\n]+)', b)
        if m: go_names[goid] = m.group(1)

rects = {}
for b in blocks[1:]:
    if b.startswith('224 &'):
        rid = b.split()[1][1:]
        goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', b)
        fid = re.search(r'm_Father: \{fileID: (\d+)\}', b)
        pos = re.search(r'm_AnchoredPosition: \{x: ([^,]+), y: ([^}]+)\}', b)
        sd = re.search(r'm_SizeDelta: \{x: ([^,]+), y: ([^}]+)\}', b)
        amin = re.search(r'm_AnchorMin: \{x: ([^,]+), y: ([^}]+)\}', b)
        amax = re.search(r'm_AnchorMax: \{x: ([^,]+), y: ([^}]+)\}', b)
        rects[goid.group(1) if goid else ''] = {
            'rid': rid,
            'fid': fid.group(1) if fid else '',
            'pos': (pos.group(1), pos.group(2)) if pos else None,
            'sd': (sd.group(1), sd.group(2)) if sd else None,
            'amin': (amin.group(1), amin.group(2)) if amin else None,
            'amax': (amax.group(1), amax.group(2)) if amax else None,
        }

for b in blocks[1:]:
    if b.startswith('114 &') and 'guid: bf93a1da200e05a46a3db21ae978e645' in b:
        goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', b)
        if goid:
            gid = goid.group(1)
            name = go_names.get(gid, '')
            r = rects.get(gid, {})
            print(f"GO {name} (id:{gid}): pos={r.get('pos')} sd={r.get('sd')} amin={r.get('amin')} amax={r.get('amax')}")
