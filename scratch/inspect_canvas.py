import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

m = re.search(r'&6772902182182001398\nGameObject:.*?(?=\n--- |\Z)', text, re.DOTALL)
if m:
    print(m.group(0))
    for cid in re.findall(r'- component: \{fileID: (\d+)\}', m.group(0)):
        cm = re.search(r'&' + cid + r'\n([A-Za-z0-9_]+):.*?(?=\n--- |\Z)', text, re.DOTALL)
        if cm:
            print(f"--- Comp &{cid} ({cm.group(1)}) ---")
            print(cm.group(0)[:400])

