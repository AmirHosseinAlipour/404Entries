with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
m = re.search(r'--- !u!1 &1305662650[\s\S]*?(?=--- !u!1 &|\Z)', text)
if m:
    print(m.group(0)[:1500])

