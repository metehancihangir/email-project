import React from 'react';
import Navbar from '../components/Navbar';
import { motion } from 'framer-motion';

export default function AboutPage() {
  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />
      <motion.main
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: "easeOut" }}
        className="flex-1 max-w-4xl mx-auto w-full p-6 md:p-12"
      >
        <div className="rounded-2xl p-4 md:p-8 text-text-muted">
          <h1 className="text-3xl md:text-4xl font-extrabold text-text mb-6 drop-shadow-sm">Hakkımızda</h1>
          
          <div className="space-y-5 font-medium text-lg">
            <p className="text-xl text-text font-semibold">
              Merhaba! SUBMAIL'e hoş geldiniz.
            </p>
            <p>
              Günümüzde bilgi kirliliği ve zaman darlığı, ilgi duyduğumuz alanlardaki gelişmeleri takip etmeyi oldukça zorlaştırıyor. SUBMAIL olarak bu sorunu çözmek için yola çıktık ve gelişmiş yapay zeka teknolojilerini kullanarak size özel, rafine edilmiş içerikler sunan bir platform geliştirdik.
            </p>
            
            <h3 className="text-xl font-bold text-text mt-8 drop-shadow-sm">Uygulamamız Ne İşe Yarıyor?</h3>
            <p>
              SUBMAIL, <strong>Finans</strong>, <strong>Mitoloji</strong>, <strong>Bilim</strong> ve <strong>Politika</strong> alanlarındaki en heyecan verici gelişmeleri, haftalık periyotlarla doğrudan e-posta kutunuza ulaştırır.
            </p>
            <ul className="list-disc pl-5 space-y-2 mt-4 text-base">
              <li><strong>Akıllı İçerik:</strong> Gelişmiş yapay zeka altyapımız, binlerce haberi ve kaynağı tarayarak sadece okumaya değer, en rafine bilgileri sizin için derler.</li>
              <li><strong>Zaman Tasarrufu:</strong> Uzun araştırmalar yapmanıza gerek kalmaz. Hafta sonu kahvenizi yudumlarken, sadece dakikalar içinde gündeme hakim olursunuz.</li>
              <li><strong>Sıfır Tekrar:</strong> Geliştirdiğimiz "Geçmiş Konu Belleği" sayesinde, daha önce okuduğunuz konular karşınıza tekrar çıkmaz. Her bültende yeni bir ufuk açılır.</li>
            </ul>

            <p className="mt-8 font-semibold">
              Amacımız vaktinizi çalmak değil, vaktinize değer katmaktır. Aramıza katıldığınız için teşekkür ederiz!
            </p>
          </div>
        </div>
      </motion.main>

      {/* Footer */}
      <footer className="py-6 text-center text-xs text-text-muted/60">
        <p>&copy; {new Date().getFullYear()} SUBMAIL. Tüm hakları saklıdır.</p>
        <p className="mt-1">
          <a href="/gizlilik-politikasi" className="hover:text-primary transition-colors">Gizlilik Politikası</a>
          <span className="mx-2">•</span>
          <a href="/kullanim-kosullari" className="hover:text-primary transition-colors">Kullanım Koşulları</a>
        </p>
      </footer>
    </div>
  );
}
