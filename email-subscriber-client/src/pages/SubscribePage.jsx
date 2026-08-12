import Navbar from '../components/Navbar';
import SubscribeForm from '../components/SubscribeForm';
import SubmailLogo from '../components/SubmailLogo';
import bgImage from '../assets/bg.png';

export default function SubscribePage() {
  return (
    <div 
      className="min-h-screen bg-surface flex flex-col font-sans relative"
      style={{ backgroundImage: `url(${bgImage})`, backgroundSize: 'cover', backgroundPosition: 'center', backgroundAttachment: 'fixed' }}
    >
      <div className="flex-1 flex flex-col min-h-screen bg-white/40 backdrop-blur-sm">
        <Navbar />

        <main className="flex-1 flex flex-col md:flex-row items-center justify-center p-6 md:p-12 max-w-7xl mx-auto w-full gap-12 relative z-10 pt-4 md:pt-6">
        
        {/* Hero Section */}
        <div className="flex-1 flex flex-col items-center justify-center text-center space-y-6">
          <img 
            src="/logo_hd.png?v=7" 
            alt="SUBMAIL Logo" 
            className="h-20 sm:h-24 md:h-[104px] lg:h-[116px] w-auto object-contain" 
          />
          <h1 className="text-4xl md:text-5xl font-extrabold text-text tracking-tight leading-tight w-full">
            Gelişmeleri İlk Sen Öğren!
          </h1>
          <p className="text-lg md:text-xl text-text-muted max-w-lg mx-auto">
            Haftalık bültenimize katılarak sektördeki en son trendleri, ipuçlarını ve özel içerikleri doğrudan e-posta kutunda bulabilirsin.
          </p>
          
          <ul className="space-y-4 pt-4 text-left inline-block mx-auto">
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
      </div>
    </div>
  );
}
