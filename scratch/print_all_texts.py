with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
matches = re.findall(r'm_Text: "(.*?)"', text)
for i, m in enumerate(matches):
    print(f'{i}: {m[:60]}')

