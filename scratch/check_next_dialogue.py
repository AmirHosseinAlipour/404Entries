with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
matches = re.findall(r'm_MethodName: NextDialogue', text)
print('NextDialogue calls found:', len(matches))

# also check enterdialouge
m2 = re.findall(r'enterdialouge', text)
print('enterdialouge occurrences:', len(m2))

