import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Amoozesh.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

blocks = text.split('--- !u!')

go_names = {}
for b in blocks[1:]:
    if b.startswith('1 &'):
        goid = b.split()[1][1:]
        m = re.search(r'm_Name:\s*([^\n]+)', b)
        if m: go_names[goid] = m.group(1)

for b in blocks[1:]:
    if b.startswith('224 &'):
        rid = b.split()[1][1:]
        goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', b)
        if goid:
            name = go_names.get(goid.group(1), '')
            if 'Text' in name:
                fid = re.search(r'm_Father: \{fileID: (\d+)\}', b)
                pos = re.search(r'm_AnchoredPosition: \{x: ([^,]+), y: ([^}]+)\}', b)
                sd = re.search(r'm_SizeDelta: \{x: ([^,]+), y: ([^}]+)\}', b)
                amin = re.search(r'm_AnchorMin: \{x: ([^,]+), y: ([^}]+)\}', b)
                amax = re.search(r'm_AnchorMax: \{x: ([^,]+), y: ([^}]+)\}', b)
                piv = re.search(r'm_Pivot: \{x: ([^,]+), y: ([^}]+)\}', b)
                father_id = fid.group(1) if fid else "0"
                print(f"{name} (rect:{rid}, father:{father_id}): pos=({pos.group(1)},{pos.group(2)}) sd=({sd.group(1)},{sd.group(2)}) amin=({amin.group(1)},{amin.group(2)}) amax=({amax.group(1)},{amax.group(2)}) piv=({piv.group(1)},{piv.group(2)})")

