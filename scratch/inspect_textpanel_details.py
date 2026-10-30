import re, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    content = f.read()

for gid in ["5337092378967793134", "4866812489042411019", "1422007302607295020"]:
    go_block_m = re.search(r'&' + gid + r'\nGameObject:.*?(?=\n--- |\Z)', content, re.DOTALL)
    print(f"\n=================== GameObject &{gid} ===================")
    if go_block_m:
        print(go_block_m.group(0))
        comp_ids = re.findall(r'- component:\s*\{fileID:\s*(\d+)\}', go_block_m.group(0))
        for cid in comp_ids:
            cm = re.search(r'&' + cid + r'\n([A-Za-z0-9_]+):.*?(?=\n--- |\Z)', content, re.DOTALL)
            if cm:
                print(f"--- Component &{cid} ({cm.group(1)}) ---")
                print(cm.group(0)[:600])

