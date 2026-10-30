with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
m = re.findall(r'--- !u!114 &\d+[\s\S]*?m_GameObject: \{fileID: 5133584315611796092\}[\s\S]*?(?=--- !u!|\Z)', text)
for block in m:
    print(block[:600])

