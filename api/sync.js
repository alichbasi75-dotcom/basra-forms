// api/sync.js - Vercel Serverless Function for Secure Publishing
// This function acts as a secure cloud relay between the Office Admin PC and GitHub

export default async function handler(req, res) {
  // Enable CORS so the desktop app can call this endpoint
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, x-admin-key');

  if (req.method === 'OPTIONS') {
    return res.status(200).end();
  }

  if (req.method === 'GET') {
    return res.status(200).json({
      status: 'active',
      service: 'Basra Registration Centers Cloud Sync Relay',
      version: '1.0.0'
    });
  }

  if (req.method !== 'POST') {
    return res.status(405).json({ error: 'Method not allowed' });
  }

  const adminSecret = process.env.ADMIN_SECRET || 'BasraForms@2026';
  const ghToken = process.env.GH_TOKEN;
  const owner = process.env.GH_OWNER || 'alichbasi75-dotcom';
  const repo = process.env.GH_REPO || 'basra-forms';

  // 1. Verify Secret Key
  const clientKey = req.headers['x-admin-key'] || (req.headers.authorization ? req.headers.authorization.replace('Bearer ', '') : '');
  if (!clientKey || clientKey !== adminSecret) {
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
    const { forms, categories, files } = req.body || {};
    const updatedItems = [];

    // Helper to upload or update a file in GitHub repository
    async function uploadToGitHub(filePath, base64Content, commitMessage) {
      const url = `https://api.github.com/repos/${owner}/${repo}/contents/${filePath}`;
      
      // Check if file exists to get its SHA
      let sha = null;
      try {
        const getRes = await fetch(url, {
          headers: {
            'Authorization': `Bearer ${ghToken}`,
            'Accept': 'application/vnd.github+json',
            'User-Agent': 'Basra-Forms-Sync'
          }
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

      if (!putRes.ok) {
        const errData = await putRes.text();
        throw new Error(`Failed to upload ${filePath}: ${errData}`);
      }
      return true;
    }

    // 2. Upload forms.json if provided
    if (forms && Array.isArray(forms)) {
      const formsBase64 = Buffer.from(JSON.stringify(forms, null, 2), 'utf-8').toString('base64');
      await uploadToGitHub('data/forms.json', formsBase64, 'تحديث قاعدة بيانات النماذج من لوحة المسؤول');
      updatedItems.push('data/forms.json');
    }

    // 3. Upload categories.json if provided
    if (categories && Array.isArray(categories)) {
      const catBase64 = Buffer.from(JSON.stringify(categories, null, 2), 'utf-8').toString('base64');
      await uploadToGitHub('data/categories.json', catBase64, 'تحديث التصنيفات من لوحة المسؤول');
      updatedItems.push('data/categories.json');
    }

    // 4. Upload additional assets (PDFs, Word, Sheet images)
    if (files && Array.isArray(files)) {
      for (const f of files) {
        if (f.path && f.contentBase64) {
          await uploadToGitHub(f.path, f.contentBase64, `إضافة/تحديث ملف: ${f.path}`);
          updatedItems.push(f.path);
        }
      }
    }

    return res.status(200).json({
      success: true,
      message: 'تم نشر وتحديث المنظومة بنجاح لجميع المراكز أونلاين!',
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
}
