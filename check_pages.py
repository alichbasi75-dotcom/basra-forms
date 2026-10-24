import fitz, os, json

with open(r'C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system\data\forms.json', encoding='utf-8') as f:
    forms = json.load(f)

for form in forms:
    file_type = form.get('fileType')
    file_name = form.get('fileName')
    upload_path = os.path.join(r'C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system\uploads', file_name)
    
    if file_name.lower().endswith('.pdf') and os.path.exists(upload_path):
        doc = fitz.open(upload_path)
        print(f"{form['id']} ({form['title']}): {len(doc)} pages")
    elif file_name.lower().endswith(('.jpg', '.jpeg', '.png')):
        print(f"{form['id']} ({form['title']}): 1 image page")
    else:
        print(f"{form['id']} [OFFICE {file_type}]: {file_name}")
