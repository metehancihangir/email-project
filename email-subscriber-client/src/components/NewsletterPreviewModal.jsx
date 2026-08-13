import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';

export default function NewsletterPreviewModal({ isOpen, onClose }) {
  if (!isOpen) return null;

  return (
    <AnimatePresence>
      <div 
        className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 bg-black/60 backdrop-blur-sm cursor-pointer"
        onClick={onClose}
      >
        <motion.div
          onClick={(e) => e.stopPropagation()}
          initial={{ opacity: 0, y: 20, scale: 0.95 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, y: 20, scale: 0.95 }}
          transition={{ duration: 0.2 }}
          className="bg-[#f9fafb] w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh] cursor-default border border-gray-200"
        >
          {/* Header */}
          <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center bg-white">
            <div className="flex items-center gap-2">
              <span className="w-8 h-8 bg-primary text-white rounded flex items-center justify-center font-bold text-lg">S</span>
              <span className="font-bold text-gray-800 tracking-tight">SUBMAIL</span>
            </div>
            <button 
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 focus:outline-none transition-colors"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
          </div>

          {/* Email Content Simulation */}
          <div className="p-6 md:p-8 overflow-y-auto flex-1 font-sans text-gray-800">
            <div className="max-w-xl mx-auto bg-white border border-gray-100 shadow-sm rounded-lg overflow-hidden">
              <div className="bg-primary px-6 py-4 text-white text-center">
                <h2 className="text-xl font-bold tracking-wider">HAFTALIK FİNANS ÖZETİ</h2>
                <p className="text-sm opacity-90 mt-1">{new Date().toLocaleDateString('tr-TR')}</p>
              </div>
              
              <div className="p-6 space-y-6">
                <div>
                  <h3 className="text-xl font-bold text-gray-900 mb-2 leading-tight">Küresel Piyasalarda Yapay Zeka Rallisi Devam Ediyor</h3>
                  <img src="https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?q=80&w=600&h=300&fit=crop" alt="Finance Cover" className="w-full h-48 object-cover rounded-md mb-4 bg-gray-100" />
                  <p className="text-gray-600 text-sm leading-relaxed">
                    Teknoloji hisseleri önderliğinde borsalar haftayı rekor seviyelerde kapattı. Uzmanlar, yapay zeka entegrasyonlarının kurumsal karlılıklara etkisinin önümüzdeki çeyrekte daha net görüleceğini belirtiyor. Merkez bankalarının faiz indirim sinyalleri de piyasalardaki rüzgarı desteklemeye devam ediyor.
                  </p>
                </div>
                
                <hr className="border-gray-100" />

                <div>
                  <h3 className="text-xl font-bold text-gray-900 mb-2 leading-tight">Kripto Paralarda Kurumsal İlgi Artışı</h3>
                  <p className="text-gray-600 text-sm leading-relaxed mb-4">
                    Spot ETF onaylarının ardından kurumsal yatırımcıların Bitcoin ve Ethereum piyasalarına girişi hızlandı. Regülasyon netliği sağlandıkça portföylerdeki dijital varlık ağırlıklarının artması bekleniyor.
                  </p>
                  <span className="text-primary font-semibold text-sm cursor-default">Haberin Detayını Oku &rarr;</span>
                </div>
              </div>
              
              <div className="bg-gray-50 px-6 py-5 text-center border-t border-gray-100">
                <p className="text-xs text-gray-400 mb-2">Bu e-postayı aldınız çünkü SUBMAIL bültenine abonesiniz.</p>
                <div className="space-x-4">
                  <span className="text-xs text-primary underline cursor-default">Tercihleri Güncelle</span>
                  <span className="text-xs text-gray-500 underline cursor-default">Abonelikten Ayrıl</span>
                </div>
              </div>
            </div>
          </div>
        </motion.div>
      </div>
    </AnimatePresence>
  );
}
