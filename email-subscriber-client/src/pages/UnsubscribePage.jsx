import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import api from '../api/axiosInstance';

export default function UnsubscribePage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const navigate = useNavigate();
  const [status, setStatus] = useState('loading'); // loading, success, error

  useEffect(() => {
    if (!token) {
      setStatus('error');
      return;
    }

    api.get(`/api/subscribers/unsubscribe/${token}`)
      .then(() => setStatus('success'))
      .catch(() => setStatus('error'));
  }, [token]);

  return (
    <div className="min-h-screen bg-surface-alt flex items-center justify-center p-4">
      <AnimatePresence mode="wait">
        {status === 'loading' && (
          <motion.div
            key="loading"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="flex flex-col items-center"
          >
            <div className="w-12 h-12 border-4 border-gray-300 border-t-gray-500 rounded-full animate-spin"></div>
            <p className="mt-4 text-text-muted">İşleminiz gerçekleştiriliyor...</p>
          </motion.div>
        )}

        {status === 'success' && (
          <motion.div
            key="success"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            className="bg-surface p-8 rounded-2xl shadow-xl max-w-md w-full text-center"
          >
            <div className="w-16 h-16 bg-gray-100 text-gray-500 rounded-full flex items-center justify-center mx-auto mb-6">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-text mb-2">Aboneliğiniz İptal Edildi</h2>
            <p className="text-text-muted mb-6">Listemizden başarıyla ayrıldınız. Umarız ileride tekrar görüşürüz.</p>
            <button
              onClick={() => navigate('/')}
              className="w-full bg-gray-100 hover:bg-gray-200 text-text font-medium py-3 px-4 rounded-xl transition-colors"
            >
              Ana Sayfaya Dön
            </button>
          </motion.div>
        )}

        {status === 'error' && (
          <motion.div
            key="error"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            className="bg-surface p-8 rounded-2xl shadow-xl max-w-md w-full text-center"
          >
             <div className="w-16 h-16 bg-red-100 text-red-500 rounded-full flex items-center justify-center mx-auto mb-6">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-text mb-2">Geçersiz Bağlantı</h2>
            <p className="text-text-muted mb-6">Bu bağlantı geçersiz veya daha önce kullanılmış olabilir.</p>
            <button
              onClick={() => navigate('/')}
              className="w-full bg-primary hover:bg-primary-dark text-white font-medium py-3 px-4 rounded-xl transition-colors"
            >
              Ana Sayfaya Dön
            </button>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
