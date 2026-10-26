const express = require('express');
const cors = require('cors');
const path = require('path');
const fs = require('fs');
const multer = require('multer');

const app = express();
const PORT = process.env.PORT || 3000;

app.use((req, res, next) => {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, x-admin-key');
  res.setHeader('Access-Control-Allow-Private-Network', 'true');
  if (req.method === 'OPTIONS') return res.status(200).end();
  next();
});
app.use(cors());
app.use(express.json({ limit: '50mb' }));
app.use(express.urlencoded({ extended: true }));

const DATA_DIR = path.join(__dirname, 'data');
const UPLOADS_DIR = path.join(__dirname, 'uploads');
const PUBLIC_DIR = path.join(__dirname, 'public');
const SHEETS_DIR = path.join(__dirname, 'sheets');
if (!fs.existsSync(SHEETS_DIR)) fs.mkdirSync(SHEETS_DIR, { recursive: true });

if (!fs.existsSync(UPLOADS_DIR)) fs.mkdirSync(UPLOADS_DIR, { recursive: true });
if (!fs.existsSync(DATA_DIR)) fs.mkdirSync(DATA_DIR, { recursive: true });

// Helpers
function readJson(filename, defaultValue = []) {
  const filePath = path.join(DATA_DIR, filename);
  try {
    if (!fs.existsSync(filePath)) {
      fs.writeFileSync(filePath, JSON.stringify(defaultValue, null, 2), 'utf8');
      return defaultValue;
    }
    return JSON.parse(fs.readFileSync(filePath, 'utf8'));
  } catch (err) {
    console.error(`Error reading ${filename}:`, err);
    return defaultValue;
  }
}

function writeJson(filename, data) {
  const filePath = path.join(DATA_DIR, filename);
  const content = JSON.stringify(data, null, 2);
  fs.writeFileSync(filePath, content, 'utf8');
  try {
    const pubDataDir = path.join(PUBLIC_DIR, 'data');
    if (!fs.existsSync(pubDataDir)) fs.mkdirSync(pubDataDir, { recursive: true });
    fs.writeFileSync(path.join(pubDataDir, filename), content, 'utf8');
  } catch (e) {}
}

// Activity logging for Centers (views, downloads, prints)
function logActivity(centerCode, centerName, formId, formTitle, action, customId, customTimestamp, customFormattedTime) {
  try {
    const logs = readJson('activity_log.json', []);
    const now = new Date();
    // Baghdad/Basra UTC+3 time
    const timeFormatter = new Intl.DateTimeFormat('ar-IQ', {
      timeZone: 'Asia/Baghdad',
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true
    });

    let resolvedName = centerName || 'غير محدد';
    if (centerCode && (!resolvedName || resolvedName === 'غير محدد' || resolvedName.startsWith('مركز '))) {
      const centers = readJson('centers.json', []);
      const found = centers.find(c => String(c.code) === String(centerCode));
      if (found && found.name) resolvedName = found.name;
    }

    const entryId = customId || ('act_' + Date.now() + '_' + Math.round(Math.random() * 1000));
    const existing = logs.find(l => l.id === entryId);
    if (existing) return existing;

    const entry = {
      id: entryId,
      timestamp: customTimestamp || now.toISOString(),
      formattedTime: customFormattedTime || timeFormatter.format(now),
      centerCode: centerCode || 'غير محدد',
      centerName: resolvedName,
      formId: formId || '',
      formTitle: formTitle || '',
      action: action || 'view' // 'view' (عرض), 'download' (تنزيل), 'print' (طباعة), 'office' (فتح بأوفيس)
    };
    logs.unshift(entry);
    if (logs.length > 2000) logs.length = 2000;
    writeJson('activity_log.json', logs);

    if (action === 'download' && formId) {
      try {
        const forms = readJson('forms.json', []);
        const fIdx = forms.findIndex(f => f.id === formId);
        if (fIdx !== -1) {
          forms[fIdx].downloads = (forms[fIdx].downloads || 0) + 1;
          writeJson('forms.json', forms);
        }
      } catch (e) {}
    }

    return entry;
  } catch (err) {
    console.error('Error logging activity:', err);
  }
}

