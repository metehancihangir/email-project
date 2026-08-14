import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import api from '../api/axiosInstance';

export default function UnsubscribePage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const navigate = useNavigate();
  const [status, setStatus] = useState('loading'); // loading, confirm, success, error
  const [subscriberEmail, setSubscriberEmail] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);

  useEffect(() => {
    if (!token) {
      setStatus('error');
      return;
    }

    // Salt-okunur token kontrolü (GET isteği aboneliği silmez, botların yanlışlıkla silmesini engeller)
    api.get(`/api/subscribers/unsubscribe/${token}`)
      .then((res) => {
        setSubscriberEmail(res.data.email || '');
        setStatus('confirm');
      })
      .catch(() => setStatus('error'));
  }, [token]);

  const handleConfirmUnsubscribe = async () => {
    setIsProcessing(true);
    try {
      await api.post(`/api/subscribers/unsubscribe/${token}`);
      setStatus('success');
    } catch {
      setStatus('error');
    } finally {
      setIsProcessing(false);
    }
  };

  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex items-center justify-center p-4">
      <AnimatePresence mode="wait">
        {status === 'loading' && (
          <motion.div
            key="loading"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="flex flex-col items-center"
          >
            <div className="w-12 h-12 border-4 border-gray-300 border-t-primary rounded-full animate-spin"></div>
            <p className="mt-4 text-text-muted">Abonelik bilgileri doğrulanıyor...</p>
          </motion.div>
        )}

        {status === 'confirm' && (
          <motion.div
            key="confirm"
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            className="bg-surface p-8 rounded-2xl shadow-xl max-w-md w-full text-center"
          >
            <div className="w-16 h-16 bg-amber-100 text-amber-600 rounded-full flex items-center justify-center mx-auto mb-6">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-text mb-2">Aboneliği İptal Et</h2>
            <p className="text-text-muted mb-6">
              {subscriberEmail ? (
                <>
                  <strong className="text-text">{subscriberEmail}</strong> adresiniz bülten listemizden çıkarılacaktır. Devam etmek istiyor musunuz?
                </>
              ) : (
                'Bülten listemizden ayrılmak istediğinize emin misiniz?'
              )}
            </p>
            <div className="space-y-3">
              <button
                onClick={handleConfirmUnsubscribe}
                disabled={isProcessing}
                className="w-full bg-red-600 hover:bg-red-700 disabled:opacity-50 text-white font-medium py-3 px-4 rounded-xl transition-colors flex justify-center items-center shadow-md shadow-red-600/20"
              >
                {isProcessing ? (
                  <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                ) : (
                  'Evet, Abonelikten Ayrıl'
                )}
              </button>
              <button
                onClick={() => navigate('/')}
                disabled={isProcessing}
                className="w-full bg-gray-100 hover:bg-gray-200 text-text font-medium py-3 px-4 rounded-xl transition-colors"
              >
                Vazgeç / Ana Sayfaya Dön
              </button>
            </div>
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
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7" />
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
