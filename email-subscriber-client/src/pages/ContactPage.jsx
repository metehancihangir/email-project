import Navbar from '../components/Navbar';
import { motion } from 'framer-motion';

export default function ContactPage() {
  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />
      <motion.main 
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: "easeOut" }}
        className="flex-1 flex items-center justify-center max-w-4xl mx-auto w-full p-4 sm:p-6 md:p-12"
      >
        <div className="bg-white/80 backdrop-blur-md shadow-xl border border-white/60 rounded-3xl p-8 sm:p-12 text-center max-w-lg w-full space-y-6">
          <div className="w-16 h-16 bg-primary-light text-primary rounded-2xl flex items-center justify-center mx-auto shadow-xs">
            <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
            </svg>
          </div>

          <h1 className="text-3xl md:text-4xl font-extrabold text-text tracking-tight">İletişim</h1>
          
          <p className="text-text-muted text-base leading-relaxed">
            Önerileriniz, geri bildirimleriniz veya işbirlikleri için ekibimizle her zaman iletişime geçebilirsiniz. Fikirlerinize büyük değer veriyoruz.
          </p>
          
          <div className="pt-2">
            <a 
              href="https://mail.google.com/mail/?view=cm&fs=1&to=metehancihangir10@gmail.com" 
              target="_blank" 
              rel="noopener noreferrer" 
              className="inline-flex items-center justify-center gap-2 bg-primary text-white font-bold py-3.5 px-8 rounded-2xl hover:bg-primary-hover transition-all shadow-md shadow-primary/25 active:scale-95 text-sm"
            >
              <span>Bize Ulaşın</span>
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M14 5l7 7m0 0l-7 7m7-7H3" />
              </svg>
            </a>
          </div>
        </div>
      </motion.main>

      {/* Footer */}
      <footer className="py-6 text-center text-xs text-text-muted/60">
        <p>&copy; {new Date().getFullYear()} SUBMAIL. Tüm hakları saklıdır.</p>
      </footer>
    </div>
  );
}
