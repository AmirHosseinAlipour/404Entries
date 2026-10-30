with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
for gid in ['4428939773682499539', '3932395790572670334']:
    gm = re.search(r'--- !u!1 &' + gid + r'[\s\S]*?(?=--- !u!1 &|\Z)', text)
    print('=== GO', gid)
    texts = re.findall(r'm_Text: "(.*?)"', gm.group(0))
    for t in texts:
        print('  Text:', t.encode('utf-8').decode('unicode_escape', errors='ignore'))

