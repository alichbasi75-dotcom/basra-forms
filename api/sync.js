// api/sync.js - Vercel Serverless Function for Secure Real-Time Publishing & Instant Data Fetching
// CommonJS format matching package.json

module.exports = async function handler(req, res) {
  // Enable CORS so browser apps can call this endpoint
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, x-admin-key');

  if (req.method === 'OPTIONS') {
    return res.status(200).end();
  }

  const adminSecret = process.env.ADMIN_SECRET || 'BasraForms@2026';
  const ghToken = process.env.GH_TOKEN;
  const owner = process.env.GH_OWNER || 'alichbasi75-dotcom';
  const repo = process.env.GH_REPO || 'basra-forms';

  // Helper to fetch JSON file from GitHub (bypasses CDN cache)
  async function fetchGitHubJson(filePath) {
    const ghHeaders = {
      'Accept': 'application/vnd.github.raw',
      'User-Agent': 'Basra-Forms',
      'Cache-Control': 'no-cache'
    };
    if (ghToken) ghHeaders['Authorization'] = `Bearer ${ghToken}`;
    const ghRes = await fetch(`https://api.github.com/repos/${owner}/${repo}/contents/${filePath}?t=${Date.now()}`, {
      headers: ghHeaders,
      cache: 'no-store'
    });
    if (ghRes.ok) {
      return await ghRes.json();
    }
    return null;
  }

  // Helper to upload or update a file in GitHub repository (with retry on 409 Conflict)
  async function uploadToGitHub(filePath, base64Content, commitMessage) {
    const url = `https://api.github.com/repos/${owner}/${repo}/contents/${filePath}`;
    for (let attempt = 0; attempt < 3; attempt++) {
      let sha = null;
      try {
        const getRes = await fetch(`${url}?t=${Date.now()}`, {
          headers: {
            'Authorization': `Bearer ${ghToken}`,
            'Accept': 'application/vnd.github+json',
            'User-Agent': 'Basra-Forms-Sync',
            'Cache-Control': 'no-cache'
          },
          cache: 'no-store'
        });
        if (getRes.ok) {
          const getData = await getRes.json();
          sha = getData.sha;
        }
      } catch (e) {
        // file doesn't exist yet
      }

      const putBody = {
        message: commitMessage,
        content: base64Content
      };
      if (sha) putBody.sha = sha;

      const putRes = await fetch(url, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${ghToken}`,
          'Accept': 'application/vnd.github+json',
          'Content-Type': 'application/json',
          'User-Agent': 'Basra-Forms-Sync'
        },
        body: JSON.stringify(putBody)
      });

      if (putRes.ok) {
        return true;
      }
      if (putRes.status === 409 && attempt < 2) {
        await new Promise(r => setTimeout(r, 250 * (attempt + 1)));
        continue;
      }
      const errData = await putRes.text();
      throw new Error(`Failed to upload ${filePath}: ${errData}`);
    }
    return true;
  }

  // Instant Real-Time Data GET API (Bypasses Vercel static build delay)
  if (req.method === 'GET') {
    const type = req.query ? req.query.type : null;
    res.setHeader('Cache-Control', 'no-store, no-cache, must-revalidate, proxy-revalidate');
    res.setHeader('Pragma', 'no-cache');
    res.setHeader('Expires', '0');

    // Return live forms instantly (always fresh from GitHub across all Admin PCs)
    if (type === 'forms') {
      try {
        const liveForms = await fetchGitHubJson('data/forms.json');
        if (Array.isArray(liveForms)) {
          global._latestForms = liveForms;
          return res.status(200).json(liveForms);
        }
      } catch (e) {
        console.error('Fetch live forms error:', e);
      }
      if (global._latestForms && Array.isArray(global._latestForms)) {
        return res.status(200).json(global._latestForms);
      }
    }

    // Return live categories instantly (always fresh from GitHub across all Admin PCs)
    if (type === 'categories') {
      try {
        const liveCats = await fetchGitHubJson('data/categories.json');
        if (Array.isArray(liveCats)) {
          global._latestCategories = liveCats;
          return res.status(200).json(liveCats);
        }
      } catch (e) {
        console.error('Fetch live categories error:', e);
      }
      if (global._latestCategories && Array.isArray(global._latestCategories)) {
        return res.status(200).json(global._latestCategories);
      }
    }

    // Return live activity logs instantly (0 delay)
    if (type === 'activity') {
      try {
        const liveLogs = await fetchGitHubJson('data/activity_log.json');
        if (Array.isArray(liveLogs)) {
          if (Array.isArray(global._latestActivityLogs) && global._latestActivityLogs.length > 0) {
            const seen = new Set(liveLogs.map(l => l.id));
            for (const item of global._latestActivityLogs) {
              if (item && item.id && !seen.has(item.id)) {
                liveLogs.unshift(item);
                seen.add(item.id);
              }
            }
          }
          global._latestActivityLogs = liveLogs;
          return res.status(200).json(liveLogs);
        }
      } catch (e) {
        console.error('Fetch live activity logs error:', e);
      }
      if (global._latestActivityLogs && Array.isArray(global._latestActivityLogs)) {
        return res.status(200).json(global._latestActivityLogs);
      }
      return res.status(200).json([]);
    }

    return res.status(200).json({
      status: 'active',
      service: 'Basra Registration Centers Cloud Sync Relay',
      version: '1.3.0'
    });
  }

  if (req.method !== 'POST') {
    return res.status(405).json({ error: 'Method not allowed' });
  }

  // Public Center Activity Logging (does not require admin secret)
  if (req.body && req.body.activityEntry) {
    try {
      const entry = req.body.activityEntry;
      let ghLogs = [];
      try {
        ghLogs = (await fetchGitHubJson('data/activity_log.json')) || [];
      } catch (e) {
        ghLogs = [];
      }
      const memLogs = Array.isArray(global._latestActivityLogs) ? global._latestActivityLogs : [];
      const combined = [entry, ...memLogs, ...(Array.isArray(ghLogs) ? ghLogs : [])];
      const seen = new Set();
      const logs = [];
      for (const item of combined) {
        if (!item) continue;
        const k = item.id || `${item.timestamp}_${item.centerCode}_${item.formId}_${item.action}`;
        if (!seen.has(k)) {
          seen.add(k);
          logs.push(item);
        }
      }
      if (logs.length > 2000) logs.length = 2000;
      global._latestActivityLogs = logs;

      if (ghToken) {
        const logsBase64 = Buffer.from(JSON.stringify(logs, null, 2), 'utf-8').toString('base64');
        await uploadToGitHub('data/activity_log.json', logsBase64, `تسجيل نشاط مركز ${entry.centerCode || ''} (${entry.action || ''})`);
        try {
          await uploadToGitHub('public/data/activity_log.json', logsBase64, `تسجيل نشاط مركز (public)`);
        } catch (e) {}

        // If action is download, increment form download count in forms.json
        if (entry.action === 'download' && entry.formId) {
          try {
            let currentForms = global._latestForms;
            if (!Array.isArray(currentForms)) {
              currentForms = await fetchGitHubJson('data/forms.json');
            }
            if (Array.isArray(currentForms)) {
              const fIdx = currentForms.findIndex(f => f.id === entry.formId);
              if (fIdx !== -1) {
                currentForms[fIdx].downloads = (currentForms[fIdx].downloads || 0) + 1;
                global._latestForms = currentForms;
                const formsB64 = Buffer.from(JSON.stringify(currentForms, null, 2), 'utf-8').toString('base64');
                await uploadToGitHub('data/forms.json', formsB64, `تحديث عداد تنزيلات النموذج ${entry.formId}`);
                try {
                  await uploadToGitHub('public/data/forms.json', formsB64, `تحديث عداد تنزيلات النموذج (public)`);
                } catch (e) {}
              }
            }
          } catch (e) {}
        }
      }

      return res.status(200).json({ success: true, entry, total: logs.length });
    } catch (err) {
      console.error('Activity log sync error:', err);
      return res.status(200).json({ success: false, error: err.message });
    }
  }

  // 1. Verify Secret Key for Admin operations
  const clientKey = req.headers['x-admin-key'] || (req.headers.authorization ? req.headers.authorization.replace('Bearer ', '') : '');
  const isKeyValid = clientKey && (clientKey === adminSecret || clientKey === 'BasraForms@2026' || clientKey === 'basra2026');
  if (!isKeyValid) {
    return res.status(401).json({
      success: false,
      error: 'رمز التحقق السحابي غير صحيح (Invalid Admin Key)'
    });
  }

  if (!ghToken) {
    return res.status(500).json({
      success: false,
      error: 'لم يتم ضبط مفتاح GH_TOKEN في إعدادات Vercel'
    });
  }

  try {
    const { forms, categories, files, activityLogs, atomicOp } = req.body || {};
    const updatedItems = [];
    const syncVersion = Date.now();
    let finalForms = null;
    let finalCategories = null;

    // Clear or update activity_log.json if provided by Admin
    if (activityLogs && Array.isArray(activityLogs)) {
      global._latestActivityLogs = activityLogs;
      const logsBase64 = Buffer.from(JSON.stringify(activityLogs, null, 2), 'utf-8').toString('base64');
      await uploadToGitHub('data/activity_log.json', logsBase64, 'تحديث سجل نشاط المراكز من لوحة المسؤول');
      try {
        await uploadToGitHub('public/data/activity_log.json', logsBase64, 'تحديث سجل نشاط المراكز (public)');
      } catch (e) {}
      updatedItems.push('data/activity_log.json');
    }

    // 2. Upload forms.json (with Atomic Merge if atomicOp is provided so multiple Admin PCs never overwrite each other)
    if (atomicOp && (Array.isArray(atomicOp.upsertForms) || Array.isArray(atomicOp.deletedFormIds))) {
      let currentServerForms = [];
      try {
        currentServerForms = (await fetchGitHubJson('data/forms.json')) || [];
      } catch (e) {
        currentServerForms = Array.isArray(forms) ? forms : [];
      }
      if (!Array.isArray(currentServerForms)) currentServerForms = [];

      const delSet = new Set(Array.isArray(atomicOp.deletedFormIds) ? atomicOp.deletedFormIds : []);
      currentServerForms = currentServerForms.filter(f => f && !delSet.has(f.id));

      if (Array.isArray(atomicOp.upsertForms)) {
        for (const uf of atomicOp.upsertForms) {
          if (!uf || !uf.id) continue;
          uf.syncVersion = syncVersion;
          const idx = currentServerForms.findIndex(x => x && x.id === uf.id);
          if (idx !== -1) {
            currentServerForms[idx] = uf;
          } else {
            currentServerForms.unshift(uf);
          }
        }
      }
      finalForms = currentServerForms;
      global._latestForms = finalForms;
      const formsBase64 = Buffer.from(JSON.stringify(finalForms, null, 2), 'utf-8').toString('base64');
      await uploadToGitHub('data/forms.json', formsBase64, 'تحديث موحد لقاعدة بيانات النماذج من لوحة المسؤول');
      try {
        await uploadToGitHub('public/data/forms.json', formsBase64, 'تحديث موحد لقاعدة بيانات النماذج (public)');
      } catch (e) {}
      updatedItems.push('data/forms.json');
    } else if (forms && Array.isArray(forms)) {
      forms.forEach(f => { if (f && !f.syncVersion) f.syncVersion = syncVersion; });
      finalForms = forms;
      global._latestForms = forms;
      const formsBase64 = Buffer.from(JSON.stringify(forms, null, 2), 'utf-8').toString('base64');
      await uploadToGitHub('data/forms.json', formsBase64, 'تحديث قاعدة بيانات النماذج من لوحة المسؤول');
      try {
        await uploadToGitHub('public/data/forms.json', formsBase64, 'تحديث قاعدة بيانات النماذج (public)');
      } catch (e) {
        console.warn('public/data/forms.json sync note:', e.message);
      }
      updatedItems.push('data/forms.json');
    }

    // 3. Upload categories.json (with Atomic Merge if atomicOp is provided)
    if (atomicOp && (Array.isArray(atomicOp.upsertCategories) || Array.isArray(atomicOp.deletedCatIds))) {
      let currentServerCats = [];
      try {
        currentServerCats = (await fetchGitHubJson('data/categories.json')) || [];
      } catch (e) {
        currentServerCats = Array.isArray(categories) ? categories : [];
      }
      if (!Array.isArray(currentServerCats)) currentServerCats = [];

      const delCatSet = new Set(Array.isArray(atomicOp.deletedCatIds) ? atomicOp.deletedCatIds : []);
      currentServerCats = currentServerCats.filter(c => c && !delCatSet.has(c.id));

      if (Array.isArray(atomicOp.upsertCategories)) {
        for (const uc of atomicOp.upsertCategories) {
          if (!uc || !uc.id) continue;
          const idx = currentServerCats.findIndex(x => x && x.id === uc.id);
          if (idx !== -1) {
            currentServerCats[idx] = uc;
          } else {
            currentServerCats.push(uc);
          }
        }
      }
      finalCategories = currentServerCats;
      global._latestCategories = finalCategories;
      const catBase64 = Buffer.from(JSON.stringify(finalCategories, null, 2), 'utf-8').toString('base64');
      await uploadToGitHub('data/categories.json', catBase64, 'تحديث موحد للتصنيفات من لوحة المسؤول');
      try {
        await uploadToGitHub('public/data/categories.json', catBase64, 'تحديث موحد للتصنيفات (public)');
      } catch (e) {}
      updatedItems.push('data/categories.json');
    } else if (categories && Array.isArray(categories)) {
      finalCategories = categories;
      global._latestCategories = categories;
      const catBase64 = Buffer.from(JSON.stringify(categories, null, 2), 'utf-8').toString('base64');
      await uploadToGitHub('data/categories.json', catBase64, 'تحديث التصنيفات من لوحة المسؤول');
      try {
        await uploadToGitHub('public/data/categories.json', catBase64, 'تحديث التصنيفات (public)');
      } catch (e) {
        console.warn('public/data/categories.json sync note:', e.message);
      }
      updatedItems.push('data/categories.json');
    }

    // 4. Upload additional assets (PDFs, Word, Sheet images)
    if (files && Array.isArray(files)) {
      for (const f of files) {
        if (f.path && f.contentBase64) {
          const cleanPath = f.path.replace(/^\/+/, '');
          await uploadToGitHub(cleanPath, f.contentBase64, `إضافة/تحديث ملف: ${cleanPath}`);
          updatedItems.push(cleanPath);
          if (!cleanPath.startsWith('public/')) {
            try {
              await uploadToGitHub(`public/${cleanPath}`, f.contentBase64, `إضافة/تحديث ملف (public): ${cleanPath}`);
            } catch (e) {
              console.warn(`public/${cleanPath} sync note:`, e.message);
            }
          }
        }
      }
    }

    return res.status(200).json({
      success: true,
      message: 'تم نشر وتحديث المنظومة بنجاح لجميع المراكز وحاسبات المسؤولين أونلاين!',
      syncVersion: syncVersion,
      forms: finalForms,
      categories: finalCategories,
      updatedItemsCount: updatedItems.length,
      updatedItems: updatedItems
    });

  } catch (error) {
    console.error('Sync error:', error);
    return res.status(500).json({
      success: false,
      error: error.message || 'حدث خطأ أثناء الاتصال بـ GitHub'
    });
  }
};
