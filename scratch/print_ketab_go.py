with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
m = re.search(r'm_Name: KetabkhonehDialoge[\s\S]*?(?=--- !u!114|\Z)', text)
if m:
    print(m.group(0))

