import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

for cid in ['1539610346', '6718007316000186916']:
    m = re.search(r'&' + cid + r'\nRectTransform:.*?m_GameObject: \{fileID: (\d+)\}', text, re.DOTALL)
    if m:
        goid = m.group(1)
        go_m = re.search(r'&' + goid + r'\nGameObject:.*?(?=\n--- |\Z)', text, re.DOTALL)
        print(cid, go_m.group(0)[:300])
        for cid2 in re.findall(r'- component: \{fileID: (\d+)\}', go_m.group(0)):
            cm = re.search(r'&' + cid2 + r'\n([A-Za-z0-9_]+):.*?(?=\n--- |\Z)', text, re.DOTALL)
            if cm:
                rc = re.search(r'm_RaycastTarget:\s*(\d+)', cm.group(0))
                print(f"   Comp &{cid2} ({cm.group(1)}): RaycastTarget = {rc.group(1) if rc else 'none'}")

