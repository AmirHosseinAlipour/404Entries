import sys, re
sys.stdout.reconfigure(encoding='utf-8')
with open('Assets/Scenes/Prototype.unity', 'r', encoding='utf-8') as f:
    text = f.read()

idx = text.find('m_Name: EventSystem')
print(text[idx-50:idx+1500])

