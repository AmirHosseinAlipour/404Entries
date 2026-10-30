import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

# find PaperManager component (search for counts: or PaperManager)
m = re.search(r'm_Script: \{fileID: 11500000, guid: [0-9a-f]+, type: 3\}\s+m_Name:\s*\n.*?letterLow:.*?(?=\n--- |\Z)', text, re.DOTALL)
if not m:
    # search by letterLow
    idx = text.find('letterLow:')
    print("Found letterLow at", idx)
    print(text[idx-200:idx+1500])
else:
    print(m.group(0))

