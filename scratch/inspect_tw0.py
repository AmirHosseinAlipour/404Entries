import sys, re
sys.stdout.reconfigure(encoding='utf-8')
with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

m = re.search(r'&7575844806580331650\nMonoBehaviour:.*?m_GameObject: \{fileID: (\d+)\}', text, re.DOTALL)
if m:
    goid = m.group(1)
    txt_m = re.search(r'm_GameObject: \{fileID: ' + goid + r'\}.*?m_Text: "(.*?)"', text, re.DOTALL)
    if txt_m:
        raw = txt_m.group(1).encode('utf-8').decode('unicode_escape')
        print('Raw text in typewriters[0]:', raw)

