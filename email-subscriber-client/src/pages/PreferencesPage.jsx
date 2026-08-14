import { useEffect, useState, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { motion } from 'framer-motion';
import axios from 'axios';
import Navbar from '../components/Navbar';
import Toast from '../components/Toast';

export default function PreferencesPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [toast, setToast] = useState({ message: '', type: '' });
  const [selectedCategories, setSelectedCategories] = useState([]);
  const categoryInputRef = useRef(null);

  const categories = [
    { id: 'Mitoloji', label: 'Mitoloji 🏛️', description: 'Antik efsaneler, tanrılar ve efsanevi yaratıklar hakkında ilginç bilgiler.' },
    { id: 'Bilim', label: 'Bilim 🔬', description: 'Uzay, teknoloji, doğa ve evrene dair en yeni bilimsel keşifler.' },
    { id: 'Finans', label: 'Finans 💰', description: 'Piyasalar, yatırım dünyası ve ekonomideki son gelişmeler.' },
    { id: 'Politika', label: 'Politika 🌍', description: 'İç ve dış siyasetteki gelişmeler, yasalar ve jeopolitik olaylar.' }
  ];
  
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  useEffect(() => {
    if (!token) {
      setToast({ type: 'error', message: 'Token bulunamadı.' });
      setLoading(false);
      return;
    }

    const fetchPreferences = async () => {
      try {
        const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5117';
        const res = await axios.get(`${apiUrl}/api/subscribers/preferences/${token}`);
        if (res.data.interests) {
          setSelectedCategories(res.data.interests.split(',').map(s => s.trim()));
        }
      } catch (err) {
        console.error(err);
        setToast({ type: 'error', message: 'Tercihlerinizi yüklerken bir hata oluştu veya bağlantınız geçersiz.' });
      } finally {
        setLoading(false);
      }
    };

    fetchPreferences();
  }, [token]);

  useEffect(() => {
    if (categoryInputRef.current) {
      if (selectedCategories.length === 0) {
        categoryInputRef.current.setCustomValidity('Lütfen en az 1 konu seçin.');
      } else {
        categoryInputRef.current.setCustomValidity('');
      }
    }
  }, [selectedCategories]);

  const closeToast = () => setToast({ message: '', type: '' });

  const handleCategoryChange = (cat) => {
    if (selectedCategories.includes(cat)) {
      setSelectedCategories(prev => prev.filter(c => c !== cat));
    } else {
      setSelectedCategories(prev => [...prev, cat]);
    }
  };

  const handleSave = async (e) => {
    e?.preventDefault();
    if (selectedCategories.length === 0) {
      return;
    }

    setSaving(true);
    try {
      const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5117';
      await axios.put(`${apiUrl}/api/subscribers/preferences/${token}`, {
        interests: selectedCategories.join(',')
      });
      setToast({ type: 'success', message: 'Tercihleriniz başarıyla güncellendi!' });
    } catch (err) {
      console.error(err);
      setToast({ type: 'error', message: 'Tercihler güncellenemedi.' });
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />

      <main className="flex-1 flex items-center justify-center p-4 sm:p-6 md:p-12 w-full max-w-xl mx-auto">
        <Toast type={toast.type} message={toast.message} onClose={closeToast} />

        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white/80 backdrop-blur-md p-6 sm:p-8 md:p-10 rounded-3xl shadow-xl border border-white/60 w-full"
        >
          <div className="text-center mb-6">
            <h2 className="text-2xl sm:text-3xl font-extrabold text-text tracking-tight mb-2">Bülten Tercihleri</h2>
            <p className="text-sm text-text-muted">
              Haftalık bültenleriniz için ilgi alanlarınızı dilediğiniz gibi güncelleyebilirsiniz.
            </p>
          </div>

          {loading ? (
            <div className="flex flex-col items-center justify-center py-12">
              <div className="w-10 h-10 border-3 border-primary/20 border-t-primary rounded-full animate-spin"></div>
              <p className="mt-4 text-sm text-text-muted font-medium">Tercihleriniz yükleniyor...</p>
            </div>
          ) : token ? (
            <form onSubmit={handleSave} className="space-y-6">
              <div className="relative">
                <label className="block text-xs font-bold text-text-muted uppercase tracking-wider mb-2">
                  İlgilendiğiniz Konular (En az 1) <span className="text-red-500">*</span>
                </label>

                <div 
                  className={`w-full min-h-[48px] px-4 py-2.5 border border-slate-200/80 rounded-2xl bg-white/90 flex items-center justify-between cursor-pointer transition-all hover:border-primary shadow-xs ${saving ? 'opacity-50 pointer-events-none' : ''}`}
                  onClick={() => !saving && setIsDropdownOpen(!isDropdownOpen)}
                >
                  <div className="flex flex-wrap gap-2">
                    {selectedCategories.length === 0 ? (
                      <span className="text-slate-400 text-sm">Konu seçiniz...</span>
                    ) : (
                      selectedCategories.map(cat => {
                        const categoryObj = categories.find(c => c.id === cat);
                        return (
                          <span key={cat} className="bg-primary/10 text-primary text-xs font-bold px-2.5 py-1 rounded-xl flex items-center gap-1.5 border border-primary/20">
                            {categoryObj?.label || cat}
                            <button 
                              type="button" 
                              onClick={(e) => { e.stopPropagation(); handleCategoryChange(cat); }}
                              className="hover:text-primary-hover focus:outline-none cursor-pointer text-sm font-bold"
                              disabled={saving}
                            >
                              &times;
                            </button>
                          </span>
                        )
                      })
                    )}
                  </div>
                  <svg className={`w-5 h-5 text-slate-400 transition-transform ${isDropdownOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 9l-7 7-7-7" />
                  </svg>
                </div>

                <input 
                  ref={categoryInputRef}
                  type="text" 
                  className="opacity-0 absolute inset-0 w-full h-full z-[-1]" 
                  required 
                  value={selectedCategories.join(',')} 
                  onChange={() => {}} 
                />

                {isDropdownOpen && (
                  <div className="absolute z-20 w-full mt-2 bg-white/95 backdrop-blur-md border border-slate-200/80 rounded-2xl shadow-xl max-h-60 overflow-auto p-1.5 space-y-1">
                    {categories.map(cat => {
                      const isSelected = selectedCategories.includes(cat.id);
                      return (
                        <div 
                          key={cat.id} 
                          className={`px-4 py-3 rounded-xl cursor-pointer flex items-center justify-between transition-colors ${
                            isSelected ? 'bg-primary/10 text-primary font-bold' : 'hover:bg-slate-50 text-text'
                          }`}
                          onClick={() => handleCategoryChange(cat.id)}
                        >
                          <div className="flex items-center gap-3">
                            <div className={`w-5 h-5 rounded-lg border flex items-center justify-center ${isSelected ? 'bg-primary border-primary' : 'border-gray-300'}`}>
                              {isSelected && (
                                <svg className="w-3.5 h-3.5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M5 13l4 4L19 7" />
                                </svg>
                              )}
                            </div>
                            <span className="text-sm font-medium">{cat.label}</span>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>

              <button
                type="submit"
                disabled={saving || selectedCategories.length === 0}
                className="w-full py-3.5 px-4 rounded-2xl font-bold text-sm text-white bg-primary hover:bg-primary-hover shadow-md shadow-primary/25 transition-all active:scale-98 disabled:opacity-50 disabled:cursor-not-allowed flex justify-center items-center cursor-pointer"
              >
                {saving ? (
                  <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                ) : (
                  'Tercihleri Kaydet'
                )}
              </button>
            </form>
          ) : (
            <div className="text-center py-8 space-y-3">
              <p className="text-text-muted text-sm leading-relaxed">
                Geçersiz veya süresi dolmuş bağlantı. Lütfen e-postanızdaki en güncel linki kullandığınızdan emin olun.
              </p>
            </div>
          )}
        </motion.div>
      </main>

      <footer className="py-6 text-center text-xs text-text-muted/60">
        <p>&copy; {new Date().getFullYear()} SUBMAIL. Tüm hakları saklıdır.</p>
      </footer>
    </div>
  );
}
