import sys, re
sys.stdout.reconfigure(encoding='utf-8')
with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

for cid in ['6077795717913784773', '5489127270580098807']:
    m = re.search(r'&' + cid + r'\nRectTransform:.*?m_GameObject: \{fileID: (\d+)\}', text, re.DOTALL)
    if m:
        goid = m.group(1)
        txt = re.search(r'm_GameObject: \{fileID: ' + goid + r'\}.*?m_Text: "(.*?)"', text, re.DOTALL)
        if txt:
            print(cid, txt.group(1).encode('utf-8').decode('unicode_escape'))