// Multer storage
const storage = multer.diskStorage({
  destination: (req, file, cb) => {
    cb(null, UPLOADS_DIR);
  },
  filename: (req, file, cb) => {
    const ext = path.extname(file.originalname).toLowerCase();
    const safeName = Date.now() + '_' + Math.round(Math.random() * 1E6) + ext;
    cb(null, safeName);
  }
});

const upload = multer({
  storage: storage,
  limits: { fileSize: 50 * 1024 * 1024 }, // 50MB
  fileFilter: (req, file, cb) => {
    const ext = path.extname(file.originalname).toLowerCase();
    const allowed = ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.png', '.jpg', '.jpeg'];
    if (allowed.includes(ext)) {
      cb(null, true);
    } else {
      cb(new Error('الملف غير مدعوم! الصيغ المدعومة هي: PDF, Word, Excel, صور'));
    }
  }
});

// Static serving
app.use(express.static(PUBLIC_DIR));
app.use('/uploads', express.static(UPLOADS_DIR));

// Page routes
app.get('/', (req, res) => {
  res.redirect('/center');
});

app.get('/center', (req, res) => {
  res.sendFile('center.html', { root: PUBLIC_DIR });
});

app.get('/admin', (req, res) => {
  res.sendFile('admin.html', { root: PUBLIC_DIR });
});

// API Routes
app.get('/api/centers', (req, res) => {
  const centers = readJson('centers.json', []);
  res.json({ success: true, count: centers.length, centers });
});

app.get('/api/categories', (req, res) => {
  const categories = readJson('categories.json', []);
  res.json({ success: true, categories });
});

app.get('/api/forms', (req, res) => {
  let forms = readJson('forms.json', []);
  const { category, type, q, include_hidden } = req.query;

  // إخفاء النماذج المتوقف العمل بها مؤقتاً عن واجهة المراكز
  if (include_hidden !== 'true') {
    forms = forms.filter(f => !f.hidden);
  }

  if (category && category !== 'all') {
    forms = forms.filter(f => f.category === category);
  }
  if (type && type !== 'all') {
    forms = forms.filter(f => f.fileType === type);
  }
  if (q && q.trim()) {
    const term = q.trim().toLowerCase();
    forms = forms.filter(f => 
      (f.title && f.title.toLowerCase().includes(term)) ||
      (f.description && f.description.toLowerCase().includes(term))
    );
  }

  res.json({ success: true, count: forms.length, forms });
});

// Admin get all forms (including temporarily hidden ones)
app.get('/api/admin/forms', (req, res) => {
  const forms = readJson('forms.json', []);
  res.json({ success: true, count: forms.length, forms });
});

app.get('/api/forms/:id', (req, res) => {
  const forms = readJson('forms.json', []);
  const form = forms.find(f => f.id === req.params.id);
  if (!form) return res.status(404).json({ success: false, message: 'النموذج غير موجود' });
  res.json({ success: true, form });
});

// Download and increment download counter with exact original filename
app.get('/api/forms/:id/download', (req, res) => {
  const forms = readJson('forms.json', []);
  const formIndex = forms.findIndex(f => f.id === req.params.id);
  if (formIndex === -1) return res.status(404).send('النموذج غير موجود');

  const form = forms[formIndex];
  forms[formIndex].downloads = (forms[formIndex].downloads || 0) + 1;
  writeJson('forms.json', forms);

  // Log center activity
  const centerCode = req.query.centerCode || '';
  const centerName = req.query.centerName || '';
  logActivity(centerCode, centerName, form.id, form.title, 'download');

  const filePath = path.join(UPLOADS_DIR, form.fileName);
  if (!fs.existsSync(filePath)) {
    return res.status(404).send('الملف غير متوفر على السيرفر');
  }

  const originalName = form.originalName || form.fileName;
  const ext = path.extname(originalName) || path.extname(form.fileName);
  const fallbackAsciiName = `form_${form.id}${ext}`;
  const encodedUtf8Name = encodeURIComponent(originalName);

  // Set RFC 5987 / RFC 6266 standard Content-Disposition for full Arabic Unicode support
  res.setHeader(
    'Content-Disposition',
    `attachment; filename="${fallbackAsciiName}"; filename*=UTF-8''${encodedUtf8Name}`
  );

  res.sendFile(form.fileName, { root: UPLOADS_DIR }, (err) => {
    if (err && !res.headersSent) {
      console.error('Download sendFile error:', err);
      res.status(500).send('خطأ أثناء تحميل الملف');
    }
  });
});

