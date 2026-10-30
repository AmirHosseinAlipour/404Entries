import re, sys, os
sys.stdout.reconfigure(encoding='utf-8')

all_files = []
for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.prefab') or f.endswith('.unity'):
            all_files.append(os.path.join(root, f))

sizes = {}
for path in all_files:
    try:
        with open(path, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
        matches = re.findall(r'm_FontSize:\s*(\d+)', content)
        for m in matches:
            s = int(m)
            sizes[s] = sizes.get(s, 0) + 1
    except:
        pass

print("Standard UI Text m_FontSize distribution:")
for s in sorted(sizes.keys()):
    print(f"  size {s}: {sizes[s]} occurrences")

