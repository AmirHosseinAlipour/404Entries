import sys, re
sys.stdout.reconfigure(encoding='utf-8')
with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

for cid in ['6077795717913784773', '5489127270580098807']:
    m = re.search(r'&' + cid + r'\nRectTransform:.*?m_GameObject: \{fileID: (\d+)\}', text, re.DOTALL)
    if m:
        goid = m.group(1)
        go_m = re.search(r'&' + goid + r'\nGameObject:.*?(?=\n--- |\Z)', text, re.DOTALL)
        print(cid, go_m.group(0))