// Endpoint to log user actions (view, download, print, office open)
app.post('/api/activity', (req, res) => {
  const { id, timestamp, formattedTime, centerCode, centerName, formId, formTitle, action } = req.body || {};
  const entry = logActivity(centerCode, centerName, formId, formTitle, action, id, timestamp, formattedTime);
  res.json({ success: true, entry });
});

// Admin endpoint to clear activity logs
app.post('/api/admin/activity/clear', (req, res) => {
  writeJson('activity_log.json', []);
  res.json({ success: true });
});

// Admin endpoint to view center activity logs
app.get('/api/admin/activity', (req, res) => {
  const logs = readJson('activity_log.json', []);
  const { centerCode, action, limit } = req.query;
  let filtered = logs;

  if (centerCode) {
    filtered = filtered.filter(l => l.centerCode === centerCode);
  }
  if (action && action !== 'all') {
    filtered = filtered.filter(l => l.action === action);
  }

  const max = parseInt(limit) || 200;
  res.json({
    success: true,
    total: filtered.length,
    logs: filtered.slice(0, max)
  });
});

// Admin endpoint for activity statistics summary
app.get('/api/admin/activity/stats', (req, res) => {
  const logs = readJson('activity_log.json', []);
  const forms = readJson('forms.json', []);
  const centers = readJson('centers.json', []);

  let totalViews = 0;
  let totalDownloads = 0;
  let totalPrints = 0;
  let totalOffice = 0;

  const centerStats = {};
  const formStats = {};

  logs.forEach(l => {
    if (l.action === 'view') totalViews++;
    else if (l.action === 'download') totalDownloads++;
    else if (l.action === 'print') totalPrints++;
    else if (l.action === 'office') totalOffice++;

    if (l.centerCode && l.centerCode !== 'غير محدد') {
      if (!centerStats[l.centerCode]) {
        centerStats[l.centerCode] = {
          code: l.centerCode,
          name: l.centerName,
          views: 0,
          downloads: 0,
          prints: 0,
          total: 0
        };
      }
      centerStats[l.centerCode].total++;
      if (l.action === 'view') centerStats[l.centerCode].views++;
      if (l.action === 'download') centerStats[l.centerCode].downloads++;
      if (l.action === 'print') centerStats[l.centerCode].prints++;
    }

    if (l.formTitle) {
      formStats[l.formTitle] = (formStats[l.formTitle] || 0) + 1;
    }
  });

  const activeCentersList = Object.values(centerStats).sort((a, b) => b.total - a.total);

  res.json({
    success: true,
    stats: {
      totalLogs: logs.length,
      totalViews,
      totalDownloads,
      totalPrints,
      totalOffice,
      activeCentersCount: activeCentersList.length,
      topCenters: activeCentersList.slice(0, 10),
      allCentersStats: activeCentersList
    }
  });
});

// Admin delete/clear all activity logs (حذف سجل العمليات وتصفير الجدول)
app.delete('/api/admin/activity', (req, res) => {
  try {
    writeJson('activity_log.json', []);
    res.json({ success: true, message: 'تم مسح وحذف سجل نشاط وعمليات المراكز بالكامل' });
  } catch (err) {
    res.status(500).json({ success: false, message: err.message });
  }
});

// Admin authentication
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD || 'basra2026';

app.post('/api/admin/login', (req, res) => {
  const { password } = req.body;
  if (password === ADMIN_PASSWORD) {
    res.json({ 
      success: true, 
      token: 'admin_token_' + Date.now(), 
      office: 'مكتب انتخابات محافظة البصرة' 
    });
  } else {
    res.status(401).json({ success: false, message: 'كلمة المرور غير صحيحة!' });
  }
});

