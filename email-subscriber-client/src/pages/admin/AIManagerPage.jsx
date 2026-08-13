import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import api from '../../api/axiosInstance';

export default function AIManagerPage() {
  const [triggering, setTriggering] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [successMessage, setSuccessMessage] = useState('');

  const categories = [
    { id: 'Mitoloji', name: 'Mitoloji', icon: '🏛️', color: 'bg-amber-100 text-amber-700' },
    { id: 'Finans', name: 'Finans', icon: '📈', color: 'bg-emerald-100 text-emerald-700' },
    { id: 'Bilim', name: 'Bilim', icon: '🔬', color: 'bg-blue-100 text-blue-700' }
  ];

  const handleTriggerClick = (category) => {
    setSelectedCategory(category);
    setShowConfirm(true);
    setSuccessMessage('');
  };

  const confirmTrigger = async () => {
    setTriggering(true);
    try {
      await api.post(`/api/admin/trigger-ai?category=${selectedCategory.id}`);
      setShowConfirm(false);
      setSuccessMessage(`${selectedCategory.name} kategorisinde yapay zeka tetiklendi! Bülten oluşturulup yakında tüm kullanıcılara gönderilecek.`);
      setTimeout(() => setSuccessMessage(''), 5000);
    } catch (err) {
      console.error(err);
      alert('Yapay zeka tetiklenirken bir hata oluştu.');
      setShowConfirm(false);
    } finally {
      setTriggering(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-text">Yapay Zeka Merkezi</h1>
      </div>

      <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
        <p className="text-gray-600 mb-8">
          Aşağıdaki butonları kullanarak yapay zekanın seçilen kategoride anında bir bülten oluşturmasını ve tüm kullanıcılara göndermesini sağlayabilirsiniz.
        </p>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {categories.map((cat) => (
            <motion.div 
              key={cat.id}
              whileHover={{ y: -4 }}
              className="bg-gray-50 rounded-xl p-6 border border-gray-100 flex flex-col items-center text-center space-y-4"
            >
              <div className={`w-16 h-16 rounded-full flex items-center justify-center text-3xl ${cat.color}`}>
                {cat.icon}
              </div>
              <h3 className="font-semibold text-lg">{cat.name} Kategorisi</h3>
              <button
                onClick={() => handleTriggerClick(cat)}
                className="w-full py-2.5 px-4 bg-primary text-white rounded-xl font-medium hover:bg-primary-hover transition-colors"
              >
                Yapay Zekayı Tetikle
              </button>
            </motion.div>
          ))}
        </div>

        <AnimatePresence>
          {successMessage && (
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className="mt-6 p-4 bg-green-50 text-green-700 border border-green-200 rounded-xl flex items-center gap-3"
            >
              <svg className="w-5 h-5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7" />
              </svg>
              <span>{successMessage}</span>
            </motion.div>
          )}
        </AnimatePresence>
      </div>

      {/* Onay Modalı */}
      <AnimatePresence>
        {showConfirm && selectedCategory && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
          >
            <motion.div
              initial={{ scale: 0.95 }}
              animate={{ scale: 1 }}
              exit={{ scale: 0.95 }}
              className="bg-white rounded-2xl shadow-xl w-full max-w-md p-6"
            >
              <div className="flex items-center gap-4 mb-4 text-amber-500">
                <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
                <h2 className="text-xl font-bold text-text">DİKKAT!</h2>
              </div>
              
              <p className="text-gray-600 mb-6">
                Bu işleme devam ederseniz, yapay zeka <strong>{selectedCategory.name}</strong> konusunda bir bülten oluşturacak ve bunu tüm abonelere gönderecektir. <br/><br/>
                Bu işlem geri alınamaz. Emin misiniz?
              </p>

              <div className="flex gap-3 justify-end">
                <button
                  onClick={() => setShowConfirm(false)}
                  disabled={triggering}
                  className="px-4 py-2 text-gray-500 hover:bg-gray-100 rounded-xl font-medium transition-colors"
                >
                  İptal Et
                </button>
                <button
                  onClick={confirmTrigger}
                  disabled={triggering}
                  className="px-4 py-2 bg-primary text-white rounded-xl font-medium hover:bg-primary-hover transition-colors flex items-center gap-2"
                >
                  {triggering ? (
                    <>
                      <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                      Tetikleniyor...
                    </>
                  ) : (
                    "Evet, Bülteni Gönder"
                  )}
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
