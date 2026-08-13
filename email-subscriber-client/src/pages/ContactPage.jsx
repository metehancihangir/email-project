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
        className="flex-1 flex items-center justify-center max-w-4xl mx-auto w-full p-6 md:p-12"
      >
        <div className="rounded-2xl p-4 md:p-8 text-center max-w-lg w-full">
          <h1 className="text-3xl md:text-4xl font-extrabold text-text mb-4 drop-shadow-sm">İletişim</h1>
          <p className="text-text-muted mb-8 text-lg font-medium">
            Önerileriniz, şikayetleriniz veya işbirlikleri için ekibimizle her zaman iletişime geçebilirsiniz. Fikirlerinize çok değer veriyoruz.
          </p>
          
          <a 
            href="https://mail.google.com/mail/?view=cm&fs=1&to=metehancihangir10@gmail.com" 
            target="_blank" 
            rel="noopener noreferrer" 
            className="inline-block bg-primary text-white font-medium py-3 px-8 rounded-lg hover:bg-primary-dark transition-colors shadow-sm"
          >
            Bize Ulaşın
          </a>
        </div>
      </motion.main>

      {/* Footer */}
      <footer className="py-6 text-center text-xs text-text-muted/60">
        <p>&copy; {new Date().getFullYear()} SUBMAIL. Tüm hakları saklıdır.</p>
      </footer>
    </div>
  );
}
