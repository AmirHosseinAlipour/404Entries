import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

m = re.search(r'&44127815247946769\nGameObject:.*?(?=\n--- |\Z)', text, re.DOTALL)
if m:
    print(m.group(0))
    # find components
    for cid in re.findall(r'- component: \{fileID: (\d+)\}', m.group(0)):
        comp_m = re.search(r'&' + cid + r'\n([A-Za-z0-9_]+):.*?(?=\n--- |\Z)', text, re.DOTALL)
        if comp_m:
            print(f"Component &{cid} ({comp_m.group(1)}):\n", comp_m.group(0)[:500])
