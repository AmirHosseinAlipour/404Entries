import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Amoozesh.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

# map &fileID to blocks
blocks = {}
for m in re.finditer(r'--- !u!(\d+) &(\d+)\n(.*?)(?=\n--- !u!|\Z)', text, re.DOTALL):
    blocks[m.group(2)] = (m.group(1), m.group(3))

for fid, (typ, body) in blocks.items():
    if typ == '1': # GameObject
        name_m = re.search(r'm_Name: (Person\d+.*)', body)
        if name_m:
            name = name_m.group(1)
            # find all components of this GO
            comp_ids = re.findall(r'- component: \{fileID: (\d+)\}', body)
            sw_info = "No SW"
            for cid in comp_ids:
                if cid in blocks and blocks[cid][0] == '114':
                    cbody = blocks[cid][1]
                    if 'guid: 8567844f83d02d143bd1f5596eb0c763' in cbody: # SwitchWindow
                        cur = re.search(r'currentDialogueIndex: (\d+)', cbody)
                        show = re.search(r'dialogueWindowToShow: \{fileID: (\d+)', cbody)
                        hide = re.search(r'windowToHide: \{fileID: (\d+)', cbody)
                        sw_info = f"currentDialogueIndex={cur.group(1) if cur else '?'}, show={show.group(1) if show else '?'}, hide={hide.group(1) if hide else '?'}"
            print(f"{name} (&{fid}): {sw_info}")

