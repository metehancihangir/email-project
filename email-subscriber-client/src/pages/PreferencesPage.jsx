import { useEffect, useState, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import axios from 'axios';
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
    { id: "Mitoloji", label: "Mitoloji 🏛️" },
    { id: "Bilim", label: "Bilim 🔬" },
    { id: "Finans", label: "Finans 💰" }
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
        const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
        const res = await axios.get(`${apiUrl}/api/subscribers/preferences/${token}`);
        if (res.data.interests) {
          setSelectedCategories(res.data.interests.split(',').map(s => s.trim()));
        }
      } catch (err) {
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
      const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
      await axios.put(`${apiUrl}/api/subscribers/preferences/${token}`, {
        interests: selectedCategories.join(',')
      });
      setToast({ type: 'success', message: 'Tercihleriniz başarıyla güncellendi!' });
    } catch (err) {
      setToast({ type: 'error', message: 'Tercihler güncellenemedi.' });
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[50vh]">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
        <p className="mt-4 text-text-muted">Tercihleriniz yükleniyor...</p>
      </div>
    );
  }

  return (
    <div className="w-full max-w-md mx-auto">
      <Toast type={toast.type} message={toast.message} onClose={closeToast} />
      <div className="bg-surface-alt p-6 md:p-8 rounded-xl shadow-lg border border-gray-100">
        <h2 className="text-2xl font-semibold mb-2 text-text text-center">Bülten Tercihleri</h2>
        <p className="text-sm text-text-muted text-center mb-6">
          Hafta sonu bültenleriniz için ilgi alanlarınızı güncelleyebilirsiniz.
        </p>

        {token ? (
          <form onSubmit={handleSave}>
            <div className="mb-6 relative">
              <label className="block text-sm font-medium text-text-muted mb-3">
                İlgilendiğiniz Konular (En az 1) <span className="text-red-500">*</span>
              </label>
              <div 
                className={`w-full min-h-[42px] px-4 py-2 border border-gray-300 rounded-lg bg-surface flex items-center justify-between cursor-pointer transition-all hover:border-primary ${saving ? 'opacity-50 pointer-events-none' : ''}`}
                onClick={() => !saving && setIsDropdownOpen(!isDropdownOpen)}
              >
                <div className="flex flex-wrap gap-2">
                  {selectedCategories.length === 0 ? (
                    <span className="text-gray-400">Konu seçiniz...</span>
                  ) : (
                    selectedCategories.map(cat => {
                      const categoryObj = categories.find(c => c.id === cat);
                      return (
                        <span key={cat} className="bg-primary/10 text-primary text-sm font-medium px-2 py-1 rounded-md flex items-center gap-1">
                          {categoryObj?.label || cat}
                          <button 
                            type="button" 
                            onClick={(e) => { e.stopPropagation(); handleCategoryChange(cat); }}
                            className="hover:text-primary-dark focus:outline-none"
                            disabled={saving}
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
            <button
              type="submit"
              disabled={saving}
              className={`w-full py-3 rounded-lg font-medium text-white transition-all flex justify-center items-center ${
                saving ? 'bg-primary-dark opacity-70 cursor-not-allowed' : 'bg-primary hover:bg-primary-dark shadow-md hover:shadow-lg'
              }`}
            >
              {saving ? 'Kaydediliyor...' : 'Tercihleri Kaydet'}
            </button>
          </form>
        ) : (
          <div className="text-center py-8">
            <p className="text-text-muted">Geçersiz bağlantı. Lütfen e-postanızdaki linki kullandığınızdan emin olun.</p>
          </div>
        )}
      </div>
    </div>
  );
}
