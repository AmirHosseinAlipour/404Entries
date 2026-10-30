import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's find the parent of Person 1 (Father is 7389257964854964267)
# And parent of 7389257964854964267
curr = '7389257964854964267'
while curr != '0' and curr:
    m = re.search(r'&' + curr + r'\n([A-Za-z0-9_]+):.*?(?=\n--- |\Z)', text, re.DOTALL)
    if not m:
        print(f"ID {curr} not found")
        break
    typ = m.group(1)
    goid_m = re.search(r'm_GameObject: \{fileID: (\d+)\}', m.group(0))
    goid = goid_m.group(1) if goid_m else "?"
    go_m = re.search(r'&' + goid + r'\nGameObject:.*?m_Name: ([^\n]+)', text)
    goname = go_m.group(1) if go_m else "?"
    print(f"ID {curr} ({typ}) on '{goname}' (&{goid})")
    father_m = re.search(r'm_Father: \{fileID: (\d+)\}', m.group(0))
    curr = father_m.group(1) if father_m else '0'

