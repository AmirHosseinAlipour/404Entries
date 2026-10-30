import sys, re
sys.stdout.reconfigure(encoding='utf-8')
with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's find Texts under FailOrPass
matches = re.findall(r'm_Name: (Letter\w+|lastLetter\w+|Text[^\n]*).*?m_Text: "(.*?)"', text, re.DOTALL)
for name, mtext in matches:
    clean = mtext.replace('\\n', '\n')
    try:
        dec = clean.encode('utf-8').decode('unicode_escape')
    except:
        dec = clean
    print(f"=== {name} ===\n{dec}\n")

