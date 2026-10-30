import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# find RectTransform 7152224146529876162
m = re.search(r'&7152224146529876162\nRectTransform:.*?(?=\n--- |\Z)', text, re.DOTALL)
if m:
    goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', m.group(0)).group(1)
    go_m = re.search(r'&' + goid + r'\nGameObject:.*?(?=\n--- |\Z)', text, re.DOTALL)
    print(go_m.group(0))
    for cid in re.findall(r'- component: \{fileID: (\d+)\}', go_m.group(0)):
        cm = re.search(r'&' + cid + r'\n([A-Za-z0-9_]+):.*?(?=\n--- |\Z)', text, re.DOTALL)
        if cm:
            print(f"--- Comp &{cid} ({cm.group(1)}) ---")
            print(cm.group(0)[:600])

