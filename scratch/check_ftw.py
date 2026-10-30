import re, sys, os
sys.stdout.reconfigure(encoding='utf-8')

for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.prefab') or f.endswith('.unity'):
            path = os.path.join(root, f)
            with open(path, 'r', encoding='utf-8', errors='ignore') as fp:
                c = fp.read()
            if 'FarsiTypewriter' in c or 'DynamicFontResizer' in c:
                # check count
                ftw_count = len(re.findall(r'FarsiTypewriter', c))
                print(f"{path}: FTW mentions={ftw_count}")

