import { useState } from 'react';
import Navbar from '../components/Navbar';
import { motion, AnimatePresence } from 'framer-motion';

const faqs = [
  {
    question: "Abone olmaktan nasıl çıkarım?",
    answer: "Gönderdiğimiz tüm e-posta bültenlerinin en alt kısmında \"Abonelikten Ayrıl\" bağlantısı bulunmaktadır. O bağlantıya tıklayarak tek adımda sistemimizden güvenle çıkış yapabilir ve bülten alımını durdurabilirsiniz."
  },
  {
    question: "Tercihlerimi (Bülten konularımı) nasıl değiştirebilirim?",
    answer: "Tıpkı abonelikten çıkma işlemi gibi, size gelen e-postaların alt kısmında yer alan \"Tercihleri Güncelle\" bağlantısına tıklayarak ilgili sayfaya gidebilirsiniz. O sayfadan (örneğin Finans, Bilim, Politika veya Mitoloji arasından) almak istediğiniz bültenleri dilediğiniz gibi açıp kapatabilirsiniz."
  },
  {
    question: "E-postalar hangi saatlerde geliyor?",
    answer: (
      <>
        Her konunun kendine has bir zamanlayıcısı vardır:
        <ul className="list-disc pl-5 mt-2 space-y-1">
          <li><strong>Finans:</strong> Cumartesi Sabah 10:00</li>
          <li><strong>Bilim:</strong> Cumartesi Akşam 20:00</li>
          <li><strong>Mitoloji:</strong> Pazar Sabah 10:00</li>
          <li><strong>Politika:</strong> Cuma Akşam 20:00</li>
        </ul>
      </>
    )
  }
];

export default function HelpPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [openIndex, setOpenIndex] = useState(null);

  const filteredFaqs = faqs.filter(faq => 
    faq.question.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const toggleAccordion = (index) => {
    setOpenIndex(openIndex === index ? null : index);
  };

  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />
      <motion.main
        initial={{ opacity: 0, y: 30 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: "easeOut" }}
        className="flex-1 max-w-4xl mx-auto w-full p-4 sm:p-6 md:p-12"
      >
        <div className="bg-white/80 backdrop-blur-md shadow-xl border border-white/60 rounded-3xl p-6 sm:p-8 md:p-12">
          <h1 className="text-3xl md:text-4xl font-extrabold text-text mb-2 drop-shadow-sm">Yardım Merkezi</h1>
          <p className="text-text-muted mb-8 text-base sm:text-lg font-medium">Aklınıza takılan soruların yanıtları burada.</p>

          {/* Search Bar */}
          <div className="relative mb-8">
            <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
              <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path>
              </svg>
            </div>
            <input
              type="text"
              placeholder="Sorularda Ara..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full pl-11 pr-4 py-3.5 border border-slate-200/80 rounded-2xl focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent transition-all text-text bg-white/90 shadow-xs text-sm"
            />
          </div>

          <div className="space-y-4">
            {filteredFaqs.length > 0 ? (
              filteredFaqs.map((faq, index) => (
                <div key={index} className="bg-white/90 rounded-2xl shadow-xs border border-slate-200/80 overflow-hidden transition-all duration-200">
                  <button
                    onClick={() => toggleAccordion(index)}
                    className="w-full px-6 py-4 flex items-center justify-between focus:outline-none hover:bg-slate-50 transition-colors cursor-pointer"
                  >
                    <span className="font-bold text-text text-left text-sm sm:text-base">{faq.question}</span>
                    <svg 
                      className={`w-5 h-5 text-primary transition-transform duration-300 shrink-0 ml-3 ${openIndex === index ? 'rotate-180' : ''}`} 
                      fill="none" 
                      stroke="currentColor" 
                      viewBox="0 0 24 24"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 9l-7 7-7-7"></path>
                    </svg>
                  </button>
                  <AnimatePresence>
                    {openIndex === index && (
                      <motion.div
                        initial={{ height: 0, opacity: 0 }}
                        animate={{ height: 'auto', opacity: 1 }}
                        exit={{ height: 0, opacity: 0 }}
                        transition={{ duration: 0.3, ease: "easeInOut" }}
                        className="overflow-hidden"
                      >
                        <div className="px-6 pb-5 pt-2 text-text-muted text-sm sm:text-base leading-relaxed border-t border-slate-100">
                          {faq.answer}
                        </div>
                      </motion.div>
                    )}
                  </AnimatePresence>
                </div>
              ))
            ) : (
              <div className="text-center py-8 text-text-muted">
                <p>Aradığınız kriterlere uygun soru bulunamadı.</p>
              </div>
            )}
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
