import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Amoozesh.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's find all Person windows under Dialogues in Amoozesh
windows = re.findall(r'm_Name: (Person\d+.*?)\n.*?m_Component:\n(.*?)(?=\n--- |\Z)', text, re.DOTALL)
print(f"Found {len(windows)} Person objects:")
for name, comps in windows:
    # get switchwindow
    sw_m = re.search(r'm_Script: \{fileID: 11500000, guid: 8567844f83d02d143bd1f5596eb0c763.*?\n(.*?)(?=\n  m_Script|\n--- |\Z)', comps, re.DOTALL)
    sw_info = ""
    if sw_m:
        cur_idx = re.search(r'currentDialogueIndex: (\d+)', sw_m.group(0))
        dlg_show = re.search(r'dialogueWindowToShow: \{fileID: (\d+)', sw_m.group(0))
        cur_val = cur_idx.group(1) if cur_idx else "none"
        dlg_val = dlg_show.group(1) if dlg_show else "none"
        sw_info = f"currentDialogueIndex={cur_val}, show={dlg_val}"
    print(f"  {name}: {sw_info}")

