import glob

targets = ['ای بابا', 'حواس', 'یکی دیگه', 'مدرک منو', 'مش رجب', 'نفر']
for f in glob.glob('Assets/**/*.cs', recursive=True) + glob.glob('Assets/**/*.prefab', recursive=True) + glob.glob('Assets/**/*.unity', recursive=True):
    try:
        with open(f, 'r', encoding='utf-8') as file:
            c = file.read()
            for t in targets:
                if t in c:
                    print(f'Found "{t}" in: {f}')
                    break
    except:
        pass

