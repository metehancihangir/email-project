import Navbar from '../components/Navbar';
import SubscribeForm from '../components/SubscribeForm';
import Logo from '../components/Logo';

export default function SubscribePage() {
  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />

      <main className="flex-1 flex flex-col md:flex-row items-center justify-center p-6 md:p-12 max-w-7xl mx-auto w-full gap-12">
        
        {/* Hero Section */}
        <div className="flex-1 space-y-6 text-center md:text-left">
          <div className="flex justify-center md:justify-start mb-6">
            <Logo variant="icon" className="w-20 h-20 md:w-24 md:h-24 drop-shadow-md rounded-[1.25rem] overflow-hidden" />
          </div>
          <h1 className="text-4xl md:text-5xl font-extrabold text-text tracking-tight leading-tight">
            <span className="text-primary">Yapay Zeka</span> ile Gündemi Yakala!
          </h1>
          <p className="text-lg md:text-xl text-text-muted max-w-lg">
            Finans, Bilim ve Mitoloji dünyasındaki en çarpıcı gelişmeleri, yapay zeka destekli akıllı bültenimizle doğrudan e-posta kutunda keşfet.
          </p>
          
          <ul className="space-y-4 pt-4 text-left inline-block md:block mx-auto">
            <li className="flex items-center text-text gap-3">
              <span className="w-6 h-6 rounded-full bg-primary-light text-primary flex items-center justify-center">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
              </span>
              <span>Yapay zeka ile özetlenmiş, rafine ve net içerikler.</span>
            </li>
            <li className="flex items-center text-text gap-3">
              <span className="w-6 h-6 rounded-full bg-primary-light text-primary flex items-center justify-center">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
              </span>
              <span>Finans, Bilim ve Mitoloji için özel zamanlanmış bültenler.</span>
            </li>
            <li className="flex items-center text-text gap-3">
              <span className="w-6 h-6 rounded-full bg-primary-light text-primary flex items-center justify-center">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
              </span>
              <span>İstediğin zaman konuları değiştirme veya abonelikten çıkma özgürlüğü.</span>
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
