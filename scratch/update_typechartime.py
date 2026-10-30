import os, re

all_files = []
for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.prefab') or f.endswith('.unity'):
            all_files.append(os.path.join(root, f))

pattern = re.compile(r'(\btypeCharTime:\s*)0\.05\b')

modified_count = 0
total_replacements = 0

for path in all_files:
    try:
        with open(path, 'r', encoding='utf-8', newline='') as fp:
            content = fp.read()
        
        matches = pattern.findall(content)
        if matches:
            new_content = pattern.sub(r'\g<1>0.035', content)
            with open(path, 'w', encoding='utf-8', newline='') as fp:
                fp.write(new_content)
            modified_count += 1
            total_replacements += len(matches)
    except Exception as e:
        print(f"Error on {path}: {e}")

print(f"Updated {modified_count} files with {total_replacements} replacements of typeCharTime 0.05 -> 0.035")

