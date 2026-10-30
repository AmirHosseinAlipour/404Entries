import sys, re, os
sys.stdout.reconfigure(encoding='utf-8')

files = [
    'Assets/Prefabs/Amoozesh.prefab',
    'Assets/Prefabs/DialogueSample.prefab',
    'Assets/Prefabs/DialoguesAfterTaskCompletion.prefab',
    'Assets/Prefabs/DialoguesAfterTaskNotCompletion.prefab',
    'Assets/Scenes/Prototype.unity'
]

def update_file(path):
    if not os.path.exists(path): return
    with open(path, 'r', encoding='utf-8') as f:
        text = f.read()

    blocks = text.split('--- !u!')
    header = blocks[0]
    body_blocks = blocks[1:]

    # Map go_id -> name
    go_names = {}
    for b in body_blocks:
        if b.startswith('1 &'):
            goid = b.split()[1][1:]
            m = re.search(r'm_Name:\s*([^\n]+)', b)
            if m: go_names[goid] = m.group(1)

    # Map rect_id -> block_index
    rect_indices = {}
    for i, b in enumerate(body_blocks):
        if b.startswith('224 &'):
            rid = b.split()[1][1:]
            rect_indices[rid] = i

    changed = False

    # Find Persons and their Image children
    for b in body_blocks:
        if b.startswith('224 &'):
            goid_m = re.search(r'm_GameObject: \{fileID: (\d+)\}', b)
            if not goid_m: continue
            gname = go_names.get(goid_m.group(1), '')
            if 'Person' in gname:
                is_right = 'Person2' in gname or 'Person 2' in gname
                ch_m = re.search(r'm_Children:\n((?:  - \{fileID: \d+\}\n)*)', b)
                if not ch_m: continue
                children = re.findall(r'- \{fileID: (\d+)\}', ch_m.group(1))
                for cid in children:
                    cidx = rect_indices.get(cid)
                    if cidx is None: continue
                    cb = body_blocks[cidx]
                    c_goid_m = re.search(r'm_GameObject: \{fileID: (\d+)\}', cb)
                    if not c_goid_m: continue
                    c_name = go_names.get(c_goid_m.group(1), '')
                    if 'Image' in c_name:
                        # Found the portrait child!
                        # Update Pivot to (0.5, 0)
                        new_cb = cb
                        new_cb = re.sub(r'm_Pivot: \{x: [^,]+, y: [^}]+\}', 'm_Pivot: {x: 0.5, y: 0}', new_cb)
                        
                        if is_right:
                            new_cb = re.sub(r'm_AnchorMin: \{x: [^,]+, y: [^}]+\}', 'm_AnchorMin: {x: 1, y: 0}', new_cb)
                            new_cb = re.sub(r'm_AnchorMax: \{x: [^,]+, y: [^}]+\}', 'm_AnchorMax: {x: 1, y: 0}', new_cb)
                            new_cb = re.sub(r'm_AnchoredPosition: \{x: [^,]+, y: [^}]+\}', 'm_AnchoredPosition: {x: -195, y: 0}', new_cb)
                            new_cb = re.sub(r'm_LocalScale: \{x: [^,]+, y: [^,]+, z: [^}]+\}', 'm_LocalScale: {x: -0.36, y: 0.36, z: 0.36}', new_cb)
                        else:
                            new_cb = re.sub(r'm_AnchorMin: \{x: [^,]+, y: [^}]+\}', 'm_AnchorMin: {x: 0, y: 0}', new_cb)
                            new_cb = re.sub(r'm_AnchorMax: \{x: [^,]+, y: [^}]+\}', 'm_AnchorMax: {x: 0, y: 0}', new_cb)
                            new_cb = re.sub(r'm_AnchoredPosition: \{x: [^,]+, y: [^}]+\}', 'm_AnchoredPosition: {x: 195, y: 0}', new_cb)
                            new_cb = re.sub(r'm_LocalScale: \{x: [^,]+, y: [^,]+, z: [^}]+\}', 'm_LocalScale: {x: 0.36, y: 0.36, z: 0.36}', new_cb)

                        if new_cb != cb:
                            body_blocks[cidx] = new_cb
                            changed = True
                            print(f"[{path}] Updated {gname} -> {c_name} (rect:{cid}) is_right={is_right}")

    if changed:
        new_text = header + '--- !u!' + '--- !u!'.join(body_blocks)
        with open(path, 'w', encoding='utf-8') as f:
            f.write(new_text)
        print(f"SAVED: {path}")

for p in files:
    update_file(p)

