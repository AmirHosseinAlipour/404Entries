import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

for cid in ['121578145', '235237887', '325984326', '1200005460', '1723997541', '1952494537689546652', '2090419049153986720', '5624292976996222509', '6861336759278169257']:
    # search where - component: {fileID: cid}
    m = re.search(r'GameObject:.*?m_Component:.*?- component: \{fileID: ' + cid + r'\}', text, re.DOTALL)
    if m:
        name = re.search(r'm_Name:\s*([^\n]+)', m.group(0))
        print(cid, name.group(1) if name else "?")
    else:
        print(cid, "no direct match")
