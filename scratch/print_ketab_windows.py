with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

import re
windows = ['7152224146529876162', '1305662651', '680104516', '360428062', '733028889', '171415695', '1211073191', '317350108', '190267687', '509414258']
for i, wid in enumerate(windows):
    m = re.search(r'--- !u!\d+ &' + wid + r'[\s\S]*?(?=--- !u!|\Z)', text)
    if m:
        goid = re.search(r'm_GameObject: \{fileID: (\d+)\}', m.group(0))
        if goid:
            # find text components under this object
            gm = re.search(r'--- !u!1 &' + goid.group(1) + r'[\s\S]*?(?=--- !u!1 &|\Z)', text)
            print(f'Window {i} (GO {goid.group(1)}):')
            # search for text in this window or children
            # let's search rawText or m_Text
            # we also have typewriter id from list
            tw_ids = ['7575844806580331650', '879986993', '741365693', '1792697614', '283785228', '687232566', '928705637', '1389296496', '148513611', '226714203']
            tw_m = re.search(r'--- !u!\d+ &' + tw_ids[i] + r'[\s\S]*?(?=--- !u!|\Z)', text)
            if tw_m:
                txt = re.search(r'm_Text: "(.*?)"', tw_m.group(0))
                if not txt:
                    # check Text component on same GO
                    tgoid = re.search(r'm_GameObject: \{fileID: (\d+)\}', tw_m.group(0))
                    if tgoid:
                        tcm = re.search(r'--- !u!114 &\d+[\s\S]*?m_GameObject: \{fileID: ' + tgoid.group(1) + r'\}[\s\S]*?m_Text: "(.*?)"', text)
                        if tcm:
                            txt = tcm
                if txt:
                    print('  ', txt.group(1).encode('utf-8').decode('unicode_escape', errors='ignore'))

