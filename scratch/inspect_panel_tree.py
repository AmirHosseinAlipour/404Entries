import re, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    content = f.read()

# Let's inspect Background_Panel GameObject 5337092378967793134
def inspect_hierarchy(goid, indent=0):
    go_block_m = re.search(r'&' + goid + r'\nGameObject:.*?(?=\n--- |\Z)', content, re.DOTALL)
    if not go_block_m: return
    name_m = re.search(r'm_Name:\s*([^\n]+)', go_block_m.group(0))
    goname = name_m.group(1) if name_m else "unknown"
    
    # get components on this GameObject
    comp_ids = re.findall(r'- component:\s*\{fileID:\s*(\d+)\}', go_block_m.group(0))
    comp_types = []
    rt_id = None
    for cid in comp_ids:
        cm = re.search(r'&' + cid + r'\n([A-Za-z0-9_]+):', content)
        if cm:
            ctype = cm.group(1)
            comp_types.append(ctype)
            if ctype == 'RectTransform':
                rt_id = cid
            if ctype == 'Image':
                # check image properties
                cblock = re.search(r'&' + cid + r'\nImage:.*?(?=\n--- |\Z)', content, re.DOTALL)
                if cblock:
                    col = re.search(r'm_Color:\s*\{([^}]+)\}', cblock.group(0))
                    col_str = col.group(1) if col else ""
                    sp = re.search(r'm_Sprite:\s*\{fileID:\s*(-?\d+)[^}]*\}', cblock.group(0))
                    sp_str = sp.group(0) if sp else ""
                    comp_types[-1] += f" (Color: {col_str}, Sprite: {sp_str})"

    print("  " * indent + f"- {goname} (&{goid}) [{', '.join(comp_types)}]")
    
    # find children in RectTransform
    if rt_id:
        rt_block_m = re.search(r'&' + rt_id + r'\nRectTransform:.*?(?=\n--- |\Z)', content, re.DOTALL)
        if rt_block_m:
            child_ids = re.findall(r'- \{fileID:\s*(\d+)\}', rt_block_m.group(0))
            for ch_rt_id in child_ids:
                # find GameObject of child
                ch_rt_block = re.search(r'&' + ch_rt_id + r'\nRectTransform:.*?(?=\n--- |\Z)', content, re.DOTALL)
                if ch_rt_block:
                    ch_go_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', ch_rt_block.group(0))
                    if ch_go_m:
                        inspect_hierarchy(ch_go_m.group(1), indent + 1)

inspect_hierarchy("5337092378967793134")

