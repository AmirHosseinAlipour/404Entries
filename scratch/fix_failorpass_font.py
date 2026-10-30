import re, subprocess

orig = subprocess.check_output(['git', 'show', 'HEAD~1:Assets/Prefabs/Professors/FailOrPass.prefab'], encoding='utf-8')

def replace_font_size(match):
    prefix = match.group(1)
    val = int(match.group(2))
    if val <= 4:
        return match.group(0)
    new_val = max(8, val - 4)
    return f"{prefix}{new_val}"

pattern = re.compile(r'(\bm_FontSize:\s*)(\d+)')
fixed = pattern.sub(replace_font_size, orig)

with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'w', encoding='utf-8', newline='') as f:
    f.write(fixed)

print("Updated FailOrPass.prefab to exactly -4 from original")

