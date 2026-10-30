import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

dz_matches = re.findall(r'--- !u!114 &(\d+)\nMonoBehaviour:.*?guid: e034fac97be31c14b8b0c6d2419798f6.*?(?=\n--- |\Z)', text, re.DOTALL)
print(f"Found {len(dz_matches)} DropZone components:")
for fid in dz_matches:
    m = re.search(r'&' + fid + r'\nMonoBehaviour:.*?(?=\n--- |\Z)', text, re.DOTALL)
    if m:
        body = m.group(0)
        is_pass = re.search(r'isPassSlot:\s*(\d+)', body)
        goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', body).group(1)
        go_m = re.search(r'&' + goid + r'\nGameObject:.*?(?=\n--- |\Z)', text, re.DOTALL)
        goname = re.search(r'm_Name:\s*(.+)', go_m.group(0)).group(1)
        print(f"  DropZone &{fid} on '{goname}': isPassSlot = {is_pass.group(1) if is_pass else '?'}")
