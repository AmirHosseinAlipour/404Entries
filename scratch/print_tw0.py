with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
m = re.search(r'--- !u!114 &7575844806580331650[\s\S]*?(?=--- !u!|\Z)', text)
if m:
    print(m.group(0))
    goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', m.group(0))
    if goid:
        tgm = re.search(r'--- !u!114 &\d+[\s\S]*?m_GameObject: \{fileID: ' + goid.group(1) + r'\}[\s\S]*?m_Text: "(.*?)"', text)
        if tgm:
            print('Text:', tgm.group(1).encode('utf-8').decode('unicode_escape', errors='ignore'))

