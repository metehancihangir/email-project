import React from 'react';
import Navbar from '../components/Navbar';
import { motion } from 'framer-motion';

export default function PrivacyPolicyPage() {
  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />
      <motion.main
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: "easeOut" }}
        className="flex-1 max-w-4xl mx-auto w-full p-6 md:p-12"
      >
        <div className="p-4 md:p-8 prose max-w-none text-text-muted">
          <h1 className="text-3xl font-bold text-text mb-6 drop-shadow-sm">Aydınlatma Metni ve Gizlilik Politikası</h1>
          <p className="mb-4">
            Son Güncelleme: {new Date().toLocaleDateString('tr-TR')}
          </p>
          <h2 className="text-xl font-semibold text-text mt-6 mb-3 drop-shadow-sm">1. Veri Sorumlusu</h2>
          <p className="mb-4">
            Bu aydınlatma metni, 6698 sayılı Kişisel Verilerin Korunması Kanunu (KVKK) uyarınca, SUBMAIL tarafından kişisel verilerinizin işlenmesine ilişkin kuralları içermektedir.
          </p>
          <h2 className="text-xl font-semibold text-text mt-6 mb-3 drop-shadow-sm">2. İşlenen Kişisel Veriler ve İşlenme Amacı</h2>
          <p className="mb-4">
            Bültenimize abone olmanız halinde, tarafımızca adınız, soyadınız ve e-posta adresiniz işlenmektedir. Bu veriler yalnızca size sektörel gelişmeler, bültenler ve kampanya bilgilendirmeleri göndermek amacıyla kullanılmaktadır.
          </p>
          <h2 className="text-xl font-semibold text-text mt-6 mb-3 drop-shadow-sm">3. Kişisel Verilerin Aktarılması</h2>
          <p className="mb-4">
            Toplanan kişisel verileriniz, hukuki yükümlülüklerimizin yerine getirilmesi amacıyla yetkili kamu kurumları dışında herhangi bir üçüncü taraf ile paylaşılmamaktadır.
          </p>
          <h2 className="text-xl font-semibold text-text mt-6 mb-3 drop-shadow-sm">4. Haklarınız</h2>
          <p className="mb-4">
            KVKK 11. Madde kapsamında; kişisel verilerinizin işlenip işlenmediğini öğrenme, düzeltilmesini veya silinmesini talep etme ve ileti almayı reddetme hakkına sahipsiniz. İstediğiniz zaman e-postaların altındaki "Abonelikten Ayrıl" bağlantısına tıklayarak listeden çıkabilirsiniz.
          </p>
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
