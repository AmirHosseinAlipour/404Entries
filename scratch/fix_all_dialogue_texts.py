import sys, os, re
sys.stdout.reconfigure(encoding='utf-8')

target_files = [
    'Assets/Scenes/Prototype.unity',
    'Assets/Scenes/Prototype Mahan.unity',
    'Assets/Scenes/Prototype Mohammad.unity',
    'Assets/Scenes/Prototype 1.unity',
    'Assets/Scenes/Prototype 32.unity',
    'Assets/Prefabs/Amoozesh.prefab',
    'Assets/Prefabs/DialogueSample.prefab',
    'Assets/Prefabs/DialoguesAfterTaskCompletion.prefab',
    'Assets/Prefabs/DialoguesAfterTaskNotCompletion.prefab',
]

def process_file(path):
    if not os.path.exists(path):
        return

    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    blocks = content.split('--- !u!')
    header = blocks[0]
    body = blocks[1:]

    # Map go_id -> name
    go_names = {}
    for b in body:
        if b.startswith('1 &'):
            goid = b.split()[1][1:]
            m = re.search(r'm_Name:\s*([^\n]+)', b)
            if m: go_names[goid] = m.group(1)

    # Map rect_id -> (go_id, father_rect_id, block_index)
    rect_map = {}
    for idx, b in enumerate(body):
        if b.startswith('224 &'):
            rid = b.split()[1][1:]
            goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', b)
            fid = re.search(r'm_Father: \{fileID: (\d+)\}', b)
            rect_map[rid] = {
                'goid': goid.group(1) if goid else '',
                'fid': fid.group(1) if fid else '',
                'idx': idx
            }

    modified_count = 0

    # Find dialogue text components
    for rid, rdata in rect_map.items():
        gname = go_names.get(rdata['goid'], '')
        # Is it Text (Legacy) inside a dialogue Text container?
        father_rid = rdata['fid']
        father_goid = rect_map.get(father_rid, {}).get('goid', '')
        father_gname = go_names.get(father_goid, '')

        is_dialogue_text = (
            ('Text' in gname) and 
            ('Text' in father_gname or 'Person' in father_gname or 'Dialogue' in father_gname)
        )

        if is_dialogue_text:
            idx = rdata['idx']
            rect_block = body[idx]

            # 1. Update RectTransform to centered padding: sizeDelta=(-120, -80), pos=(0, 0), pivot=(0.5, 0.5)
            new_rect = rect_block
            new_rect = re.sub(r'm_AnchoredPosition: \{x: [^,]+, y: [^}]+\}', 'm_AnchoredPosition: {x: 0, y: 0}', new_rect)
            new_rect = re.sub(r'm_SizeDelta: \{x: [^,]+, y: [^}]+\}', 'm_SizeDelta: {x: -120, y: -80}', new_rect)
            new_rect = re.sub(r'm_Pivot: \{x: [^,]+, y: [^}]+\}', 'm_Pivot: {x: 0.5, y: 0.5}', new_rect)
            new_rect = re.sub(r'm_AnchorMin: \{x: [^,]+, y: [^}]+\}', 'm_AnchorMin: {x: 0, y: 0}', new_rect)
            new_rect = re.sub(r'm_AnchorMax: \{x: [^,]+, y: [^}]+\}', 'm_AnchorMax: {x: 1, y: 1}', new_rect)

            if new_rect != rect_block:
                body[idx] = new_rect
                modified_count += 1
                print(f"[{path}] Updated RectTransform for {gname} (rect:{rid})")

            # 2. Update Text component on the same GameObject
            goid = rdata['goid']
            for b_i, b in enumerate(body):
                if f'm_GameObject: {{fileID: {goid}}}' in b:
                    if 'guid: 5f7201a12d95ffc409449d95f23cf332' in b: # Text
                        new_tb = b
                        # Set font size to 46
                        new_tb = re.sub(r'm_FontSize:\s*(?:68|56|54|52|50|60|64)', 'm_FontSize: 46', new_tb)
                        # Set line spacing to 1.15
                        new_tb = re.sub(r'm_LineSpacing:\s*1(?!\.)', 'm_LineSpacing: 1.15', new_tb)
                        
                        # Clean up awkward artificial newlines in dialogue strings: "\n " -> " " or "\n" -> " "
                        def clean_dialogue_text(m_txt):
                            raw = m_txt.group(1)
                            # Only clean if it has Persian characters \u06
                            if '\\u06' in raw:
                                # replace \n followed by space or just \n
                                cleaned = re.sub(r'\\n\s*', ' ', raw)
                                cleaned = re.sub(r'\n\s*', ' ', cleaned)
                                # trim trailing quotes
                                return f'm_Text: {cleaned}'
                            return m_txt.group(0)

                        new_tb = re.sub(r'm_Text:\s*([^\n]+(?:\n\s+[^\n]+)*)', clean_dialogue_text, new_tb)

                        if new_tb != b:
                            body[b_i] = new_tb
                            modified_count += 1
                            print(f"[{path}] Updated Text component for {gname} (goid:{goid})")

                    if 'guid: bf93a1da200e05a46a3db21ae978e645' in b: # FarsiTypewriter
                        new_fb = b
                        new_fb = re.sub(r'middleAlign:\s*0', 'middleAlign: 1', new_fb)
                        if new_fb != b:
                            body[b_i] = new_fb
                            modified_count += 1
                            print(f"[{path}] Enabled middleAlign on FarsiTypewriter for {gname}")

    if modified_count > 0:
        new_content = header + '--- !u!' + '--- !u!'.join(body)
        with open(path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"SAVED: {path} with {modified_count} modifications.")

for p in target_files:
    process_file(p)

