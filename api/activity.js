// api/activity.js - Vercel Serverless Function for Real-Time Center Activity & Download Tracking
// CommonJS format matching package.json

const syncHandler = require('./sync.js');

module.exports = async function handler(req, res) {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, x-admin-key');

  if (req.method === 'OPTIONS') {
    return res.status(200).end();
  }

  if (req.method === 'GET') {
    req.query = Object.assign({}, req.query, { type: 'activity' });
    return syncHandler(req, res);
  }

  if (req.method === 'POST') {
    const body = req.body || {};
    if (body.activityEntry) {
      return syncHandler(req, res);
    }

    const now = new Date();
    let formattedTime = '';
    try {
      formattedTime = new Intl.DateTimeFormat('ar-IQ', {
        timeZone: 'Asia/Baghdad',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: true
      }).format(now);
    } catch (e) {
      formattedTime = now.toISOString();
    }

    const entry = {
      id: body.id || ('act_' + Date.now() + '_' + Math.round(Math.random() * 1000)),
      timestamp: body.timestamp || now.toISOString(),
      formattedTime: body.formattedTime || formattedTime,
      centerCode: body.centerCode || 'غير محدد',
      centerName: body.centerName || 'غير محدد',
      formId: body.formId || '',
      formTitle: body.formTitle || '',
      action: body.action || 'view'
    };

    req.body = { activityEntry: entry };
    return syncHandler(req, res);
  }

  return res.status(405).json({ error: 'Method not allowed' });
};
