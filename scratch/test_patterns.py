import re, sys, os
sys.stdout.reconfigure(encoding='utf-8')

all_files = []
for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.prefab') or f.endswith('.unity'):
            all_files.append(os.path.join(root, f))

patterns = [
    r'(\bm_FontSize:\s*)(\d+)',
    r'(\bm_fontSize:\s*)(\d+)',
    r'(\bm_fontSizeBase:\s*)(\d+)',
]

for p in patterns:
    total = 0
    for path in all_files:
        try:
            with open(path, 'r', encoding='utf-8', errors='ignore') as fp:
                c = fp.read()
            matches = re.findall(p, c)
            total += len(matches)
        except:
            pass
    print(f"Pattern {p}: {total} occurrences")

