import re, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    content = f.read()

# let's find the block of trigger 8975605106976783884
m = re.search(r'&8975605106976783884\nMonoBehaviour:.*?(?=\n--- |\Z)', content, re.DOTALL)
if m:
    print("Trigger block:\n", m.group(0))

    # find all dialogueWindows fileIDs
    fids = re.findall(r'- \{fileID: (\d+)\}', m.group(0))
    print(f"\nFound {len(fids)} dialogue windows fileIDs:")
    for fid in fids:
        # find RectTransform
        rt_m = re.search(r'&' + fid + r'\nRectTransform:.*?(?=\n--- |\Z)', content, re.DOTALL)
        if rt_m:
            goid_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', rt_m.group(0))
            goid = goid_m.group(1) if goid_m else ""
            goname = "?"
            if goid:
                gm = re.search(r'&' + goid + r'\nGameObject:.*?m_Name:\s*([^\n]+)', content)
                if gm: goname = gm.group(1)
            pos_m = re.search(r'm_AnchoredPosition:\s*\{([^}]+)\}', rt_m.group(0))
            pos = pos_m.group(1) if pos_m else ""
            sd_m = re.search(r'm_SizeDelta:\s*\{([^}]+)\}', rt_m.group(0))
            sd = sd_m.group(1) if sd_m else ""
            anc_min = re.search(r'm_AnchorMin:\s*\{([^}]+)\}', rt_m.group(0))
            anc_max = re.search(r'm_AnchorMax:\s*\{([^}]+)\}', rt_m.group(0))
            print(f"  Window &{fid} [{goname}]: pos=({pos}), size=({sd}), anchorMin=({anc_min.group(1) if anc_min else ''}), anchorMax=({anc_max.group(1) if anc_max else ''})")

