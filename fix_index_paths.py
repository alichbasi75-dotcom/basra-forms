import os

BASE_DIR = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system"
INDEX_PATH = os.path.join(BASE_DIR, "index.html")

with open(INDEX_PATH, "r", encoding="utf-8") as f:
    c = f.read()

# 1. Update downloadFormFile
old_dl_1 = """    function downloadFormFile(formId) {
      const f = allForms.find(item => item.id === formId);
      if (!f) return;
      const centerCode = activeCenter ? activeCenter.code : '';
      const centerName = activeCenter ? encodeURIComponent(activeCenter.name) : '';
      const downloadUrl = `/api/forms/${f.id}/download?centerCode=${centerCode}&centerName=${centerName}`;
      downloadFileWithPicker(downloadUrl, f.originalName || `${f.title}.${f.fileType}`);
    }"""

new_dl_1 = """    function downloadFormFile(formId) {
      const f = allForms.find(item => item.id === formId);
      if (!f) return;
      trackActivity(f.id, f.title, 'download');
      const downloadUrl = f.fileName ? `./uploads/${encodeURIComponent(f.fileName)}` : `/api/forms/${f.id}/download`;
      downloadFileWithPicker(downloadUrl, f.originalName || `${f.title}.${f.fileType}`);
    }"""

c = c.replace(old_dl_1, new_dl_1)

# 2. Update handleModalDownload
old_dl_2 = """    function handleModalDownload() {
      if (!activeForm) return;
      const centerCode = activeCenter ? activeCenter.code : '';
      const centerName = activeCenter ? encodeURIComponent(activeCenter.name) : '';
      const downloadUrl = `/api/forms/${activeForm.id}/download?centerCode=${centerCode}&centerName=${centerName}`;
      downloadFileWithPicker(downloadUrl, activeForm.originalName || `${activeForm.title}.${activeForm.fileType}`);
    }"""

new_dl_2 = """    function handleModalDownload() {
      if (!activeForm) return;
      trackActivity(activeForm.id, activeForm.title, 'download');
      const downloadUrl = activeForm.fileName ? `./uploads/${encodeURIComponent(activeForm.fileName)}` : `/api/forms/${activeForm.id}/download`;
      downloadFileWithPicker(downloadUrl, activeForm.originalName || `${activeForm.title}.${activeForm.fileType}`);
    }"""

c = c.replace(old_dl_2, new_dl_2)

# 3. Fallback in downloadFileWithPicker
c = c.replace("a.href = downloadUrl;", "a.href = normalizePath(downloadUrl);")

# 4. Fix trackActivity for silent fail on static hosting
old_track = """        await fetch('/api/activity', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ centerCode, centerName, formId, formTitle, action })
        });
      } catch (e) {
        console.error('Tracking error:', e);
      }"""

new_track = """        fetch('/api/activity', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ centerCode, centerName, formId, formTitle, action })
        }).catch(() => {});
      } catch (e) {
        // Silently ignore
      }"""

c = c.replace(old_track, new_track)

# 5. Fix pages arrays to use relative paths
c = c.replace("pages = ['/uploads/' + activeForm.fileName];", "pages = ['./uploads/' + activeForm.fileName];")
c = c.replace("pages = [`/sheets/${activeForm.id}_sheet.png`];", "pages = [`./sheets/${activeForm.id}_sheet.png`];")

# 6. Fix image onerror
c = c.replace("this.src = '/uploads/${activeForm.fileName}';", "this.src = './uploads/${activeForm.fileName}';")

# 7. Fix office download button inside modal
c = c.replace(
    'href="/api/forms/${activeForm.id}/download"',
    'href="${activeForm.fileName ? \'./uploads/\' + encodeURIComponent(activeForm.fileName) : \'/api/forms/\' + activeForm.id + \'/download\'}"'
)

# 8. Fix fileUrl for office app protocol
c = c.replace(
    'const fileUrl = `${window.location.origin}/uploads/${f.fileName}`;',
    'const fileUrl = new URL(\'./uploads/\' + encodeURIComponent(f.fileName), window.location.href).href;'
)
c = c.replace("this.src.indexOf('/uploads/') === -1", "this.src.indexOf('uploads') === -1")

with open(INDEX_PATH, "w", encoding="utf-8") as f:
    f.write(c)

print(f"Successfully refined index.html ({len(c)} bytes)")

