import os
import shutil
import json
import pymupdf

uploads_dir = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system\uploads"
sheets_dir = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system\public\sheets"
os.makedirs(sheets_dir, exist_ok=True)

forms_path = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system\data\forms.json"
with open(forms_path, 'r', encoding='utf-8') as f:
    forms = json.load(f)

for f in forms:
    target_file = f.get('previewFile') or f.get('fileName')
    p = os.path.join(uploads_dir, target_file)
    out_img_name = f"{f['id']}_sheet.png"
    out_img_path = os.path.join(sheets_dir, out_img_name)

    if target_file.endswith('.pdf') and os.path.exists(p):
        doc = pymupdf.open(p)
        page = doc[0]
        pix = page.get_pixmap(dpi=200)
        pix.save(out_img_path)
        f['sheetImage'] = f"/sheets/{out_img_name}"
        print(f"Rendered sheet for {f['id']}: {f['title']} -> {out_img_name}")
    elif (target_file.endswith('.jpg') or target_file.endswith('.png')) and os.path.exists(p):
        shutil.copy2(p, out_img_path)
        f['sheetImage'] = f"/sheets/{out_img_name}"
        print(f"Copied image sheet for {f['id']}: {f['title']} -> {out_img_name}")

with open(forms_path, 'w', encoding='utf-8') as f:
    json.dump(forms, f, ensure_ascii=False, indent=2)

print("Done rendering all sheets!")
