import re, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    content = f.read()

for gid in ["1305662650", "248563182", "1301684028", "879986991"]:
    m = re.search(r'&' + gid + r'\nGameObject:.*?(?=\n--- |\Z)', content, re.DOTALL)
    if m:
        name = re.search(r'm_Name:\s*([^\n]+)', m.group(0))
        print(f"GO &{gid}: {name.group(1) if name else '?'}")

