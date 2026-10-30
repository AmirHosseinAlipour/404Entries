with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
child_ids = ['2460281288454522524', '512719312376600648', '6432203115279447090', '293570473437770288']
for cid in child_ids:
    m = re.search(r'--- !u!\d+ &' + cid + r'[\s\S]*?(?=--- !u!|\Z)', text)
    if m:
        goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', m.group(0))
        if goid:
            gm = re.search(r'--- !u!1 &' + goid.group(1) + r'[\s\S]*?(?=--- !u!1 &|\Z)', text)
            if gm:
                name = re.search(r'm_Name: (.*?)\n', gm.group(0)).group(1)
                texts = re.findall(r'm_Text: "(.*?)"', gm.group(0))
                print(f'GO {goid.group(1)} ({name}):')
                for t in texts:
                    print('  Text:', t.encode('utf-8').decode('unicode_escape', errors='ignore'))

