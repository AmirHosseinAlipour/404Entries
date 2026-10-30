with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
matches = re.findall(r'm_Text: "(.*?)"', text)
for m in matches:
    try:
        decoded = m.encode('utf-8').decode('unicode_escape')
        if any(w in decoded for w in ['۶۵', '65', 'آموزش', 'مدرک', 'تصحیح', 'بایگانی']):
            print('Text:', decoded)
    except:
        pass

