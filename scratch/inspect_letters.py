import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

for fid, name in [
    ('2795389676922674469', 'letterLow'),
    ('5751582602588980014', 'letterMedium'),
    ('8359449890115724369', 'letterHigh')
]:
    m = re.search(r'&' + fid + r'\s+MonoBehaviour:.*?(?=\n--- |\Z)', text, re.DOTALL)
    if m:
        # find gameobject
        go = re.search(r'm_GameObject: \{fileID: (\d+)\}', m.group(0)).group(1)
        # find Text component on this gameobject
        txt_m = re.search(r'm_GameObject: \{fileID: ' + go + r'\}.*?m_Text: "(.*?)"', text, re.DOTALL)
        if txt_m:
            raw = txt_m.group(1).encode('utf-8').decode('unicode_escape')
            print(f"=== {name} (&{fid}) ===\n{raw}\n")
        else:
            print(f"=== {name} (&{fid}) === (no Text found on GO {go})")

