import re, sys

path = 'Assets/Prefabs/Professors/FailOrPass.prefab'
with open(path, 'r', encoding='utf-8', newline='') as fp:
    content = fp.read()

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

new_content = content
for pat in patterns:
    new_content = pat.sub(replace_font_size, new_content)

with open(path, 'w', encoding='utf-8', newline='') as fp:
    fp.write(new_content)

print("Updated FailOrPass.prefab successfully")