// Function to render / process sheets for forms automatically (Word, Excel, PDF, Image)
function processFormSheets(form) {
  const SHEETS_DIR = path.join(__dirname, 'public', 'sheets');
  if (!fs.existsSync(SHEETS_DIR)) {
    fs.mkdirSync(SHEETS_DIR, { recursive: true });
  }

  const uploadPath = path.join(UPLOADS_DIR, form.fileName);
  if (!fs.existsSync(uploadPath)) {
    form.pages = form.pages || [];
    form.pageCount = form.pages.length || 0;
    return form;
  }

  const isImage = form.fileType === 'image' || ['.png', '.jpg', '.jpeg', '.webp'].some(ext => (form.fileName || '').toLowerCase().endsWith(ext));
  const isOffice = ['docx', 'doc', 'xlsx', 'xls'].includes(form.fileType) || ['.docx', '.doc', '.xlsx', '.xls'].some(ext => (form.fileName || '').toLowerCase().endsWith(ext));

  // Quick instant copy for image files
  if (isImage) {
    try {
      const page1Name = `${form.id}_page_1.png`;
      const sheetName = `${form.id}_sheet.png`;
      fs.copyFileSync(uploadPath, path.join(SHEETS_DIR, page1Name));
      fs.copyFileSync(uploadPath, path.join(SHEETS_DIR, sheetName));

      form.sheetImage = `/sheets/${sheetName}`;
      form.pages = [`/sheets/${page1Name}`];
      form.pageCount = 1;
      form.isOffice = false;
      return form;
    } catch (e) {
      console.error('Error processing image sheets:', e);
      form.sheetImage = `/uploads/${form.fileName}`;
      form.pages = [`/uploads/${form.fileName}`];
      form.pageCount = 1;
      form.isOffice = false;
      return form;
    }
  }

  // Unified converter for Word, Excel, and PDF via render_office_and_pdf.py
  try {
    const { execFileSync } = require('child_process');
    const pyScript = path.join(__dirname, 'render_office_and_pdf.py');

    const pyExe = fs.existsSync('C:\\Python314\\python.exe') ? 'C:\\Python314\\python.exe' : 'python';
    const stdout = execFileSync(pyExe, [pyScript, uploadPath, SHEETS_DIR, form.id, form.fileType], {
      encoding: 'utf8',
      timeout: 45000
    });

    const lines = stdout.trim().split('\n');
    const lastLine = lines[lines.length - 1];
    const parsed = JSON.parse(lastLine);

    if (parsed && parsed.success) {
      form.pages = parsed.pages || [];
      form.pageCount = parsed.pageCount || (form.pages ? form.pages.length : 0);
      form.sheetImage = parsed.sheetImage || (form.pages.length > 0 ? form.pages[0] : null);
      form.isOffice = Boolean(parsed.isOffice);
      return form;
    } else {
      console.error('render_office_and_pdf error:', parsed ? parsed.error : 'Unknown');
    }
  } catch (err) {
    console.error('render_office_and_pdf execution failed:', err);
  }

  if (isOffice) {
    form.isOffice = true;
    form.pages = form.pages || [];
    form.pageCount = form.pages.length || 0;
    const defaultSheet = path.join(SHEETS_DIR, `${form.id}_sheet.png`);
    if (fs.existsSync(defaultSheet)) {
      form.sheetImage = `/sheets/${form.id}_sheet.png`;
    }
  }

  return form;
}


