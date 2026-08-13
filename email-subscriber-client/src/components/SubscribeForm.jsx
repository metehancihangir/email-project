import { useState, useEffect, useRef } from 'react';
import axios from 'axios';
import { isValidEmail } from '../utils/validation';
import Toast from './Toast';
import PrivacyModal from './PrivacyModal';



export default function SubscribeForm() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState({ message: '', type: '' });
  const categoryInputRef = useRef(null);

  const closeToast = () => setToast({ message: '', type: '' });

  const [showResend, setShowResend] = useState(false);
  const [resendState, setResendState] = useState({ isDisabled: false, remainingSeconds: 0 });
  const [submittedEmail, setSubmittedEmail] = useState('');
  const [gdprAccepted, setGdprAccepted] = useState(false);
  const [isPrivacyModalOpen, setIsPrivacyModalOpen] = useState(false);
  const [selectedCategories, setSelectedCategories] = useState([]);

  const categories = [
    { id: "Mitoloji", label: "Mitoloji 🏛️" },
    { id: "Bilim", label: "Bilim 🔬" },
    { id: "Finans", label: "Finans 💰" },
    { id: "Politika", label: "Politika 🌍" }
  ];
  
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  const handleCategoryChange = (cat) => {
    if (selectedCategories.includes(cat)) {
      setSelectedCategories(prev => prev.filter(c => c !== cat));
    } else {
      setSelectedCategories(prev => [...prev, cat]);
    }
  };

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
  
  useEffect(() => {
    if (categoryInputRef.current) {
      if (selectedCategories.length === 0) {
        categoryInputRef.current.setCustomValidity('Lütfen en az 1 konu seçin.');
      } else {
        categoryInputRef.current.setCustomValidity('');
      }
    }
  }, [selectedCategories]);
  
  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!isValidEmail(email)) {
      setToast({ type: 'error', message: 'Geçersiz e-posta adresi.' });
      return;
    }

    if (selectedCategories.length === 0) {
      return;
    }

    setLoading(true);
    setShowResend(false);

    try {
      const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
      const response = await axios.post(`${apiUrl}/api/subscribers`, {
        email,
        name,
        interests: selectedCategories.join(','),
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
      
      <form onSubmit={handleSubmit} className="p-6 md:p-8">
        <h2 className="text-3xl font-extrabold mb-6 text-text drop-shadow-sm">Bültenimize Katılın</h2>
        
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

        <div className="mb-6 relative">
          <label className="block text-sm font-medium text-text-muted mb-2">
            İlgilendiğiniz Konular (En az 1) <span className="text-red-500">*</span>
          </label>
          <div 
            className="w-full min-h-[42px] px-4 py-2 border border-gray-300 rounded-lg bg-surface flex items-center justify-between cursor-pointer transition-all hover:border-primary"
            onClick={() => !loading && setIsDropdownOpen(!isDropdownOpen)}
          >
            <div className="flex flex-wrap gap-2">
              {selectedCategories.length === 0 ? (
                <span className="text-gray-400">Konu seçiniz...</span>
              ) : (
                selectedCategories.map(cat => {
                  const categoryObj = categories.find(c => c.id === cat);
                  return (
                    <span key={cat} className="bg-primary/10 text-primary text-sm font-medium px-2 py-1 rounded-md flex items-center gap-1">
                      {categoryObj?.label}
                      <button 
                        type="button" 
                        onClick={(e) => { e.stopPropagation(); handleCategoryChange(cat); }}
                        className="hover:text-primary-dark focus:outline-none"
                      >
                        &times;
                      </button>
                    </span>
                  )
                })
              )}
            </div>
            <svg className={`w-5 h-5 text-gray-400 transition-transform ${isDropdownOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 9l-7 7-7-7" />
            </svg>
          </div>
          
          {/* Native validation catch for custom dropdown */}
          <input 
            ref={categoryInputRef}
            type="text" 
            className="opacity-0 absolute inset-0 w-full h-full z-[-1]" 
            required 
            value={selectedCategories.join(',')} 
            onChange={() => {}} 
          />

          {isDropdownOpen && (
            <div className="absolute z-10 w-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg max-h-60 overflow-auto">
              {categories.map(cat => {
                const isSelected = selectedCategories.includes(cat.id);
                return (
                  <div 
                    key={cat.id} 
                    className={`px-4 py-3 cursor-pointer flex items-center transition-colors ${
                      isSelected ? 'bg-primary/5 text-primary' : 'hover:bg-gray-50 text-text'
                    }`}
                    onClick={() => {
                      handleCategoryChange(cat.id);
                    }}
                  >
                    <div className={`w-5 h-5 mr-3 rounded border flex items-center justify-center ${isSelected ? 'bg-primary border-primary' : 'border-gray-300'}`}>
                      {isSelected && (
                        <svg className="w-3.5 h-3.5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M5 13l4 4L19 7" />
                        </svg>
                      )}
                    </div>
                    {cat.label}
                  </div>
                );
              })}
            </div>
          )}
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
