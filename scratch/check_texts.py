import re, sys, os
sys.stdout.reconfigure(encoding='utf-8')

all_files = []
for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.prefab') or f.endswith('.unity'):
            all_files.append(os.path.join(root, f))

total_texts = 0
file_counts = {}
for path in all_files:
    try:
        with open(path, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
        matches = re.findall(r'--- !u!114 &(\d+)\nMonoBehaviour:.*?guid: 5f7201a12d95ffc409449d95f23cf332.*?m_FontSize: (\d+)', content, re.DOTALL)
        if matches:
            file_counts[path] = len(matches)
            total_texts += len(matches)
    except Exception as e:
        pass

print(f"Total Text components across all prefabs & scenes: {total_texts}")
print("\nTop files with Text components:")
for p, c in sorted(file_counts.items(), key=lambda x: -x[1])[:30]:
    print(f"  {c:3d} in {p}")
