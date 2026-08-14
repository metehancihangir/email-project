import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import Navbar from '../components/Navbar';
import Logo from '../components/Logo';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5117';

export default function ArchivePage() {
  const [newsletters, setNewsletters] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState('Hepsi');
  const [searchQuery, setSearchQuery] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const categories = [
    { id: 'Hepsi', label: 'Tüm Bültenler' },
    { id: 'Mitoloji', label: 'Mitoloji' },
    { id: 'Bilim', label: 'Bilim & Teknoloji' },
    { id: 'Finans', label: 'Finans & Ekonomi' },
    { id: 'Politika', label: 'Politika & Gündem' }
  ];

  const fetchArchive = async () => {
    setLoading(true);
    try {
      const categoryParam = selectedCategory !== 'Hepsi' ? selectedCategory : '';
      const res = await axios.get(`${API_BASE_URL}/api/subscribers/archive`, {
        params: {
          category: categoryParam,
          search: searchQuery,
          page,
          pageSize: 9
        }
      });

      setNewsletters(res.data.items || []);
      setTotalPages(Math.ceil((res.data.totalCount || 0) / 9) || 1);
      setTotalCount(res.data.totalCount || 0);
    } catch (err) {
      console.error("Arşiv yüklenirken hata:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchArchive();
  }, [selectedCategory, page]);

  const handleSearchSubmit = (e) => {
    e.preventDefault();
    setPage(1);
    fetchArchive();
  };

  const getCategoryBadgeClass = (category) => {
    switch (category?.toLowerCase()) {
      case 'mitoloji': return 'bg-amber-50 text-amber-800 border-amber-200';
      case 'finans': return 'bg-emerald-50 text-emerald-800 border-emerald-200';
      case 'bilim': return 'bg-blue-50 text-blue-800 border-blue-200';
      case 'politika': return 'bg-purple-50 text-purple-800 border-purple-200';
      default: return 'bg-slate-50 text-slate-700 border-slate-200';
    }
  };

  const formatDate = (dateStr) => {
    try {
      const d = new Date(dateStr);
      return new Intl.DateTimeFormat('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' }).format(d);
    } catch {
      return dateStr;
    }
  };

  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />

      <motion.main 
        initial={{ opacity: 0, y: 25 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: "easeOut" }}
        className="flex-1 max-w-7xl mx-auto w-full p-4 md:p-10 space-y-8"
      >
        {/* Hero & Arama Kartı (Glassmorphism) */}
        <div className="bg-white/80 backdrop-blur-md shadow-xl border border-white/60 rounded-3xl p-8 md:p-12 text-center space-y-5">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-primary/10 text-primary text-xs font-bold uppercase tracking-wider">
            Bilgi Kütüphanesi
          </div>

          <h1 className="text-3xl md:text-5xl font-extrabold text-text tracking-tight leading-tight">
            Geçmiş Bülten <span className="text-primary">Arşivi</span>
          </h1>

          <p className="text-text-muted max-w-2xl mx-auto text-base md:text-lg leading-relaxed">
            Yapay zeka tarafından özenle derlenmiş mitoloji, bilim, finans ve politika bültenlerimizin tamamını inceleyin.
          </p>

          {/* Arama Formu */}
          <form 
            onSubmit={handleSearchSubmit}
            className="max-w-xl mx-auto mt-6 flex gap-2"
          >
            <div className="relative flex-1">
              <input
                type="text"
                placeholder="Bültenlerde veya başlıklarda ara..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-11 pr-4 py-3.5 bg-white/90 border border-slate-200/80 rounded-2xl text-text text-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent transition-all shadow-xs"
              />
              <svg className="w-5 h-5 text-text-muted absolute left-3.5 top-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
            <button
              type="submit"
              className="px-7 py-3.5 bg-primary text-white rounded-2xl font-bold text-sm hover:bg-primary-hover transition-colors shadow-md shadow-primary/25 cursor-pointer active:scale-95"
            >
              Ara
            </button>
          </form>

          {/* Kategori Filtre Butonları */}
          <div className="flex items-center justify-center gap-2 overflow-x-auto pt-4 pb-1 scrollbar-none flex-wrap">
            {categories.map((cat) => (
              <button
                key={cat.id}
                onClick={() => {
                  setSelectedCategory(cat.id);
                  setPage(1);
                }}
                className={`px-4 py-2 rounded-xl text-xs md:text-sm font-semibold transition-all whitespace-nowrap cursor-pointer ${
                  selectedCategory === cat.id
                    ? 'bg-primary text-white shadow-md shadow-primary/20 scale-105'
                    : 'bg-white/90 text-text-muted border border-slate-200/80 hover:bg-white hover:text-text'
                }`}
              >
                {cat.label}
              </button>
            ))}
          </div>
        </div>

        {/* Bülten Kartları Listesi */}
        <div>
          {loading ? (
            <div className="bg-white/70 backdrop-blur-md rounded-3xl p-16 text-center space-y-4 border border-white/50 shadow-sm">
              <div className="w-10 h-10 border-3 border-primary/20 border-t-primary rounded-full animate-spin mx-auto"></div>
              <p className="text-sm text-text-muted font-medium">Bültenler yükleniyor...</p>
            </div>
          ) : newsletters.length === 0 ? (
            <div className="bg-white/80 backdrop-blur-md rounded-3xl border border-white/60 p-12 text-center max-w-md mx-auto my-6 shadow-xl space-y-4">
              <div className="w-16 h-16 bg-primary-light rounded-full flex items-center justify-center mx-auto text-primary">
                <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                </svg>
              </div>
              <h3 className="font-bold text-text text-lg">Bülten Bulunamadı</h3>
              <p className="text-text-muted text-sm leading-relaxed">
                Seçilen kriterlere uygun arşivlenmiş bülten bulunamadı. Lütfen filtreyi veya arama terimini değiştirin.
              </p>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                <AnimatePresence>
                  {newsletters.map((newsletter, idx) => (
                    <motion.article
                      key={newsletter.id}
                      initial={{ opacity: 0, y: 20 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: idx * 0.05 }}
                      whileHover={{ y: -5 }}
                      className="bg-white/90 backdrop-blur-md rounded-3xl border border-white/80 overflow-hidden shadow-lg hover:shadow-2xl transition-all flex flex-col group"
                    >
                      {/* Kapak Görseli */}
                      <div className="h-48 bg-slate-100 relative overflow-hidden">
                        {newsletter.coverImageUrl ? (
                          <img 
                            src={newsletter.coverImageUrl} 
                            alt={newsletter.subject} 
                            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
                            loading="lazy"
                          />
                        ) : (
                          <div className="w-full h-full flex items-center justify-center bg-gradient-to-tr from-blue-50 to-indigo-50 text-slate-400">
                            <Logo variant="icon" className="w-12 h-12 opacity-30" />
                          </div>
                        )}
                        
                        {/* Kategori Rozeti */}
                        <span className={`absolute top-3 left-3 px-3 py-1 rounded-xl text-xs font-bold border backdrop-blur-md shadow-xs ${getCategoryBadgeClass(newsletter.category)}`}>
                          {newsletter.category || 'Genel'}
                        </span>
                      </div>

                      {/* İçerik */}
                      <div className="p-6 flex-1 flex flex-col justify-between space-y-4">
                        <div className="space-y-2.5">
                          <div className="flex items-center justify-between text-xs text-text-muted">
                            <span>{formatDate(newsletter.sentAt)}</span>
                            {newsletter.likesCount > 0 && (
                              <span className="flex items-center gap-1 text-emerald-600 font-semibold bg-emerald-50 px-2 py-0.5 rounded-md border border-emerald-100">
                                👍 {newsletter.likesCount}
                              </span>
                            )}
                          </div>
                          <h2 className="font-extrabold text-text text-lg leading-snug line-clamp-2 group-hover:text-primary transition-colors">
                            <Link to={`/arsiv/${newsletter.id}`}>
                              {newsletter.subject}
                            </Link>
                          </h2>
                          <p className="text-text-muted text-sm line-clamp-3 leading-relaxed">
                            {newsletter.excerpt}
                          </p>
                        </div>

                        <div className="pt-3 border-t border-slate-100 flex items-center justify-between">
                          <Link
                            to={`/arsiv/${newsletter.id}`}
                            className="text-primary font-bold text-sm hover:text-primary-hover flex items-center gap-1.5 transition-colors"
                          >
                            Bülteni Oku
                            <svg className="w-4 h-4 group-hover:translate-x-1 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 5l7 7-7 7" />
                            </svg>
                          </Link>
                        </div>
                      </div>
                    </motion.article>
                  ))}
                </AnimatePresence>
              </div>

              {/* Sayfalama */}
              {totalPages > 1 && (
                <div className="flex justify-center items-center gap-2 mt-10 mb-6">
                  <button
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={page === 1}
                    className="px-5 py-2.5 bg-white/90 backdrop-blur-md border border-slate-200/80 rounded-xl text-sm font-semibold text-text disabled:opacity-40 hover:bg-white transition-colors cursor-pointer shadow-xs"
                  >
                    ← Önceki
                  </button>
                  <span className="text-sm font-bold text-text-muted px-4 bg-white/70 backdrop-blur-md py-2 rounded-xl border border-white/60">
                    Sayfa {page} / {totalPages}
                  </span>
                  <button
                    onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    disabled={page === totalPages}
                    className="px-5 py-2.5 bg-white/90 backdrop-blur-md border border-slate-200/80 rounded-xl text-sm font-semibold text-text disabled:opacity-40 hover:bg-white transition-colors cursor-pointer shadow-xs"
                  >
                    Sonraki →
                  </button>
                </div>
              )}
            </>
          )}

          {/* CTA Banner (Glassmorphism & Project Theme) */}
          <div className="mt-12 bg-white/80 backdrop-blur-md rounded-3xl p-8 md:p-10 shadow-xl border border-white/60 flex flex-col md:flex-row items-center justify-between gap-6">
            <div className="flex flex-col md:flex-row items-center md:items-start gap-5 text-center md:text-left max-w-2xl">
              <div className="w-14 h-14 rounded-2xl bg-primary-light text-primary flex items-center justify-center shrink-0 shadow-xs">
                <svg className="w-7 h-7" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                </svg>
              </div>
              <div className="space-y-1.5">
                <h3 className="text-2xl font-extrabold text-text tracking-tight">
                  Yeni Bültenleri <span className="text-primary">Kaçırmayın!</span>
                </h3>
                <p className="text-text-muted text-sm md:text-base leading-relaxed">
                  İlgi alanlarınıza özel hazırlanan haftalık yapay zeka özetlerini doğrudan e-posta kutunuzda alın. Tamamen ücretsiz.
                </p>
              </div>
            </div>
            <Link
              to="/"
              className="px-8 py-3.5 bg-primary text-white hover:bg-primary-hover rounded-2xl font-extrabold text-sm shadow-lg shadow-primary/25 transition-all transform hover:scale-105 whitespace-nowrap cursor-pointer"
            >
              Hemen Ücretsiz Abone Ol
            </Link>
          </div>
        </div>
      </motion.main>

      {/* Footer */}
      <footer className="py-6 text-center text-xs text-text-muted">
        <p>&copy; {new Date().getFullYear()} SUBMAIL. Tüm hakları saklıdır.</p>
      </footer>
    </div>
  );
}
