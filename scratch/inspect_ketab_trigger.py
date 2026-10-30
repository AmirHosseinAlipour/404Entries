with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
m = re.search(r'--- !u!114 &8975605106976783884[\s\S]*?(?=--- !u!|\Z)', text)
if m:
    print(m.group(0))

