with open('Assets/Scenes/Ending.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
m = re.search(r'm_Text: "(.*?)"', text, re.DOTALL)
if m:
    decoded = m.group(1).encode('utf-8').decode('unicode_escape')
    with open('scratch/ending_text.txt', 'w', encoding='utf-8') as out:
        out.write(decoded)
    print('Written to scratch/ending_text.txt')

