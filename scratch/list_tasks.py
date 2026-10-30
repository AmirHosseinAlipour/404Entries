with open('Assets/Prefabs/TaskManager.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
matches = re.findall(r'- "(\\u[0-9a-fA-F]{4}.*?)"', text)
with open('scratch/tasks.txt', 'w', encoding='utf-8') as out:
    for i, m in enumerate(matches):
        decoded = m.encode('utf-8').decode('unicode_escape')
        out.write(f'{i}: {decoded}\n')
print(f'Done, wrote {len(matches)} tasks')

