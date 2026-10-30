import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

m = re.search(r'&8975605106976783884\nMonoBehaviour:.*?(?=\n--- |\Z)', text, re.DOTALL)
if m:
    print(m.group(0))

