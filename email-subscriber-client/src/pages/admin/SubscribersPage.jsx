import React, { useState, useEffect } from 'react';
import api from '../../api/axiosInstance';
import { motion, AnimatePresence } from 'framer-motion';

export default function SubscribersPage() {
  const [subscribers, setSubscribers] = useState([]);
  const [search, setSearch] = useState('');
  const [filters, setFilters] = useState({ isActive: '', isConfirmed: '' });
  const [loading, setLoading] = useState(true);

  // Dialog State
  const [confirmDialog, setConfirmDialog] = useState(null);

  const fetchSubscribers = async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      if (search) params.append('search', search);
      if (filters.isActive !== '') params.append('isActive', filters.isActive);
      if (filters.isConfirmed !== '') params.append('isConfirmed', filters.isConfirmed);

      const res = await api.get(`/api/admin/subscribers?${params.toString()}`);
      setSubscribers(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchSubscribers();
    }, 300); // Debounce
    return () => clearTimeout(timer);
  }, [search, filters]);

  const handleDelete = async (id) => {
    try {
      await api.delete(`/api/admin/subscribers/${id}`);
      setSubscribers((prev) => prev.filter((s) => s.id !== id));
    } catch (err) {
      console.error(err);
    }
  };

  const handleDeactivate = async (id) => {
    try {
      await api.put(`/api/admin/subscribers/${id}/deactivate`);
      fetchSubscribers();
    } catch (err) {
      console.error(err);
    }
  };

  const confirmAction = (action, subscriber) => {
    setConfirmDialog({ action, subscriber });
  };

  const executeAction = () => {
    if (confirmDialog.action === 'delete') handleDelete(confirmDialog.subscriber.id);
    if (confirmDialog.action === 'deactivate') handleDeactivate(confirmDialog.subscriber.id);
    setConfirmDialog(null);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <h1 className="text-2xl font-bold text-text">Aboneler</h1>
      </div>

      <div className="bg-surface p-4 rounded-2xl border border-gray-100 shadow-sm flex flex-col sm:flex-row gap-4">
        <div className="flex-1 relative">
          <input
            type="text"
            placeholder="İsim veya E-posta ara..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2 rounded-xl border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none transition-all"
          />
          <svg className="w-5 h-5 absolute left-3 top-2.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" /></svg>
        </div>
        <select
          value={filters.isActive}
          onChange={(e) => setFilters({ ...filters, isActive: e.target.value })}
          className="px-4 py-2 rounded-xl border border-gray-200 focus:border-primary outline-none bg-white text-gray-700"
        >
          <option value="">Tümü (Durum)</option>
          <option value="true">Aktif</option>
          <option value="false">Pasif</option>
        </select>
        <select
          value={filters.isConfirmed}
          onChange={(e) => setFilters({ ...filters, isConfirmed: e.target.value })}
          className="px-4 py-2 rounded-xl border border-gray-200 focus:border-primary outline-none bg-white text-gray-700"
        >
          <option value="">Tümü (Onay)</option>
          <option value="true">Onaylı</option>
          <option value="false">Onaysız</option>
        </select>
      </div>

      <div className="bg-surface rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-100 text-sm font-medium text-text-muted">
                <th className="p-4">E-posta</th>
                <th className="p-4">İsim</th>
                <th className="p-4">Onay</th>
                <th className="p-4">Durum</th>
                <th className="p-4">Kayıt Tarihi</th>
                <th className="p-4 text-right">İşlemler</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="6" className="p-8 text-center text-gray-400">Yükleniyor...</td>
                </tr>
              ) : subscribers.length === 0 ? (
                <tr>
                  <td colSpan="6" className="p-8 text-center text-gray-400">Kayıt bulunamadı.</td>
                </tr>
              ) : (
                <AnimatePresence>
                  {subscribers.map((sub, index) => (
                    <motion.tr
                      key={sub.id}
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      exit={{ opacity: 0, height: 0 }}
                      transition={{ delay: index * 0.05 }}
                      className="border-b border-gray-50 hover:bg-gray-50/50"
                    >
                      <td className="p-4 font-medium text-text">{sub.email}</td>
                      <td className="p-4 text-gray-600">{sub.name || '-'}</td>
                      <td className="p-4">
                        {sub.isConfirmed ? (
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-100 text-emerald-800">Onaylı</span>
                        ) : (
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-orange-100 text-orange-800">Bekliyor</span>
                        )}
                      </td>
                      <td className="p-4">
                         {sub.isActive ? (
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">Aktif</span>
                        ) : (
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">Pasif</span>
                        )}
                      </td>
                      <td className="p-4 text-gray-500 text-sm">
                        {new Date(sub.subscribedAt).toLocaleDateString('tr-TR')}
                      </td>
                      <td className="p-4 text-right space-x-2">
                        {sub.isActive && (
                          <button 
                            onClick={() => confirmAction('deactivate', sub)}
                            className="text-xs px-3 py-1 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-lg transition-colors"
                          >
                            Pasife Al
                          </button>
                        )}
                        <button 
                          onClick={() => confirmAction('delete', sub)}
                          className="text-xs px-3 py-1 bg-red-50 hover:bg-red-100 text-red-600 rounded-lg transition-colors"
                        >
                          Sil
                        </button>
                      </td>
                    </motion.tr>
                  ))}
                </AnimatePresence>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Confirm Dialog */}
      <AnimatePresence>
        {confirmDialog && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40">
            <motion.div
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              className="bg-surface p-6 rounded-2xl shadow-xl max-w-sm w-full"
            >
              <h3 className="text-lg font-bold text-text mb-2">Emin misiniz?</h3>
              <p className="text-text-muted mb-6">
                {confirmDialog.subscriber.email} kullanıcısını {confirmDialog.action === 'delete' ? 'silmek' : 'pasife almak'} istediğinize emin misiniz?
              </p>
              <div className="flex justify-end space-x-3">
                <button
                  onClick={() => setConfirmDialog(null)}
                  className="px-4 py-2 text-sm font-medium text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-xl transition-colors"
                >
                  İptal
                </button>
                <button
                  onClick={executeAction}
                  className={`px-4 py-2 text-sm font-medium text-white rounded-xl transition-colors ${
                    confirmDialog.action === 'delete' ? 'bg-red-500 hover:bg-red-600' : 'bg-primary hover:bg-primary-dark'
                  }`}
                >
                  Onayla
                </button>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>

    </div>
  );
}
