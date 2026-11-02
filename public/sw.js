// Basra Forms System V2 - PWA Service Worker (Offline-First Engine)
const CACHE_NAME = 'basra-pwa-v2-cache-v1';

const STATIC_PRECACHE = [
  '/',
  '/index.html',
  '/admin.html',
  '/center.html',
  '/runtime_config.js',
  '/manifest-centers.json',
  '/manifest-admin.json',
  '/icon-192.png',
  '/icon-512.png',
  '/sheets/form_09_page_1.png',
  '/sheets/form_1792783929926_page_1.png',
  '/sheets/form_1792790662356_page_1.png',
  '/sheets/form_1792961892317_page_1.png'
];

self.addEventListener('install', (event) => {
  self.skipWaiting();
  event.waitUntil(
    caches.open(CACHE_NAME).then(async (cache) => {
      console.log('[SW] Pre-caching static assets...');
      for (const url of STATIC_PRECACHE) {
        try {
          const res = await fetch(url, { cache: 'reload' });
          if (res && res.status === 200) {
            await cache.put(url, res);
          }
        } catch (e) {
          console.warn('[SW] Could not pre-cache:', url, e.message);
        }
      }
    })
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys.map((key) => {
          if (key !== CACHE_NAME) {
            console.log('[SW] Deleting old cache:', key);
            return caches.delete(key);
          }
        })
      );
    }).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const req = event.request;
  if (req.method !== 'GET') return;

  const url = new URL(req.url);

  // 1. Supabase REST API Queries: Network-First with Cache Fallback
  if (url.hostname.includes('supabase.co') && url.pathname.includes('/rest/v1/')) {
    event.respondWith(
      fetch(req).then((response) => {
        if (response && response.status === 200) {
          const clone = response.clone();
          caches.open(CACHE_NAME).then((cache) => cache.put(req, clone));
        }
        return response;
      }).catch(async () => {
        const cached = await caches.match(req);
        if (cached) return cached;
        return new Response(JSON.stringify({ data: [], error: null }), {
          headers: { 'Content-Type': 'application/json' }
        });
      })
    );
    return;
  }

  // 2. Supabase Storage / Files / Sheets: Cache-First with Network Fetch & Cache
  if (url.hostname.includes('supabase.co') || url.pathname.includes('/sheets/') || url.pathname.includes('/uploads/')) {
    event.respondWith(
      caches.match(req).then((cached) => {
        if (cached) return cached;
        return fetch(req).then((netRes) => {
          if (netRes && netRes.status === 200) {
            const clone = netRes.clone();
            caches.open(CACHE_NAME).then((c) => c.put(req, clone));
          }
          return netRes;
        }).catch(() => {
          return new Response('File unavailable offline', { status: 503, statusText: 'Offline' });
        });
      })
    );
    return;
  }

  // 3. Static Assets, CDN Scripts, Fonts, Icons: Cache-First with Background Revalidation
  event.respondWith(
    caches.match(req).then((cached) => {
      const fetchPromise = fetch(req).then((networkRes) => {
        if (networkRes && networkRes.status === 200) {
          const clone = networkRes.clone();
          caches.open(CACHE_NAME).then((cache) => cache.put(req, clone));
        }
        return networkRes;
      }).catch(() => {
        if (req.mode === 'navigate') {
          if (url.pathname.includes('admin')) {
            return caches.match('/admin.html');
          }
          return caches.match('/') || caches.match('/index.html');
        }
        return null;
      });

      if (cached) {
        // Return cached immediately, refresh in background
        fetchPromise.catch(() => {});
        return cached;
      }
      return fetchPromise;
    })
  );
});

// Message listener for caching dynamic URLs sent from client
self.addEventListener('message', (event) => {
  if (event.data && event.data.action === 'cache-urls' && Array.isArray(event.data.urls)) {
    caches.open(CACHE_NAME).then((cache) => {
      event.data.urls.forEach(async (u) => {
        try {
          if (!u) return;
          const match = await cache.match(u);
          if (!match) {
            const res = await fetch(u, { mode: 'cors' });
            if (res && res.status === 200) {
              await cache.put(u, res);
            }
          }
        } catch(e) {}
      });
    });
  }
});