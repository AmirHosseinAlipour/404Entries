import sys, re
sys.stdout.reconfigure(encoding='utf-8')

with open('Assets/Prefabs/Amoozesh.prefab', 'r', encoding='utf-8') as f:
    text = f.read()

# Let's search for SwitchWindow components in Amoozesh prefab
sw_matches = re.findall(r'm_Script: \{fileID: 11500000, guid: 8567844f83d02d143bd1f5596eb0c763, type: 3\}.*?(?=\n--- |\Z)', text, re.DOTALL)
print(f"Found {len(sw_matches)} SwitchWindow components in Amoozesh.prefab")
for i, sw in enumerate(sw_matches):
    print(f"--- SW {i} ---")
    for line in sw.splitlines():
        if any(k in line for k in ['currentDialogueIndex', 'shouldInvoke', 'dialogueWindowToShow', 'windowToHide', 'windowToShow']):
            print(" ", line)

