import Navbar from '../components/Navbar';
import SubscribeForm from '../components/SubscribeForm';

export default function SubscribePage() {
  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />

      <main className="flex-1 flex flex-col md:flex-row items-center justify-center p-6 md:p-12 max-w-7xl mx-auto w-full gap-12">
        
        {/* Hero Section */}
        <div className="flex-1 space-y-6 text-center md:text-left">
          <h1 className="text-4xl md:text-5xl font-extrabold text-text tracking-tight leading-tight">
            Gelişmeleri İlk Sen Öğren!
          </h1>
          <p className="text-lg md:text-xl text-text-muted max-w-lg">
            Haftalık bültenimize katılarak sektördeki en son trendleri, ipuçlarını ve özel içerikleri doğrudan e-posta kutunda bulabilirsin.
          </p>
          
          <ul className="space-y-4 pt-4 text-left inline-block md:block mx-auto">
            <li className="flex items-center text-text gap-3">
              <span className="w-6 h-6 rounded-full bg-primary-light text-primary flex items-center justify-center">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
              </span>
              <span>Sıfır spam, tamamen değer odaklı içerik.</span>
            </li>
            <li className="flex items-center text-text gap-3">
              <span className="w-6 h-6 rounded-full bg-primary-light text-primary flex items-center justify-center">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
              </span>
              <span>Her hafta düzenli ve özgün yazılar.</span>
            </li>
            <li className="flex items-center text-text gap-3">
              <span className="w-6 h-6 rounded-full bg-primary-light text-primary flex items-center justify-center">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
              </span>
              <span>İstediğiniz zaman tek tıkla abonelikten ayrılma özgürlüğü.</span>
            </li>
          </ul>
        </div>

        {/* Form Section */}
        <div className="flex-1 w-full max-w-md">
          <SubscribeForm />
        </div>

      </main>

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
