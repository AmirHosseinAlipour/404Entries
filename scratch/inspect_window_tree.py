import re, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    content = f.read()

def inspect_tree(rt_id, indent=0):
    rt_block = re.search(r'&' + rt_id + r'\nRectTransform:.*?(?=\n--- |\Z)', content, re.DOTALL)
    if not rt_block: return
    goid_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', rt_block.group(0))
    goid = goid_m.group(1) if goid_m else ""
    goname = "?"
    if goid:
        gm = re.search(r'&' + goid + r'\nGameObject:.*?m_Name:\s*([^\n]+)', content)
        if gm: goname = gm.group(1)
    
    pos_m = re.search(r'm_AnchoredPosition:\s*\{([^}]+)\}', rt_block.group(0))
    pos = pos_m.group(1) if pos_m else ""
    sd_m = re.search(r'm_SizeDelta:\s*\{([^}]+)\}', rt_block.group(0))
    sd = sd_m.group(1) if sd_m else ""
    anc_min = re.search(r'm_AnchorMin:\s*\{([^}]+)\}', rt_block.group(0))
    anc_max = re.search(r'm_AnchorMax:\s*\{([^}]+)\}', rt_block.group(0))
    piv = re.search(r'm_Pivot:\s*\{([^}]+)\}', rt_block.group(0))
    
    print("  " * indent + f"- [{goname}] (&{goid}, rt=&{rt_id}): pos=({pos}), size=({sd}), anchorMin=({anc_min.group(1) if anc_min else ''}), anchorMax=({anc_max.group(1) if anc_max else ''}), pivot=({piv.group(1) if piv else ''})")
    
    # children
    child_ids = re.findall(r'- \{fileID:\s*(\d+)\}', rt_block.group(0))
    for cid in child_ids:
        inspect_tree(cid, indent + 1)

print("=== Hierarchy of Window 1305662651 (the one in the screenshot) ===")
inspect_tree("1305662651")

