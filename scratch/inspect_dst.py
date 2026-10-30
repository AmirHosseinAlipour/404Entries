import re, sys
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    content = f.read()

# find DialogueSequenceTrigger components
# guid of DialogueSequenceTrigger:
# let's find it
m = re.findall(r'--- !u!114 &(\d+)\nMonoBehaviour:.*?DialogueSequenceTrigger', content, re.DOTALL)
print(f"Found {len(m)} DialogueSequenceTriggers by text")

# or search by script guid
guid_m = re.findall(r'--- !u!114 &(\d+)\nMonoBehaviour:.*?m_Script: \{fileID: 11500000, guid: ([a-f0-9]+), type: 3\}', content)
# find guid for DialogueSequenceTrigger
ds_guid = None
with open('Assets/DialogueSequenceTrigger.cs.meta', 'r', encoding='utf-8') as f:
    meta = f.read()
    g = re.search(r'guid:\s*([a-f0-9]+)', meta)
    if g: ds_guid = g.group(1)

print("DialogueSequenceTrigger GUID:", ds_guid)

blocks = content.split('--- !u!114 &')
for b in blocks[1:]:
    if ds_guid and ds_guid in b:
        fid = b.split('\n')[0].strip()
        goid_m = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', b)
        goid = goid_m.group(1) if goid_m else "?"
        # find GameObject name
        goname = "?"
        gm = re.search(r'&' + goid + r'\nGameObject:.*?m_Name:\s*([^\n]+)', content)
        if gm: goname = gm.group(1)
        
        # find dialogueWindows
        dw = re.findall(r'dialogueWindows:.*?(?=\n\s*[a-zA-Z_]|\Z)', b, re.DOTALL)
        dw_str = dw[0][:200].replace('\n', ' ') if dw else "none"
        print(f"Trigger &{fid} on [{goname}]: {dw_str}")