// Endpoint to render Word/Excel base64 to PNG sheets via Word/Excel COM + PyMuPDF
app.post('/api/render-office-base64', (req, res) => {
  try {
    const { fileName, fileType, formId, contentBase64 } = req.body || {};
    if (!fileName || !contentBase64 || !formId) {
      return res.status(400).json({ success: false, error: 'Missing parameters' });
    }
    const uploadPath = path.join(UPLOADS_DIR, fileName);
    fs.writeFileSync(uploadPath, Buffer.from(contentBase64, 'base64'));
    const tempForm = processFormSheets({ id: formId, fileName, fileType: fileType || 'doc' });
    const pageBase64List = [];
    const publicSheetsDir = path.join(__dirname, 'public', 'sheets');
    const rootSheetsDir = path.join(__dirname, 'sheets');
    if (!fs.existsSync(rootSheetsDir)) fs.mkdirSync(rootSheetsDir, { recursive: true });
    (tempForm.pages || []).forEach(pg => {
      const baseName = path.basename(pg);
      const fullP = path.join(publicSheetsDir, baseName);
      if (fs.existsSync(fullP)) {
        fs.copyFileSync(fullP, path.join(rootSheetsDir, baseName));
        pageBase64List.push(fs.readFileSync(fullP).toString('base64'));
      }
    });
    res.json({ success: pageBase64List.length > 0, pageBase64List, pages: tempForm.pages });
  } catch (err) {
    res.status(500).json({ success: false, error: err.message });
  }
});

// Admin upload form
app.post('/api/admin/forms', upload.single('file'), (req, res) => {
  try {
    const { title, category, description, version, fillableTemplateId } = req.body;
    if (!title || !category) {
      return res.status(400).json({ success: false, message: 'يرجى ملء جميع الحقول الإلزامية' });
    }

    let fileType = 'pdf';
    let fileName = '';
    let fileSize = '0 KB';
    let originalName = '';

    if (req.file) {
      fileName = req.file.filename;
      originalName = req.file.originalname;
      const ext = path.extname(req.file.originalname).toLowerCase();
      if (['.doc', '.docx'].includes(ext)) fileType = 'docx';
      else if (['.xls', '.xlsx'].includes(ext)) fileType = 'xlsx';
      else if (['.png', '.jpg', '.jpeg'].includes(ext)) fileType = 'image';
      else fileType = 'pdf';

      fileSize = (req.file.size / 1024).toFixed(1) + ' KB';
      if (req.file.size > 1024 * 1024) {
        fileSize = (req.file.size / (1024 * 1024)).toFixed(1) + ' MB';
      }
    } else {
      return res.status(400).json({ success: false, message: 'يرجى إرفاق ملف النموذج (PDF / Word / Excel / صورة)' });
    }

    const forms = readJson('forms.json', []);
    let newForm = {
      id: 'form_' + Date.now(),
      title,
      category,
      fileType,
      fileName,
      originalName,
      fileSize,
      uploadDate: new Date().toISOString().split('T')[0],
      updatedDate: new Date().toISOString().split('T')[0],
      version: version || 'v1.0',
      description: description || '',
      fillableTemplateId: fillableTemplateId || null,
      hidden: false,
      downloads: 0
    };

    // Render sheets and pages automatically
    newForm = processFormSheets(newForm);

    forms.unshift(newForm);
    writeJson('forms.json', forms);

    res.json({ success: true, message: 'تم رفع النموذج بنجاح وتعميمه على المراكز', form: newForm });
  } catch (err) {
    res.status(500).json({ success: false, message: err.message });
  }
});

// Admin toggle form visibility (إخفاء / إظهار النموذج عن المراكز)
app.put('/api/admin/forms/:id/toggle-visibility', (req, res) => {
  try {
    let forms = readJson('forms.json', []);
    const idx = forms.findIndex(f => f.id === req.params.id);
    if (idx === -1) return res.status(404).json({ success: false, message: 'النموذج غير موجود' });

    if (req.body && typeof req.body.hidden === 'boolean') {
      forms[idx].hidden = req.body.hidden;
    } else {
      forms[idx].hidden = !forms[idx].hidden;
    }
    forms[idx].updatedDate = new Date().toISOString().split('T')[0];

    writeJson('forms.json', forms);
    res.json({
      success: true,
      message: forms[idx].hidden ? 'تم إخفاء النموذج عن المراكز بنجاح' : 'تم تفعيل وإظهار النموذج للمراكز بنجاح',
      hidden: forms[idx].hidden,
      form: forms[idx]
    });
  } catch (err) {
    res.status(500).json({ success: false, message: err.message });
  }
});

