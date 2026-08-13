import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import Logo from './Logo';

export default function AboutModal({ isOpen, onClose }) {
  if (!isOpen) return null;

  return (
    <AnimatePresence>
      <div 
        className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6 bg-black/40 backdrop-blur-sm cursor-pointer"
        onClick={onClose}
      >
        <motion.div
          onClick={(e) => e.stopPropagation()}
          initial={{ opacity: 0, y: 20, scale: 0.95 }}
          animate={{ opacity: 1, y: 0, scale: 1 }}
          exit={{ opacity: 0, y: 20, scale: 0.95 }}
          transition={{ duration: 0.2 }}
          className="bg-auth-pattern bg-cover bg-center w-full max-w-4xl rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh] cursor-default"
        >
          {/* Header */}
          <div className="px-6 py-5 border-b border-white/20 flex justify-center items-center bg-white/40 backdrop-blur-md">
            <Logo variant="full" className="h-6 md:h-8 opacity-90 drop-shadow-sm" />
          </div>

          {/* Content */}
          <div className="p-8 overflow-y-auto flex-1 text-base text-gray-800 space-y-5 bg-white/30 backdrop-blur-md font-medium">
            <p className="text-xl text-black">
              <strong>Merhaba! SUBMAIL'e hoş geldiniz.</strong>
            </p>
            <p>
              Günümüzde bilgi kirliliği ve zaman darlığı, ilgi duyduğumuz alanlardaki gelişmeleri takip etmeyi oldukça zorlaştırıyor. SUBMAIL olarak bu sorunu çözmek için yola çıktık ve gelişmiş yapay zeka teknolojilerini kullanarak size özel, rafine edilmiş içerikler sunan bir platform geliştirdik.
            </p>
            
            <h3 className="text-lg font-semibold text-text mt-6">Uygulamamız Ne İşe Yarıyor?</h3>
            <p>
              SUBMAIL, <strong>Finans</strong>, <strong>Bilim</strong> ve <strong>Mitoloji</strong> alanlarındaki en heyecan verici gelişmeleri, haftalık periyotlarla doğrudan e-posta kutunuza ulaştırır.
            </p>
            <ul className="list-disc pl-5 space-y-2 mt-2">
              <li><strong>Akıllı İçerik:</strong> Gelişmiş yapay zeka altyapımız, binlerce haberi ve kaynağı tarayarak sadece okumaya değer, en rafine bilgileri sizin için derler.</li>
              <li><strong>Zaman Tasarrufu:</strong> Uzun araştırmalar yapmanıza gerek kalmaz. Hafta sonu kahvenizi yudumlarken, sadece dakikalar içinde gündeme hakim olursunuz.</li>
              <li><strong>Sıfır Tekrar:</strong> Geliştirdiğimiz "Geçmiş Konu Belleği" sayesinde, daha önce okuduğunuz konular karşınıza tekrar çıkmaz. Her bültende yeni bir ufuk açılır.</li>
            </ul>

            <p className="mt-6">
              Amacımız vaktinizi çalmak değil, vaktinize değer katmaktır. Aramıza katıldığınız için teşekkür ederiz!
            </p>
          </div>
        </motion.div>
      </div>
    </AnimatePresence>
  );
}
