import fitz, os, json
from PIL import Image

BASE_DIR = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system"
FORMS_JSON = os.path.join(BASE_DIR, "data", "forms.json")
UPLOADS_DIR = os.path.join(BASE_DIR, "uploads")
SHEETS_DIR = os.path.join(BASE_DIR, "public", "sheets")

os.makedirs(SHEETS_DIR, exist_ok=True)

with open(FORMS_JSON, "r", encoding="utf-8") as f:
    forms = json.load(f)

for form in forms:
    fid = form["id"]
    file_type = form.get("fileType", "")
    file_name = form.get("fileName", "")
    upload_path = os.path.join(UPLOADS_DIR, file_name)
    pages_list = []

    # Office files (Word/Excel) are marked as isOffice
    if file_type in ["docx", "doc", "xlsx", "xls"]:
        form["isOffice"] = True
        form["pages"] = []
        continue

    form["isOffice"] = False

    if file_name.lower().endswith(".pdf") and os.path.exists(upload_path):
        doc = fitz.open(upload_path)
        page_count = len(doc)
        print(f"Rendering PDF {fid}: {page_count} page(s)")
        
        for p_idx in range(page_count):
            page = doc[p_idx]
            # 200 DPI zoom (matrix 2.08)
            zoom = 2.0833
            mat = fitz.Matrix(zoom, zoom)
            pix = page.get_pixmap(matrix=mat, alpha=False)
            
            page_filename = f"{fid}_page_{p_idx + 1}.png"
            page_path = os.path.join(SHEETS_DIR, page_filename)
            pix.save(page_path)
            pages_list.append(f"/sheets/{page_filename}")
            
            # also save default sheet image as page 1
            if p_idx == 0:
                pix.save(os.path.join(SHEETS_DIR, f"{fid}_sheet.png"))
                form["sheetImage"] = f"/sheets/{fid}_sheet.png"

        form["pages"] = pages_list
        form["pageCount"] = page_count

    elif file_name.lower().endswith((".jpg", ".jpeg", ".png")) and os.path.exists(upload_path):
        print(f"Processing Image {fid}")
        out_name = f"{fid}_page_1.png"
        out_path = os.path.join(SHEETS_DIR, out_name)
        img = Image.open(upload_path).convert("RGB")
        img.save(out_path, "PNG")
        img.save(os.path.join(SHEETS_DIR, f"{fid}_sheet.png"), "PNG")
        form["pages"] = [f"/sheets/{out_name}"]
        form["pageCount"] = 1
        form["sheetImage"] = f"/sheets/{fid}_sheet.png"
    else:
        # Fallback if existing sheet exists
        sheet_path = os.path.join(SHEETS_DIR, f"{fid}_sheet.png")
        if os.path.exists(sheet_path):
            form["pages"] = [f"/sheets/{fid}_sheet.png"]
            form["pageCount"] = 1
        else:
            form["pages"] = []
            form["pageCount"] = 0

with open(FORMS_JSON, "w", encoding="utf-8") as f:
    json.dump(forms, f, ensure_ascii=False, indent=2)

print("All form pages generated and forms.json updated successfully!")
