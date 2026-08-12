import { useState, useEffect } from 'react';
import axios from 'axios';
import { isValidEmail } from '../utils/validation';
import Toast from './Toast';

export default function SubscribeForm() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState({ message: '', type: '' });
  
  const [submittedEmail, setSubmittedEmail] = useState(null);
  const [countdown, setCountdown] = useState(0);

  const closeToast = () => setToast({ message: '', type: '' });

  useEffect(() => {
    let timer;
    if (countdown > 0) {
      timer = setInterval(() => setCountdown(c => c - 1), 1000);
    }
    return () => clearInterval(timer);
  }, [countdown]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!isValidEmail(email)) {
      setToast({ type: 'error', message: 'Geçersiz e-posta adresi.' });
      return;
    }

    setLoading(true);

    try {
      const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
      
      const response = await axios.post(`${apiUrl}/api/subscribers`, {
        email,
        name,
        website: '' // Honeypot boş olmalı
      });

      if (response.status === 202) {
        setToast({ type: 'success', message: 'Abonelik başarılı! Onay e-postanız gönderildi.' });
        setSubmittedEmail(email);
        setCountdown(120);
      }
    } catch (err) {
      if (err.response) {
        if (err.response.status === 409) {
          setToast({ type: 'warning', message: 'Bu e-posta adresi zaten kayıtlı.' });
        } else if (err.response.status === 429) {
          setToast({ type: 'error', message: 'Çok fazla istek attınız. Lütfen biraz bekleyin.' });
        } else {
          setToast({ type: 'error', message: err.response.data?.message || 'Bir hata oluştu.' });
        }
      } else {
        setToast({ type: 'error', message: 'Sunucuya ulaşılamıyor.' });
      }
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    if (countdown > 0) return;
    setLoading(true);
    try {
      const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
      await axios.post(`${apiUrl}/api/subscribers/resend-confirmation`, { email: submittedEmail });
      setToast({ type: 'success', message: 'Yeni onay e-postası gönderildi!' });
      setCountdown(120);
    } catch (err) {
      if (err.response && err.response.status === 429) {
        setToast({ type: 'error', message: 'Lütfen yeni kod talep etmeden önce bekleyin.' });
      } else {
        setToast({ type: 'error', message: err.response?.data?.message || 'Bir hata oluştu.' });
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="w-full max-w-md mx-auto relative">
      <Toast type={toast.type} message={toast.message} onClose={closeToast} />
      
      <form onSubmit={handleSubmit} className="bg-surface-alt p-6 md:p-8 rounded-xl shadow-lg border border-gray-100">
        {submittedEmail ? (
          <div className="text-center">
             <div className="w-16 h-16 bg-primary/10 text-primary rounded-full flex items-center justify-center mx-auto mb-4">
               <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
             </div>
             <h3 className="text-xl font-medium text-text mb-2">E-posta Gönderildi</h3>
             <p className="text-text-muted mb-6">Onay bağlantısı <strong>{submittedEmail}</strong> adresine gönderildi. Lütfen gelen kutunuzu kontrol edin.</p>
             <div className="mt-4 pt-4 border-t border-gray-100">
               <p className="text-sm text-text-muted mb-2">Kod gelmedi mi?</p>
               <button
                 type="button"
                 onClick={handleResend}
                 disabled={countdown > 0 || loading}
                 className="text-primary hover:text-primary-dark font-medium disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
               >
                 {countdown > 0 ? `Tekrar denemek için ${countdown} saniye bekleyin` : 'Tekrar gönder'}
               </button>
             </div>
             <button
                type="button"
                onClick={() => { setSubmittedEmail(null); setEmail(''); setName(''); }}
                className="mt-6 text-sm text-gray-500 hover:text-gray-700 underline"
             >
                Farklı bir e-posta adresi kullan
             </button>
           </div>
        ) : (
          <>
            <h2 className="text-2xl font-semibold mb-6 text-text">Bültenimize Katılın</h2>
            
            {/* Honeypot Field */}
            <input 
              type="text" 
              name="website" 
              style={{ display: 'none' }} 
              tabIndex={-1} 
              aria-hidden="true"
            />

            <div className="mb-4">
              <label htmlFor="nameInput" className="block text-sm font-medium text-text-muted mb-1">
                İsim (Opsiyonel)
              </label>
              <input
                id="nameInput"
                type="text"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition-all text-text bg-surface"
                placeholder="Adınız"
                value={name}
                onChange={(e) => setName(e.target.value)}
                disabled={loading}
              />
            </div>

            <div className="mb-6">
              <label htmlFor="emailInput" className="block text-sm font-medium text-text-muted mb-1">
                E-posta Adresi <span className="text-red-500">*</span>
              </label>
              <input
                id="emailInput"
                type="email"
                required
                aria-required="true"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary transition-all text-text bg-surface"
                placeholder="ornek@email.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={loading}
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className={`w-full py-3 rounded-lg font-medium text-white transition-all flex justify-center items-center ${
                loading ? 'bg-primary-dark opacity-70 cursor-not-allowed' : 'bg-primary hover:bg-primary-dark shadow-md hover:shadow-lg'
              }`}
            >
              {loading ? (
                <svg className="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              ) : (
                'Abone Ol'
              )}
            </button>
          </>
        )}
      </form>
    </div>
  );
}
