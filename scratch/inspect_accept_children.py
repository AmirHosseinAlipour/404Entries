with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
cids = ['4664160066624133453', '7859973230998496688', '766546904563827297']
for cid in cids:
    m = re.search(r'--- !u!\d+ &' + cid + r'[\s\S]*?(?=--- !u!|\Z)', text)
    if m:
        goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', m.group(0))
        if goid:
            gm = re.search(r'--- !u!1 &' + goid.group(1) + r'[\s\S]*?m_Name: (.*?)\n', text)
            print(cid, '->', goid.group(1), gm.group(1) if gm else 'unknown')

