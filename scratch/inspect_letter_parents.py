import sys, re
sys.stdout.reconfigure(encoding='utf-8')
with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

for name, fid in [('lastLetterLow', '2199517774929126187'), ('lastLetterMedium', '3433514603796353730'), ('lastLetterHigh', '7988938196034133150')]:
    m = re.search(r'&' + fid + r'\nMonoBehaviour:.*?m_GameObject: \{fileID: (\d+)\}', text, re.DOTALL)
    if m:
        goid = m.group(1)
        go_m = re.search(r'&' + goid + r'\nGameObject:.*?(?=\n--- |\Z)', text, re.DOTALL)
        goname = re.search(r'm_Name:\s*(.+)', go_m.group(0)).group(1)
        print(f"{name}: GO '{goname}' (&{goid})")
