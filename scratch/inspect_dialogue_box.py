import re, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Professors/FailOrPass.prefab', 'r', encoding='utf-8') as f:
    content = f.read()

# find dialogueBox and texts references on PaperManager
# PaperManager has dialogueBox: {fileID: 4527065149467444880}
# texts:
#   - {fileID: 4932152861957903540}
#   - {fileID: 3815545125475017044}
#   - {fileID: 748022341850966181}

def get_go_info(file_id):
    # could be component or GameObject
    m = re.search(r'&' + file_id + r'\n([A-Za-z0-9_]+):.*?(?=\n--- |\Z)', content, re.DOTALL)
    if not m: return f"ID {file_id} not found"
    comp_type = m.group(1)
    comp_block = m.group(0)
    goid_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', comp_block)
    goid = goid_m.group(1) if goid_m else file_id
    go_block_m = re.search(r'&' + goid + r'\nGameObject:.*?(?=\n--- |\Z)', content, re.DOTALL)
    goname = "unknown"
    if go_block_m:
        name_m = re.search(r'm_Name:\s*([^\n]+)', go_block_m.group(0))
        if name_m: goname = name_m.group(1)
    return f"Type: {comp_type}, GO: {goname} (&{goid})"

print("dialogueBox (4527065149467444880):", get_go_info("4527065149467444880"))
print("texts[0] (4932152861957903540):", get_go_info("4932152861957903540"))
print("texts[1] (3815545125475017044):", get_go_info("3815545125475017044"))
print("texts[2] (748022341850966181):", get_go_info("748022341850966181"))

