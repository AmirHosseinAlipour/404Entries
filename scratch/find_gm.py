with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
# find GameManager
m = re.search(r'm_Name: GameManager[\s\S]*?(?=--- !u!)', text)
if m:
    print(m.group(0)[:1500])
else:
    print("GameManager not found in Prototype.unity directly, let's search for taskTriggers")
    m2 = re.findall(r'taskTriggers:[\s\S]*?(?=\n  \w+:)', text)
    if m2:
        print(m2[0][:1000])

