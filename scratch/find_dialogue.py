import os, re, sys
sys.stdout.reconfigure(encoding='utf-8')

for root, dirs, files in os.walk('Assets'):
    for f in files:
        if f.endswith('.cs') or f.endswith('.prefab') or f.endswith('.unity') or f.endswith('.json'):
            p = os.path.join(root, f)
            with open(p, 'r', encoding='utf-8', errors='ignore') as fp:
                c = fp.read()
            if 'ای بابا' in c or 'حواس پرت' in c:
                print(f"FOUND in {p}")
