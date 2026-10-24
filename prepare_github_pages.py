import os
import json
import re

BASE_DIR = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system"
CENTER_HTML_PATH = os.path.join(BASE_DIR, "public", "center.html")
INDEX_HTML_PATH = os.path.join(BASE_DIR, "index.html")

with open(CENTER_HTML_PATH, "r", encoding="utf-8") as f:
    content = f.read()

# 1. Add normalizePath helper at beginning of script
helper_func = """
    function normalizePath(p) {
      if (!p) return '';
      if (p.startsWith('/sheets/')) return './sheets/' + p.substring(8);
      if (p.startsWith('/uploads/')) return './uploads/' + p.substring(9);
      if (p.startsWith('/')) return '.' + p;
      return p;
    }
"""
content = content.replace("let allCenters = [];", helper_func + "\n    let allCenters = [];")

# 2. Update loadCenters to load from ./data/centers.json first
old_load_centers = """    async function loadCenters() {
      try {
        const res = await fetch('/api/centers');
        const data = await res.json();
        if (data.success) allCenters = data.centers;
      } catch (e) {
        console.error('Failed to load centers:', e);
      }
    }"""

new_load_centers = """    async function loadCenters() {
      try {
        let res = await fetch('./data/centers.json');
        if (!res.ok) res = await fetch('/api/centers');
        const data = await res.json();
        allCenters = Array.isArray(data) ? data : (data.centers || []);
      } catch (e) {
        try {
          const res = await fetch('/api/centers');
          const data = await res.json();
          allCenters = Array.isArray(data) ? data : (data.centers || []);
        } catch (err) {
          console.error('Failed to load centers:', err);
        }
      }
    }"""
content = content.replace(old_load_centers, new_load_centers)

# 3. Update loadCategories
old_load_categories = """    async function loadCategories() {
      try {
        const res = await fetch('/api/categories');
        const data = await res.json();
        if (data.success) {
          categories = data.categories;
          renderCategories();
        }
      } catch (e) {
        console.error('Failed to load categories:', e);
      }
    }"""

new_load_categories = """    async function loadCategories() {
      try {
        let res = await fetch('./data/categories.json');
        if (!res.ok) res = await fetch('/api/categories');
        const data = await res.json();
        categories = Array.isArray(data) ? data : (data.categories || []);
        renderCategories();
      } catch (e) {
        try {
          const res = await fetch('/api/categories');
          const data = await res.json();
          categories = Array.isArray(data) ? data : (data.categories || []);
          renderCategories();
        } catch (err) {
          console.error('Failed to load categories:', err);
        }
      }
    }"""
content = content.replace(old_load_categories, new_load_categories)

# 4. Update loadForms
old_load_forms = """    async function loadForms() {
      try {
        const res = await fetch('/api/forms');
        const data = await res.json();
        if (data.success) {
          allForms = data.forms;
          filterForms();
        }
      } catch (e) {
        console.error('Failed to load forms:', e);
      }
    }"""

new_load_forms = """    async function loadForms() {
      try {
        let res = await fetch('./data/forms.json');
        if (!res.ok) res = await fetch('/api/forms');
        const data = await res.json();
        let list = Array.isArray(data) ? data : (data.forms || []);
        // Hide inactive forms from centers
        list = list.filter(f => !f.hidden);
        allForms = list;
        filterForms();
      } catch (e) {
        try {
          const res = await fetch('/api/forms');
          const data = await res.json();
          let list = Array.isArray(data) ? data : (data.forms || []);
          list = list.filter(f => !f.hidden);
          allForms = list;
          filterForms();
        } catch (err) {
          console.error('Failed to load forms:', err);
        }
      }
    }"""
content = content.replace(old_load_forms, new_load_forms)

# 5. Normalize sheet image src
content = content.replace('<img src="${pageSrc}"', '<img src="${normalizePath(pageSrc)}"')
content = content.replace("dlBtn.href = `/api/forms/${activeForm.id}/download`;", "dlBtn.href = activeForm.fileName ? `./uploads/${encodeURIComponent(activeForm.fileName)}` : `/api/forms/${activeForm.id}/download`;")
content = content.replace("fetch(downloadUrl)", "fetch(normalizePath(downloadUrl))")

# 6. Make logActivity resilient against static hosting
old_log = """    async function logActivity(action) {
      if (!activeForm || !currentCenter) return;
      try {
        await fetch('/api/activity', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            centerCode: currentCenter.code,
            centerName: currentCenter.name,
            formId: activeForm.id,
            formTitle: activeForm.title,
            action: action
          })
        });
      } catch (e) {
        // Silently ignore activity logging errors
      }
    }"""

new_log = """    async function logActivity(action) {
      if (!activeForm || !currentCenter) return;
      try {
        fetch('/api/activity', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            centerCode: currentCenter.code,
            centerName: currentCenter.name,
            formId: activeForm.id,
            formTitle: activeForm.title,
            action: action
          })
        }).catch(() => {});
      } catch (e) {
        // Silently ignore
      }
    }"""
content = content.replace(old_log, new_log)

# Write to root index.html
with open(INDEX_HTML_PATH, "w", encoding="utf-8") as f:
    f.write(content)

print(f"Successfully created root index.html ({len(content)} bytes)")

# Create .nojekyll
nojekyll_path = os.path.join(BASE_DIR, ".nojekyll")
with open(nojekyll_path, "w", encoding="utf-8") as f:
    f.write("")
print("Created .nojekyll")
