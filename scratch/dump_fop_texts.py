with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

import re
matches = re.findall(r'--- !u!114 &(\d+)[\s\S]*?m_GameObject: \{fileID: (\d+)\}[\s\S]*?m_Text: "(.*?)"', text)
with open('scratch/failorpass_all_texts.txt', 'w', encoding='utf-8') as out:
    for fid, gid, t in matches:
        # find name of gid
        gm = re.search(r'--- !u!1 &' + gid + r'[\s\S]*?m_Name: (.*?)\n', text)
        gname = gm.group(1) if gm else 'unknown'
        decoded = t.encode('utf-8').decode('unicode_escape', errors='ignore')
        out.write(f'GO {gid} ({gname}) [MB {fid}]:\n{decoded}\n--------------------\n')
print('Done!')

