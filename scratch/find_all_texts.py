import sys, re
sys.stdout.reconfigure(encoding='utf-8')
with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

texts = re.findall(r'--- !u!114 &(\d+)\nMonoBehaviour:.*?guid: 5f7201a12d95ffc409449d95f23cf332.*?m_GameObject: \{fileID: (\d+)\}.*?m_Text: "(.*?)"', text, re.DOTALL)
print(f"Found {len(texts)} Text components:")
for fid, goid, mtext in texts:
    go_m = re.search(r'&' + goid + r'\nGameObject:.*?m_Name: ([^\n]+)', text)
    goname = go_m.group(1) if go_m else "unknown"
    try:
        dec = mtext.encode('utf-8').decode('unicode_escape')
    except:
        dec = mtext
    dec = dec.replace('\n', ' ')
    print(f"  {goname} (&{goid}): {dec}")
