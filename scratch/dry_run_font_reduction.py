import re, sys, os
sys.stdout.reconfigure(encoding='utf-8')

all_files = []
for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.prefab') or f.endswith('.unity'):
            all_files.append(os.path.join(root, f))

def replace_font_size(match):
    prefix = match.group(1)
    val = int(match.group(2))
    if val <= 4:
        return match.group(0) # don't touch special/tiny values like -99 or 3
    new_val = max(8, val - 4)
    return f"{prefix}{new_val}"

patterns = [
    (re.compile(r'(\bm_FontSize:\s*)(\d+)'), "m_FontSize"),
    (re.compile(r'(\bm_fontSize:\s*)(\d+)'), "m_fontSize"),
    (re.compile(r'(\bm_fontSizeBase:\s*)(\d+)'), "m_fontSizeBase"),
]

total_changes = 0
files_modified = {}

for path in all_files:
    try:
        with open(path, 'r', encoding='utf-8', errors='ignore') as fp:
            content = fp.read()
        
        file_changes = 0
        new_content = content
        for pat, name in patterns:
            # count changes
            matches = pat.findall(new_content)
            for m in matches:
                v = int(m[1])
                if v > 4:
                    file_changes += 1
            new_content = pat.sub(replace_font_size, new_content)
        
        if file_changes > 0:
            files_modified[path] = file_changes
            total_changes += file_changes
    except Exception as e:
        print(f"Error on {path}: {e}")

print(f"DRY RUN: Total potential changes = {total_changes} across {len(files_modified)} files.")
for p, c in sorted(files_modified.items(), key=lambda x: -x[1])[:35]:
    print(f"  {c:3d} changes in {p}")

