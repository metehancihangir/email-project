import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';

export default function PrivacyModal({ isOpen, onClose, onAccept }) {
  if (!isOpen) return null;

  return (
    <AnimatePresence>
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 bg-black/40 backdrop-blur-sm">
        <motion.div
          initial={{ opacity: 0, y: 20, scale: 0.95 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, y: 20, scale: 0.95 }}
          transition={{ duration: 0.2 }}
          className="bg-surface w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[85vh]"
        >
          {/* Header */}
          <div className="px-6 py-4 border-b border-gray-100 flex justify-between items-center bg-surface-alt">
            <h2 className="text-xl font-bold text-text">Aydınlatma Metni ve Gizlilik Politikası</h2>
            <button onClick={onClose} className="text-gray-400 hover:text-gray-600 transition-colors">
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" /></svg>
            </button>
          </div>

          {/* Content */}
          <div className="p-6 overflow-y-auto flex-1 text-sm text-text-muted space-y-4">
            <p><strong>Son Güncelleme:</strong> {new Date().toLocaleDateString('tr-TR')}</p>
            
            <h3 className="text-lg font-semibold text-text mt-4">1. Veri Sorumlusu</h3>
            <p>Bu aydınlatma metni, 6698 sayılı Kişisel Verilerin Korunması Kanunu (KVKK) uyarınca, SUBMAIL tarafından kişisel verilerinizin işlenmesine ilişkin kuralları içermektedir.</p>
            
            <h3 className="text-lg font-semibold text-text mt-4">2. İşlenen Kişisel Veriler ve İşlenme Amacı</h3>
            <p>Bültenimize abone olmanız halinde, tarafımızca adınız, soyadınız ve e-posta adresiniz işlenmektedir. Bu veriler yalnızca size sektörel gelişmeler, bültenler ve kampanya bilgilendirmeleri göndermek amacıyla kullanılmaktadır.</p>
            
            <h3 className="text-lg font-semibold text-text mt-4">3. Kişisel Verilerin Aktarılması</h3>
            <p>Toplanan kişisel verileriniz, hukuki yükümlülüklerimizin yerine getirilmesi amacıyla yetkili kamu kurumları dışında herhangi bir üçüncü taraf ile paylaşılmamaktadır.</p>
            
            <h3 className="text-lg font-semibold text-text mt-4">4. Haklarınız</h3>
            <p>KVKK 11. Madde kapsamında; kişisel verilerinizin işlenip işlenmediğini öğrenme, düzeltilmesini veya silinmesini talep etme ve ileti almayı reddetme hakkına sahipsiniz. İstediğiniz zaman e-postaların altındaki "Abonelikten Ayrıl" bağlantısına tıklayarak listeden çıkabilirsiniz.</p>
          </div>

          {/* Footer */}
          <div className="px-6 py-4 border-t border-gray-100 bg-surface-alt flex justify-end gap-3">
            <button
              onClick={onClose}
              className="px-5 py-2.5 rounded-lg text-sm font-medium text-gray-600 hover:bg-gray-200 transition-colors"
            >
              Kapat
            </button>
            {onAccept && (
              <button
                onClick={onAccept}
                className="px-5 py-2.5 rounded-lg text-sm font-medium text-white bg-primary hover:bg-primary-dark transition-colors shadow-sm"
              >
                Okudum, Onaylıyorum
              </button>
            )}
          </div>
        </motion.div>
      </div>
    </AnimatePresence>
  );
}
