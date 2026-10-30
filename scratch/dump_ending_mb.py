with open('Assets/Scenes/Ending.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
mb = re.findall(r'--- !u!114 &(\d+)[\s\S]*?(?=--- !u!|\Z)', text)
with open('scratch/ending_monobehaviours.txt', 'w', encoding='utf-8') as out:
    for block in mb:
        out.write(block + '\n==================\n')

print(f'Done, {len(mb)} MonoBehaviours written.')

