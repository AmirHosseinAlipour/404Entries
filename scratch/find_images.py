with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
images = re.findall(r'--- !u!114 &(\d+)[\s\S]*?m_Script: \{fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc[\s\S]*?m_Sprite: \{fileID: .*?, guid: ([0-9a-fA-F]+)', text)
print('Total Image components in FailOrPass.prefab:', len(images))
for fid, guid in images:
    # find gameObject
    m = re.search(r'--- !u!114 &' + fid + r'[\s\S]*?m_GameObject: \{fileID: (\d+)\}', text)
    if m:
        goid = m.group(1)
        gm = re.search(r'--- !u!1 &' + goid + r'[\s\S]*?m_Name: (.*?)\n', text)
        print(f'GO {goid} ({gm.group(1) if gm else "?"}): sprite guid {guid}')

