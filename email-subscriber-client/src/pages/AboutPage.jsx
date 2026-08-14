import React from 'react';
import { Link } from 'react-router-dom';
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
        <div className="bg-white/70 backdrop-blur-md shadow-xl border border-white/50 rounded-2xl p-8 md:p-12 text-text-muted">
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
            <ul className="space-y-4 mt-6 text-base">
              <li className="flex gap-4 items-start">
                <span className="w-8 h-8 rounded-full bg-primary-light text-primary flex items-center justify-center shrink-0 mt-1">
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 10V3L4 14h7v7l9-11h-7z"></path></svg>
                </span>
                <div>
                  <strong className="text-text block mb-1">Akıllı İçerik</strong>
                  Gelişmiş yapay zeka altyapımız, binlerce haberi ve kaynağı tarayarak <strong>sadece okumaya değer</strong>, en rafine bilgileri sizin için derler.
                </div>
              </li>
              <li className="flex gap-4 items-start">
                <span className="w-8 h-8 rounded-full bg-primary-light text-primary flex items-center justify-center shrink-0 mt-1">
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                </span>
                <div>
                  <strong className="text-text block mb-1">Zaman Tasarrufu</strong>
                  Uzun araştırmalar yapmanıza gerek kalmaz. Hafta sonu kahvenizi yudumlarken, sadece dakikalar içinde <strong>gündeme hakim olursunuz</strong>.
                </div>
              </li>
              <li className="flex gap-4 items-start">
                <span className="w-8 h-8 rounded-full bg-primary-light text-primary flex items-center justify-center shrink-0 mt-1">
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path></svg>
                </span>
                <div>
                  <strong className="text-text block mb-1">Sıfır Tekrar (Geçmiş Konu Belleği)</strong>
                  Geliştirdiğimiz "Geçmiş Konu Belleği" sayesinde, <strong>daha önce okuduğunuz konular karşınıza tekrar çıkmaz</strong>. Her bültende yeni bir ufuk açılır.
                </div>
              </li>
            </ul>

            <p className="mt-8 font-semibold">
              Amacımız vaktinizi çalmak değil, vaktinize değer katmaktır. Aramıza katıldığınız için teşekkür ederiz!
            </p>

            <div className="mt-10 text-center">
              <Link
                to="/arsiv"
                className="inline-flex items-center gap-2 text-primary font-bold hover:text-primary-hover transition-colors px-7 py-3.5 bg-primary/10 rounded-2xl hover:bg-primary/20 shadow-xs"
              >
                <span>Bülten Arşivine Göz At</span>
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5l7 7-7 7" /></svg>
              </Link>
            </div>
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
