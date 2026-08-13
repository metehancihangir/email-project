import Navbar from '../components/Navbar';
import { motion } from 'framer-motion';

export default function HelpPage() {
  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />
      <motion.main 
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: "easeOut" }}
        className="flex-1 max-w-4xl mx-auto w-full p-6 md:p-12"
      >
        <div className="rounded-2xl p-4 md:p-8">
          <h1 className="text-3xl md:text-4xl font-extrabold text-text mb-2 drop-shadow-sm">Yardım ve İletişim</h1>
          <p className="text-text-muted mb-8 text-lg font-medium">Aklınıza takılan soruların yanıtları burada.</p>
          
          <div className="space-y-6">
            
            {/* SSS 1 */}
            <div className="p-2">
              <h3 className="text-xl font-bold text-text mb-2 drop-shadow-sm">Abone olmaktan nasıl çıkarım?</h3>
              <p className="text-text-muted">
                Gönderdiğimiz tüm e-posta bültenlerinin en alt kısmında "Abonelikten Ayrıl" bağlantısı bulunmaktadır. O bağlantıya tıklayarak tek adımda sistemimizden güvenle çıkış yapabilir ve bülten alımını durdurabilirsiniz.
              </p>
            </div>

            {/* SSS 2 */}
            <div className="p-2">
              <h3 className="text-xl font-bold text-text mb-2 drop-shadow-sm">Tercihlerimi (Bülten konularımı) nasıl değiştirebilirim?</h3>
              <p className="text-text-muted">
                Tıpkı abonelikten çıkma işlemi gibi, size gelen e-postaların alt kısmında yer alan "Tercihleri Güncelle" bağlantısına tıklayarak ilgili sayfaya gidebilirsiniz. O sayfadan (örneğin Finans, Bilim veya Mitoloji arasından) almak istediğiniz bültenleri dilediğiniz gibi açıp kapatabilirsiniz.
              </p>
            </div>
            
            {/* SSS 3 */}
            <div className="p-2">
              <h3 className="text-xl font-bold text-text mb-2 drop-shadow-sm">E-postalar hangi saatlerde geliyor?</h3>
              <div className="text-text-muted">
                Her konunun kendine has bir zamanlayıcısı vardır:
                <ul className="list-disc pl-5 mt-2 space-y-1">
                  <li><strong>Finans:</strong> Cumartesi Sabah 10:00</li>
                  <li><strong>Bilim:</strong> Cumartesi Akşam 20:00</li>
                  <li><strong>Mitoloji:</strong> Pazar Sabah 10:00</li>
                </ul>
              </div>
            </div>

            <hr className="my-8 border-white/20" />

            {/* İletişim */}
            <div className="p-2 text-center">
              <h3 className="text-2xl font-bold text-primary-dark mb-2 drop-shadow-sm">Bize Ulaşın</h3>
              <p className="text-text-muted mb-6">
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