// Admin edit form (تعديل النموذج)
app.put('/api/admin/forms/:id', (req, res) => {
  try {
    const { title, description, category, hidden } = req.body;
    let forms = readJson('forms.json', []);
    const idx = forms.findIndex(f => f.id === req.params.id);
    if (idx === -1) return res.status(404).json({ success: false, message: 'النموذج غير موجود' });

    if (title) forms[idx].title = title.trim();
    if (description !== undefined) forms[idx].description = description.trim();
    if (category) forms[idx].category = category;
    if (hidden !== undefined) forms[idx].hidden = Boolean(hidden);
    forms[idx].updatedDate = new Date().toISOString().split('T')[0];

    writeJson('forms.json', forms);
    res.json({ success: true, message: 'تم تعديل بيانات النموذج بنجاح', form: forms[idx] });
  } catch (err) {
    res.status(500).json({ success: false, message: err.message });
  }
});

// Admin delete form
app.delete('/api/admin/forms/:id', (req, res) => {
  let forms = readJson('forms.json', []);
  const form = forms.find(f => f.id === req.params.id);
  if (!form) return res.status(404).json({ success: false, message: 'النموذج غير موجود' });

  // Delete actual file if exists
  if (form.fileName) {
    const p = path.join(UPLOADS_DIR, form.fileName);
    if (fs.existsSync(p)) {
      try { fs.unlinkSync(p); } catch (e) { console.error(e); }
    }
  }

  forms = forms.filter(f => f.id !== req.params.id);
  writeJson('forms.json', forms);

  res.json({ success: true, message: 'تم حذف النموذج بنجاح' });
});

// Admin categories management (إدارة الأصناف: إضافة، تعديل، حذف)
app.post('/api/admin/categories', (req, res) => {
  try {
    const { name, icon } = req.body;
    if (!name || !name.trim()) {
      return res.status(400).json({ success: false, message: 'يرجى إدخال اسم الصنف' });
    }
    const categories = readJson('categories.json', []);
    const id = req.body.id && req.body.id.trim() ? req.body.id.trim() : 'cat_' + Date.now();
    if (categories.some(c => c.id === id || c.name === name.trim())) {
      return res.status(400).json({ success: false, message: 'هذا الصنف موجود مسبقاً' });
    }
    const newCat = {
      id,
      name: name.trim(),
      icon: icon && icon.trim() ? icon.trim() : 'fa-folder'
    };
    categories.push(newCat);
    writeJson('categories.json', categories);
    res.json({ success: true, message: 'تم إضافة الصنف بنجاح', category: newCat });
  } catch (err) {
    res.status(500).json({ success: false, message: err.message });
  }
});

app.put('/api/admin/categories/:id', (req, res) => {
  try {
    const { name, icon } = req.body;
    let categories = readJson('categories.json', []);
    const idx = categories.findIndex(c => c.id === req.params.id);
    if (idx === -1) return res.status(404).json({ success: false, message: 'الصنف غير موجود' });

    if (name && name.trim()) {
      const oldName = categories[idx].name;
      categories[idx].name = name.trim();

      // Update forms that have this category
      let forms = readJson('forms.json', []);
      let updatedForms = false;
      forms.forEach(f => {
        if (f.category === req.params.id) {
          f.categoryName = name.trim();
          updatedForms = true;
        }
      });
      if (updatedForms) writeJson('forms.json', forms);
    }
    if (icon && icon.trim()) {
      categories[idx].icon = icon.trim();
    }

    writeJson('categories.json', categories);
    res.json({ success: true, message: 'تم تعديل بيانات الصنف بنجاح', category: categories[idx] });
  } catch (err) {
    res.status(500).json({ success: false, message: err.message });
  }
});

app.delete('/api/admin/categories/:id', (req, res) => {
  try {
    let categories = readJson('categories.json', []);
    const cat = categories.find(c => c.id === req.params.id);
    if (!cat) return res.status(404).json({ success: false, message: 'الصنف غير موجود' });

    // Check if any forms currently use this category
    const forms = readJson('forms.json', []);
    const inUse = forms.filter(f => f.category === req.params.id);
    if (inUse.length > 0) {
      return res.status(400).json({
        success: false,
        message: `لا يمكن حذف هذا الصنف لأن هناك (${inUse.length}) نموذج مرتبط به. يرجى تعديل تصنيف النماذج أولاً.`
      });
    }

    categories = categories.filter(c => c.id !== req.params.id);
    writeJson('categories.json', categories);
    res.json({ success: true, message: 'تم حذف الصنف بنجاح' });
  } catch (err) {
    res.status(500).json({ success: false, message: err.message });
  }
});

