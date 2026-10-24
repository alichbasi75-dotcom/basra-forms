import sys
import os
import json
import shutil
import pymupdf

def main():
    if len(sys.argv) < 5:
        print(json.dumps({"success": False, "error": "Missing arguments"}))
        return

    upload_file_path = os.path.abspath(sys.argv[1])
    sheets_dir = os.path.abspath(sys.argv[2])
    form_id = sys.argv[3]
    file_type = sys.argv[4].lower()

    os.makedirs(sheets_dir, exist_ok=True)

    if not os.path.exists(upload_file_path):
        print(json.dumps({"success": False, "error": "Upload file does not exist"}))
        return

    pages = []
    sheet_image = f"/sheets/{form_id}_sheet.png"

    # 1. IMAGE FILES
    if file_type == 'image' or upload_file_path.lower().endswith(('.png', '.jpg', '.jpeg', '.webp')):
        page1_name = f"{form_id}_page_1.png"
        sheet_name = f"{form_id}_sheet.png"
        shutil.copy2(upload_file_path, os.path.join(sheets_dir, page1_name))
        shutil.copy2(upload_file_path, os.path.join(sheets_dir, sheet_name))
        print(json.dumps({
            "success": True,
            "pages": [f"/sheets/{page1_name}"],
            "pageCount": 1,
            "sheetImage": f"/sheets/{sheet_name}",
            "isOffice": False
        }))
        return

    # 2. WORD DOCUMENTS (.docx, .doc)
    if file_type in ['docx', 'doc'] or upload_file_path.lower().endswith(('.docx', '.doc')):
        temp_pdf = os.path.join(sheets_dir, f"_temp_{form_id}.pdf")
        word_app = None
        doc = None
        try:
            import win32com.client
            import pythoncom
            pythoncom.CoInitialize()
            word_app = win32com.client.Dispatch("Word.Application")
            word_app.Visible = False
            word_app.DisplayAlerts = False
            word_app.ScreenUpdating = False
            doc = word_app.Documents.Open(upload_file_path, ReadOnly=True, ConfirmConversions=False)
            doc.SaveAs(temp_pdf, FileFormat=17) # 17 = wdFormatPDF
            doc.Close(False)
            doc = None
            word_app.Quit()
            word_app = None

            # Render generated PDF pages
            if os.path.exists(temp_pdf):
                pdf_doc = pymupdf.open(temp_pdf)
                page_count = len(pdf_doc)
                for idx, page in enumerate(pdf_doc):
                    pix = page.get_pixmap(dpi=200)
                    p_name = f"{form_id}_page_{idx + 1}.png"
                    pix.save(os.path.join(sheets_dir, p_name))
                    pages.append(f"/sheets/{p_name}")
                    if idx == 0:
                        pix.save(os.path.join(sheets_dir, f"{form_id}_sheet.png"))
                pdf_doc.close()
                os.remove(temp_pdf)

                print(json.dumps({
                    "success": True,
                    "pages": pages,
                    "pageCount": len(pages),
                    "sheetImage": f"/sheets/{form_id}_sheet.png",
                    "isOffice": True
                }))
                return
        except Exception as e:
            if doc:
                try: doc.Close(False)
                except: pass
            if word_app:
                try: word_app.Quit()
                except: pass
            if os.path.exists(temp_pdf):
                try: os.remove(temp_pdf)
                except: pass
            # Fallback if COM conversion fails
            print(json.dumps({
                "success": False,
                "error": str(e),
                "pages": [],
                "pageCount": 0,
                "sheetImage": None,
                "isOffice": True
            }))
            return

    # 3. EXCEL SPREADSHEETS (.xlsx, .xls)
    if file_type in ['xlsx', 'xls'] or upload_file_path.lower().endswith(('.xlsx', '.xls')):
        temp_pdf = os.path.join(sheets_dir, f"_temp_{form_id}.pdf")
        excel_app = None
        wb = None
        try:
            import win32com.client
            import pythoncom
            pythoncom.CoInitialize()
            excel_app = win32com.client.Dispatch("Excel.Application")
            excel_app.Visible = False
            excel_app.DisplayAlerts = False
            excel_app.ScreenUpdating = False
            wb = excel_app.Workbooks.Open(upload_file_path, ReadOnly=True, CorruptLoad=1)
            wb.ExportAsFixedFormat(0, temp_pdf) # 0 = xlTypePDF
            wb.Close(False)
            wb = None
            excel_app.Quit()
            excel_app = None

            if os.path.exists(temp_pdf):
                pdf_doc = pymupdf.open(temp_pdf)
                page_count = len(pdf_doc)
                for idx, page in enumerate(pdf_doc):
                    pix = page.get_pixmap(dpi=200)
                    p_name = f"{form_id}_page_{idx + 1}.png"
                    pix.save(os.path.join(sheets_dir, p_name))
                    pages.append(f"/sheets/{p_name}")
                    if idx == 0:
                        pix.save(os.path.join(sheets_dir, f"{form_id}_sheet.png"))
                pdf_doc.close()
                os.remove(temp_pdf)

                print(json.dumps({
                    "success": True,
                    "pages": pages,
                    "pageCount": len(pages),
                    "sheetImage": f"/sheets/{form_id}_sheet.png",
                    "isOffice": True
                }))
                return
        except Exception as e:
            if wb:
                try: wb.Close(False)
                except: pass
            if excel_app:
                try: excel_app.Quit()
                except: pass
            if os.path.exists(temp_pdf):
                try: os.remove(temp_pdf)
                except: pass
            print(json.dumps({
                "success": False,
                "error": str(e),
                "pages": [],
                "pageCount": 0,
                "sheetImage": None,
                "isOffice": True
            }))
            return

    # 4. PDF DOCUMENTS (.pdf)
    if file_type == 'pdf' or upload_file_path.lower().endswith('.pdf'):
        try:
            pdf_doc = pymupdf.open(upload_file_path)
            page_count = len(pdf_doc)
            for idx, page in enumerate(pdf_doc):
                pix = page.get_pixmap(dpi=200)
                p_name = f"{form_id}_page_{idx + 1}.png"
                pix.save(os.path.join(sheets_dir, p_name))
                pages.append(f"/sheets/{p_name}")
                if idx == 0:
                    pix.save(os.path.join(sheets_dir, f"{form_id}_sheet.png"))
            pdf_doc.close()

            print(json.dumps({
                "success": True,
                "pages": pages,
                "pageCount": len(pages),
                "sheetImage": f"/sheets/{form_id}_sheet.png",
                "isOffice": False
            }))
            return
        except Exception as e:
            print(json.dumps({
                "success": False,
                "error": str(e),
                "pages": [],
                "pageCount": 0,
                "sheetImage": None,
                "isOffice": False
            }))
            return

    # Unknown
    print(json.dumps({"success": False, "error": "Unsupported file type"}))

if __name__ == '__main__':
    main()
