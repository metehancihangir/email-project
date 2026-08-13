import { useState, useEffect } from 'react';
import axios from 'axios';
import { isValidEmail } from '../utils/validation';
import Toast from './Toast';
import PrivacyModal from './PrivacyModal';

export default function SubscribeForm() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState({ message: '', type: '' });

  const closeToast = () => setToast({ message: '', type: '' });

  const [showResend, setShowResend] = useState(false);
  const [resendState, setResendState] = useState({ isDisabled: false, remainingSeconds: 0 });
  const [submittedEmail, setSubmittedEmail] = useState('');
  const [gdprAccepted, setGdprAccepted] = useState(false);
  const [isPrivacyModalOpen, setIsPrivacyModalOpen] = useState(false);

  useEffect(() => {
    if (!resendState.isDisabled) return;
    const interval = setInterval(() => {
      setResendState((prev) => {
        if (prev.remainingSeconds <= 1) {
          clearInterval(interval);
          return { isDisabled: false, remainingSeconds: 0 };
        }
        return { ...prev, remainingSeconds: prev.remainingSeconds - 1 };
      });
    }, 1000);
    return () => clearInterval(interval);
  }, [resendState.isDisabled]);
  
  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!isValidEmail(email)) {
      setToast({ type: 'error', message: 'Geçersiz e-posta adresi.' });
      return;
    }

    setLoading(true);
    setShowResend(false);

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
        setShowResend(true);
        // İsteğe bağlı olarak input'ları temizleyebiliriz
        setEmail('');
        setName('');
        // Resend butonunun cooldown'a girmesini istiyorsak:
        // setResendState({ isDisabled: true, remainingSeconds: 120 });
      }
    } catch (err) {
      if (err.response) {
        if (err.response.status === 409) {
          setToast({ type: 'warning', message: 'Bu e-posta adresi zaten kayıtlı.' });
        } else if (err.response.status === 429) {
          setToast({ type: 'error', message: 'Çok fazla istek attınız. Lütfen biraz bekleyin.' });
        } else {
          const msg = err.response.data?.message || 'Bir hata oluştu.';
          if (msg === 'Onay e-postası zaten gönderildi.') {
            setSubmittedEmail(email);
            setShowResend(true);
            setToast({ type: 'warning', message: 'Onay e-postası zaten gönderilmiş. İsterseniz tekrar gönderebilirsiniz.' });
          } else {
            setToast({ type: 'error', message: msg });
          }
        }
      } else {
        setToast({ type: 'error', message: 'Sunucuya ulaşılamıyor.' });
      }
    } finally {
      setLoading(false);
    }
  };

  const handleResend = async () => {
    const targetEmail = submittedEmail || email;
    if (!targetEmail || resendState.isDisabled) return;
    setLoading(true);
    try {
      const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
      const res = await axios.post(`${apiUrl}/api/subscribers/resend-confirmation-v2`, { email: targetEmail });
      setToast({ type: 'success', message: 'Yeni onay e-postası gönderildi!' });
      
      const nextAllowedAt = res.data?.nextAllowedAt;
      if (nextAllowedAt) {
        const remaining = Math.max(0, Math.ceil((new Date(nextAllowedAt) - Date.now()) / 1000));
        setResendState({ isDisabled: true, remainingSeconds: remaining });
      } else {
        setResendState({ isDisabled: true, remainingSeconds: 120 });
      }
    } catch (err) {
      if (err.response?.status === 429) {
        const retryAfter = err.response.data?.retryAfterSeconds ?? 120;
        setResendState({ isDisabled: true, remainingSeconds: retryAfter });
        setToast({ type: 'error', message: err.response.data?.message || 'Lütfen bekleyin.' });
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

        <div className="mb-6 flex items-start">
          <div className="flex items-center h-5 mt-1">
            <input
              id="gdpr"
              type="checkbox"
              required
              checked={gdprAccepted}
              onChange={(e) => setGdprAccepted(e.target.checked)}
              className="w-4 h-4 text-primary border-gray-300 rounded focus:ring-primary"
            />
          </div>
          <label htmlFor="gdpr" className="ml-2 text-xs font-medium text-text-muted">
            <button 
              type="button" 
              onClick={() => setIsPrivacyModalOpen(true)} 
              className="text-primary hover:underline focus:outline-none font-semibold"
            >
              Aydınlatma metnini
            </button>{' '} 
            okudum ve ticari elektronik ileti almayı kabul ediyorum.
          </label>
        </div>

        <button
          type="submit"
          disabled={loading || resendState.isDisabled}
          className={`w-full py-3 rounded-lg font-medium text-white transition-all flex justify-center items-center ${
            (loading || resendState.isDisabled) ? 'bg-primary-dark opacity-70 cursor-not-allowed' : 'bg-primary hover:bg-primary-dark shadow-md hover:shadow-lg'
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

        {showResend && (
          <div className="mt-4 text-center text-sm text-text-muted">
            <p>
              Kodunuz gelmedi mi?{' '}
              <button
                type="button"
                onClick={handleResend}
                disabled={loading || resendState.isDisabled}
                className="font-medium text-primary hover:text-primary-dark underline focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed disabled:no-underline"
              >
                {resendState.isDisabled
                  ? `Tekrar deneyin (${Math.floor(resendState.remainingSeconds / 60).toString().padStart(2, '0')}:${(resendState.remainingSeconds % 60).toString().padStart(2, '0')})`
                  : 'Tekrar deneyin'}
              </button>
            </p>
          </div>
        )}
      </form>

      <PrivacyModal 
        isOpen={isPrivacyModalOpen} 
        onClose={() => setIsPrivacyModalOpen(false)} 
        onAccept={() => {
          setGdprAccepted(true);
          setIsPrivacyModalOpen(false);
        }} 
      />
    </div>
  );
}