// Admin stats
app.get('/api/stats', (req, res) => {
  const forms = readJson('forms.json', []);
  const centers = readJson('centers.json', []);

  const totalDownloads = forms.reduce((acc, f) => acc + (f.downloads || 0), 0);
  const byType = {
    docx: forms.filter(f => f.fileType === 'docx').length,
    xlsx: forms.filter(f => f.fileType === 'xlsx').length,
    pdf: forms.filter(f => f.fileType === 'pdf').length,
    image: forms.filter(f => f.fileType === 'image').length,
  };

  res.json({
    success: true,
    stats: {
      totalForms: forms.length,
      totalCenters: centers.length,
      totalDownloads,
      byType
    }
  });
});

// Check and self-heal any forms missing pages or sheet images
try {
  const existingForms = readJson('forms.json', []);
  let updated = false;
  existingForms.forEach(f => {
    const hasPages = f.pages && f.pages.length > 0;
    const hasSheet = f.sheetImage && fs.existsSync(path.join(__dirname, 'public', f.sheetImage));
    if (!hasPages && !hasSheet) {
      if (f.fileName && fs.existsSync(path.join(UPLOADS_DIR, f.fileName))) {
        console.log(`Auto-generating missing pages/sheets for form: ${f.id} (${f.title}) [${f.fileType}]`);
        processFormSheets(f);
        updated = true;
      }
    }
  });
  if (updated) {
    writeJson('forms.json', existingForms);
  }
} catch (e) {
  console.error('Self-healing forms check error:', e);
}

// POST /api/admin/publish-to-cloud
app.post('/api/admin/publish-to-cloud', async (req, res) => {
  try {
    const { cloudUrl, adminKey } = req.body || {};
    const targetUrl = (cloudUrl || 'https://basra-forms.vercel.app/api/sync').trim();
    const key = (adminKey || 'BasraForms@2026').trim();

    const forms = readJson('forms.json');
    const categories = readJson('categories.json');

    const filesToSend = [];

    if (fs.existsSync(UPLOADS_DIR)) {
      const uFiles = fs.readdirSync(UPLOADS_DIR);
      for (const fn of uFiles) {
        const full = path.join(UPLOADS_DIR, fn);
        const st = fs.statSync(full);
        if (st.isFile() && st.size < 10 * 1024 * 1024) {
          filesToSend.push({
            path: `uploads/${fn}`,
            contentBase64: fs.readFileSync(full).toString('base64')
          });
        }
      }
    }

    if (fs.existsSync(SHEETS_DIR)) {
      const sFiles = fs.readdirSync(SHEETS_DIR);
      for (const fn of sFiles) {
        if (!fn.endsWith('.png')) continue;
        const full = path.join(SHEETS_DIR, fn);
        const st = fs.statSync(full);
        if (st.isFile() && st.size < 10 * 1024 * 1024) {
          filesToSend.push({
            path: `sheets/${fn}`,
            contentBase64: fs.readFileSync(full).toString('base64')
          });
        }
      }
    }

    const fetchRes = await fetch(targetUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'x-admin-key': key
      },
      body: JSON.stringify({
        forms,
        categories,
        files: filesToSend
      })
    });

    const result = await fetchRes.json();
    if (!fetchRes.ok) {
      return res.status(fetchRes.status).json(result);
    }

    return res.json({
      success: true,
      message: result.message || 'تم نشر وتحديث المنظومة بنجاح لجميع المراكز أونلاين!',
      count: result.updatedItemsCount
    });

  } catch (err) {
    console.error('Publish error:', err);
    return res.status(500).json({ success: false, error: err.message || 'فشل الاتصال بالوسيط السحابي' });
  }
});


// ==========================================
// AUTOMATIC CLOUD SYNC & AUTO-UPDATE
// ==========================================
const CLOUD_SYNC_SOURCES = [
  'https://basra-forms.vercel.app',
  'https://alichbasi75-dotcom.github.io/basra-forms'
];

