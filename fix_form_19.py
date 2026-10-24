import os
import win32com.client
import fitz

base_dir = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system"
uploads_dir = os.path.join(base_dir, "uploads")
sheets_dir = os.path.join(base_dir, "public", "sheets")

# Find form_19 xlsx
xlsx_files = [f for f in os.listdir(uploads_dir) if f.startswith("form_19") and f.endswith(".xlsx")]
if not xlsx_files:
    print("form_19 not found in uploads")
    exit(1)

xlsx_path = os.path.join(uploads_dir, xlsx_files[0])
output_pdf = os.path.join(uploads_dir, "preview_19.pdf")

print("Opening:", xlsx_path)

excel = win32com.client.Dispatch("Excel.Application")
excel.Visible = False
excel.DisplayAlerts = False

try:
    wb = excel.Workbooks.Open(xlsx_path)
    ws = wb.Worksheets(1)
    
    # Ensure Right-To-Left or standard view
    # Set page setup
    ws.PageSetup.Orientation = 1  # 1 = xlPortrait, 2 = xlLandscape
    ws.PageSetup.PaperSize = 9    # 9 = xlPaperA4
    ws.PageSetup.Zoom = False
    ws.PageSetup.FitToPagesWide = 1
    ws.PageSetup.FitToPagesTall = False
    
    # Auto-fit columns nicely
    ws.Columns("A:C").AutoFit()
    
    # Center horizontally on page
    ws.PageSetup.CenterHorizontally = True
    
    # Export to PDF
    ws.ExportAsFixedFormat(0, output_pdf)
    wb.Close(False)
    print("Exported preview_19.pdf successfully!")
finally:
    excel.Quit()

# Inspect generated PDF
doc = fitz.open(output_pdf)
print(f"Total pages generated: {len(doc)}")
for i in range(min(5, len(doc))):
    page = doc[i]
    txt = page.get_text().strip().split("\n")
    print(f"Page {i+1}: {len(txt)} lines. Sample text: {txt[:6]}")
