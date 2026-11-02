// Basra Forms System V2 - PWA Service Worker (Offline-First Engine V2)
const CACHE_NAME = 'basra-pwa-v2-cache-v2';

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
      console.log('[SW] Pre-caching static assets into', CACHE_NAME);
      for (const url of STATIC_PRECACHE) {
        try {
          const res = await fetch(url);
          if (res && res.ok) {
            await cache.put(url, res);
          }
        } catch (e) {
          console.warn('[SW] Pre-cache warning for:', url, e);
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
            console.log('[SW] Removing old cache:', key);
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

  // 1. Navigation requests (HTML pages)
  if (req.mode === 'navigate') {
    event.respondWith(
      fetch(req).then((response) => {
        if (response && response.ok) {
          const clone = response.clone();
          caches.open(CACHE_NAME).then((c) => c.put(req, clone));
        }
        return response;
      }).catch(async () => {
        const cached = await caches.match(req) || await caches.match(url.pathname);
        if (cached) return cached;
        if (url.pathname.includes('admin')) {
          return caches.match('/admin.html');
        }
        return caches.match('/') || caches.match('/index.html');
      })
    );
    return;
  }

  // 2. Sheets and static images: Cache-First ALWAYS
  if (url.pathname.includes('/sheets/') || url.pathname.includes('/uploads/')) {
    event.respondWith(
      caches.match(req, { ignoreSearch: true }).then((cached) => {
        if (cached) return cached;
        const fileName = url.pathname.split('/').pop();
        return caches.match('/sheets/' + fileName).then((altCached) => {
          if (altCached) return altCached;
          return fetch(req).then((netRes) => {
            if (netRes && netRes.ok) {
              const clone = netRes.clone();
              caches.open(CACHE_NAME).then((c) => c.put(req, clone));
            }
            return netRes;
          }).catch(() => {
            return new Response('Offline', { status: 503 });
          });
        });
      })
    );
    return;
  }

  // 3. Supabase REST API Queries: Network-First with Cache Fallback
  if (url.hostname.includes('supabase.co') && url.pathname.includes('/rest/v1/')) {
    event.respondWith(
      fetch(new Request(req, { cache: 'no-cache' })).then((response) => {
        if (response && response.ok) {
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

  // 4. Everything else: Cache-First with Network Fallback
  event.respondWith(
    caches.match(req, { ignoreSearch: true }).then((cached) => {
      if (cached) return cached;
      return fetch(req).then((netRes) => {
        if (netRes && netRes.ok) {
          const clone = netRes.clone();
          caches.open(CACHE_NAME).then((c) => c.put(req, clone));
        }
        return netRes;
      }).catch(() => null);
    })
  );
});

self.addEventListener('message', (event) => {
  if (event.data && event.data.action === 'cache-urls' && Array.isArray(event.data.urls)) {
    caches.open(CACHE_NAME).then((cache) => {
      event.data.urls.forEach(async (u) => {
        try {
          if (!u) return;
          const match = await cache.match(u);
          if (!match) {
            const res = await fetch(u, { mode: 'cors' });
            if (res && res.ok) cache.put(u, res);
          }
        } catch(e) {}
      });
    });
  }
});