async function autoSyncWithCloud() {
  console.log('[Auto-Sync] Checking for latest cloud updates from GitHub/Vercel...');
  for (const baseUrl of CLOUD_SYNC_SOURCES) {
    try {
      const res = await fetch(`${baseUrl}/data/forms.json?t=${Date.now()}`, { signal: AbortSignal.timeout(6000) });
      if (!res.ok) continue;
      const cloudForms = await res.json();
      if (!Array.isArray(cloudForms) || cloudForms.length === 0) continue;

      const localForms = readJson('forms.json') || [];
      const hasDifferences = (JSON.stringify(cloudForms) !== JSON.stringify(localForms));

      // Download missing files
      for (const f of cloudForms) {
        if (f.fileName) {
          const localUpload = path.join(UPLOADS_DIR, f.fileName);
          if (!fs.existsSync(localUpload)) {
            try {
              const fileRes = await fetch(`${baseUrl}/uploads/${encodeURIComponent(f.fileName)}`, { signal: AbortSignal.timeout(15000) });
              if (fileRes.ok) {
                const buf = Buffer.from(await fileRes.arrayBuffer());
                fs.writeFileSync(localUpload, buf);
                console.log(`[Auto-Sync] Downloaded upload: ${f.fileName}`);
              }
            } catch (err) {}
          }
        }

        const pages = f.pages || (f.sheetImage ? [f.sheetImage] : []);
        for (const p of pages) {
          const fn = path.basename(p);
          const localSheet = path.join(SHEETS_DIR, fn);
          if (!fs.existsSync(localSheet)) {
            try {
              const sheetRes = await fetch(`${baseUrl}/sheets/${encodeURIComponent(fn)}`, { signal: AbortSignal.timeout(15000) });
              if (sheetRes.ok) {
                const buf = Buffer.from(await sheetRes.arrayBuffer());
                fs.writeFileSync(localSheet, buf);
                console.log(`[Auto-Sync] Downloaded sheet: ${fn}`);
              }
            } catch (err) {}
          }
        }
      }

      // Sync categories & centers
      try {
        const catRes = await fetch(`${baseUrl}/data/categories.json?t=${Date.now()}`, { signal: AbortSignal.timeout(5000) });
        if (catRes.ok) {
          const cats = await catRes.json();
          if (Array.isArray(cats) && cats.length > 0) writeJson('categories.json', cats);
        }
      } catch (e) {}

      try {
        const centersRes = await fetch(`${baseUrl}/data/centers.json?t=${Date.now()}`, { signal: AbortSignal.timeout(5000) });
        if (centersRes.ok) {
          const centers = await centersRes.json();
          if (Array.isArray(centers) && centers.length > 0) writeJson('centers.json', centers);
        }
      } catch (e) {}

      if (hasDifferences) {
        writeJson('forms.json', cloudForms);
        console.log(`[Auto-Sync] Successfully updated forms database (${cloudForms.length} forms) from ${baseUrl}!`);
      }

      return { success: true, updated: hasDifferences, count: cloudForms.length, source: baseUrl };
    } catch (e) {
      console.log(`[Auto-Sync] Cloud source ${baseUrl} skipped: ${e.message}`);
    }
  }
  return { success: false, reason: 'Offline or cloud sources unreachable' };
}

// Endpoint to trigger manual or on-load sync
app.post('/api/admin/auto-sync', async (req, res) => {
  const result = await autoSyncWithCloud();
  res.json(result);
});

app.listen(PORT, '0.0.0.0', () => {
  // Auto-sync on startup in background
  setTimeout(() => { autoSyncWithCloud().catch(() => {}); }, 3000);
  console.log(`====================================================`);
  console.log(`نظام نماذج مكتب انتخابات محافظة البصرة ومراكز التسجيل الـ 76`);
  console.log(`- بوابة مراكز التسجيل (المركز): http://localhost:${PORT}/center`);
  console.log(`- لوحة تحكم مكتب المحافظة (المسؤول): http://localhost:${PORT}/admin`);
  console.log(`====================================================`);
});
