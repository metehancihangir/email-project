import { Outlet } from 'react-router-dom';

/**
 * Admin Layout — Faz 4'te tam olarak implemente edilecek.
 * Sol sidebar + üst bar + içerik alanı.
 */
export default function AdminLayout() {
  return (
    <div className="flex min-h-screen bg-surface-alt">
      {/* Sidebar — Faz 4 */}
      <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
        <div className="p-6 border-b border-gray-200">
          <h1 className="text-lg font-semibold text-primary">📬 EmailSubscriber</h1>
          <p className="text-xs text-text-muted mt-1">Admin Paneli</p>
        </div>
        <nav className="flex-1 p-4 space-y-1">
          {/* Faz 4'te NavLinks bileşeni eklenecek */}
          <p className="text-xs text-text-muted px-3 py-2">Navigasyon Faz 4'te eklenecek</p>
        </nav>
      </aside>

      {/* İçerik Alanı */}
      <main className="flex-1 overflow-auto">
        <Outlet />
      </main>
    </div>
  );
}
