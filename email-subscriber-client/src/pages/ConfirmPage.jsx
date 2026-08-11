import React, { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import api from '../api/axiosInstance';
import Toast from '../components/Toast';

export default function ConfirmPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  const navigate = useNavigate();

  const [status, setStatus] = useState('loading'); // loading, success, expired, error
  const [toast, setToast] = useState(null);
  const [email, setEmail] = useState('');
  const [isResending, setIsResending] = useState(false);

  useEffect(() => {
    if (!token) {
      setStatus('error');
      return;
    }

    api.get(`/api/subscribers/confirm/${token}`)
      .then(() => setStatus('success'))
      .catch((err) => {
        if (err.response?.status === 410) {
          setStatus('expired');
        } else {
          setStatus('error');
        }
      });
  }, [token]);

  const handleResend = async (e) => {
    e.preventDefault();
    if (!email) return;

    setIsResending(true);
    try {
      await api.post('/api/subscribers/resend-confirmation', { email });
      setToast({ type: 'success', message: 'Yeni onay e-postası gönderildi!' });
      setTimeout(() => navigate('/'), 3000);
    } catch (err) {
      setToast({ type: 'error', message: err.response?.data?.message || 'Bir hata oluştu.' });
    } finally {
      setIsResending(false);
    }
  };

  return (
    <div className="min-h-screen bg-surface-alt flex items-center justify-center p-4">
      {toast && <Toast type={toast.type} message={toast.message} onClose={() => setToast(null)} />}
      
      <AnimatePresence mode="wait">
        {status === 'loading' && (
          <motion.div
            key="loading"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="flex flex-col items-center"
          >
            <div className="w-12 h-12 border-4 border-primary/30 border-t-primary rounded-full animate-spin"></div>
            <p className="mt-4 text-text-muted">Onaylanıyor...</p>
          </motion.div>
        )}

        {status === 'success' && (
          <motion.div
            key="success"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            className="bg-surface p-8 rounded-2xl shadow-xl max-w-md w-full text-center"
          >
            <div className="w-16 h-16 bg-primary-light text-primary rounded-full flex items-center justify-center mx-auto mb-6">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-text mb-2">Aboneliğiniz Onaylandı!</h2>
            <p className="text-text-muted mb-6">E-bültenimize başarıyla katıldınız. Size özel içerikleri yakında e-posta kutunuzda göreceksiniz.</p>
            <button
              onClick={() => navigate('/')}
              className="w-full bg-primary hover:bg-primary-dark text-white font-medium py-3 px-4 rounded-xl transition-colors"
            >
              Ana Sayfaya Dön
            </button>
          </motion.div>
        )}

        {status === 'expired' && (
          <motion.div
            key="expired"
            initial={{ scale: 0.8, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            className="bg-surface p-8 rounded-2xl shadow-xl max-w-md w-full"
          >
            <div className="w-16 h-16 bg-red-100 text-red-500 rounded-full flex items-center justify-center mx-auto mb-6">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold text-text text-center mb-2">Bağlantı Süresi Dolmuş</h2>
            <p className="text-text-muted text-center mb-6">Güvenliğiniz için onay bağlantıları 24 saat geçerlidir. Yeni bir onay e-postası almak için e-posta adresinizi girin.</p>
            
            <form onSubmit={handleResend} className="space-y-4">
              <div>
                <input
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="E-posta adresiniz"
                  className="w-full px-4 py-3 rounded-xl border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none transition-all"
                />
              </div>
              <button
                type="submit"
                disabled={isResending}
                className="w-full bg-primary hover:bg-primary-dark disabled:opacity-50 text-white font-medium py-3 px-4 rounded-xl transition-colors flex justify-center"
              >
                {isResending ? (
                  <div className="w-6 h-6 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                ) : (
                  'Yeni Onay E-postası Gönder'
                )}
              </button>
            </form>
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
            <p className="text-text-muted mb-6">Bu onay bağlantısı geçersiz veya daha önce kullanılmış olabilir.</p>
            <button
              onClick={() => navigate('/')}
              className="w-full bg-gray-100 hover:bg-gray-200 text-text font-medium py-3 px-4 rounded-xl transition-colors"
            >
              Ana Sayfaya Dön
            </button>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
