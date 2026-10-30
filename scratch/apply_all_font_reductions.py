import re, sys, os

all_files = []
for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.prefab') or f.endswith('.unity'):
            all_files.append(os.path.join(root, f))

def replace_font_size(match):
    prefix = match.group(1)
    val = int(match.group(2))
    if val <= 4:
        return match.group(0)
    new_val = max(8, val - 4)
    return f"{prefix}{new_val}"

patterns = [
    re.compile(r'(\bm_FontSize:\s*)(\d+)'),
    re.compile(r'(\bm_fontSize:\s*)(\d+)'),
    re.compile(r'(\bm_fontSizeBase:\s*)(\d+)'),
]

modified_files = []
total_changes = 0

for path in all_files:
    try:
        with open(path, 'r', encoding='utf-8', newline='') as fp:
            content = fp.read()
        
        file_changes = 0
        new_content = content
        for pat in patterns:
            matches = pat.findall(new_content)
            for m in matches:
                v = int(m[1])
                if v > 4:
                    file_changes += 1
            new_content = pat.sub(replace_font_size, new_content)
        
        if file_changes > 0 and new_content != content:
            with open(path, 'w', encoding='utf-8', newline='') as fp:
                fp.write(new_content)
            modified_files.append((path, file_changes))
            total_changes += file_changes
    except Exception as e:
        print(f"Error updating {path}: {e}")

print(f"DONE: Successfully updated {len(modified_files)} files with {total_changes} font size reductions.")
for p, c in sorted(modified_files, key=lambda x: -x[1])[:30]:
    print(f"  {c:2d} changes: {p}")

