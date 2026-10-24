import os
import json
import fitz

base_dir = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system"
uploads_dir = os.path.join(base_dir, "uploads")
sheets_dir = os.path.join(base_dir, "public", "sheets")
forms_json_path = os.path.join(base_dir, "data", "forms.json")

pdf_path = os.path.join(uploads_dir, "preview_19.pdf")
doc = fitz.open(pdf_path)
page_count = len(doc)
print(f"Rendering {page_count} pages for form_19...")

# Remove old form_19_page_*.png files
for f in os.listdir(sheets_dir):
    if f.startswith("form_19_page_") or f == "form_19_sheet.png":
        try:
            os.remove(os.path.join(sheets_dir, f))
        except Exception:
            pass

pages_list = []
# Render with high DPI (zoom 2.0 = 144-150 DPI)
zoom = 2.0
mat = fitz.Matrix(zoom, zoom)

for idx in range(page_count):
    page = doc[idx]
    pix = page.get_pixmap(matrix=mat, alpha=False)
    p_num = idx + 1
    page_filename = f"form_19_page_{p_num}.png"
    pix.save(os.path.join(sheets_dir, page_filename))
    pages_list.append(f"/sheets/{page_filename}")
    if idx == 0:
        pix.save(os.path.join(sheets_dir, "form_19_sheet.png"))

print(f"Rendered {len(pages_list)} PNG page sheets successfully!")

# Update data/forms.json
with open(forms_json_path, "r", encoding="utf-8") as f:
    forms = json.load(f)

for item in forms:
    if item["id"] == "form_19":
        item["pages"] = pages_list
        item["pageCount"] = page_count
        item["sheetImage"] = "/sheets/form_19_sheet.png"
        item["isOffice"] = True
        print("Updated form_19 in forms.json:")
        print(f"- pageCount: {page_count}")
        print(f"- pages length: {len(item['pages'])}")
        break

with open(forms_json_path, "w", encoding="utf-8") as f:
    json.dump(forms, f, ensure_ascii=False, indent=2)

print("forms.json saved successfully!")
