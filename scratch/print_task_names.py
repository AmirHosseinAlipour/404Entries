import sys, re
sys.stdout.reconfigure(encoding='utf-8')
with open('Assets/Prefabs/TaskManager.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

m = re.findall(r'taskNames:\n(.*?)(?=\n  \w|\Z)', text, re.DOTALL)
if m:
    lines = m[0].split('- ')
    for i, line in enumerate(lines[1:]):
        raw = line.strip().strip('"').replace('\n', ' ')
        raw = re.sub(r'\s+', ' ', raw)
        try:
            decoded = raw.encode('utf-8').decode('unicode_escape')
        except Exception as e:
            decoded = raw
        print(f'{i}: {decoded}')

