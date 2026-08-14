import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import Navbar from '../components/Navbar';
import axios from 'axios';
import DOMPurify from 'dompurify';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5117';

export default function NewsletterDetailPage() {
  const { id } = useParams();
  const [newsletter, setNewsletter] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchDetail = async () => {
      setLoading(true);
      try {
        const res = await axios.get(`${API_BASE_URL}/api/subscribers/archive/${id}`);
        setNewsletter(res.data);
      } catch (err) {
        console.error("Bülten detayı yüklenirken hata:", err);
        setError("Bülten bulunamadı veya silinmiş.");
      } finally {
        setLoading(false);
      }
    };

    fetchDetail();
  }, [id]);

  const formatDate = (dateStr) => {
    try {
      const d = new Date(dateStr);
      return new Intl.DateTimeFormat('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' }).format(d);
    } catch {
      return dateStr;
    }
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

  return (
    <div className="min-h-screen bg-auth-pattern bg-cover bg-center bg-no-repeat flex flex-col font-sans">
      <Navbar />

      <motion.main 
        initial={{ opacity: 0, y: 25 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5, ease: "easeOut" }}
        className="max-w-4xl mx-auto px-4 md:px-8 py-8 flex-1 w-full"
      >
        {/* Geri Dön Linki */}
        <div className="mb-6">
          <Link
            to="/arsiv"
            className="inline-flex items-center gap-2 text-sm font-bold text-text-muted hover:text-primary transition-colors cursor-pointer bg-white/80 backdrop-blur-md px-4 py-2 rounded-xl border border-white/60 shadow-xs"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
            </svg>
            Tüm Bülten Arşivine Dön
          </Link>
        </div>

        {loading ? (
          <div className="bg-white/80 backdrop-blur-md rounded-3xl p-16 text-center space-y-4 border border-white/50 shadow-xl">
            <div className="w-10 h-10 border-3 border-primary/20 border-t-primary rounded-full animate-spin mx-auto"></div>
            <p className="text-sm text-text-muted font-medium">Bülten yükleniyor...</p>
          </div>
        ) : error || !newsletter ? (
          <div className="bg-white/80 backdrop-blur-md rounded-3xl border border-white/60 p-12 text-center my-8 shadow-xl space-y-4">
            <div className="w-16 h-16 bg-red-50 text-red-500 rounded-full flex items-center justify-center mx-auto text-2xl">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
              </svg>
            </div>
            <h3 className="font-bold text-text text-lg">{error || "Bülten Bulunamadı"}</h3>
            <Link
              to="/arsiv"
              className="inline-block px-6 py-2.5 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary-hover transition-colors"
            >
              Arşive Göz At
            </Link>
          </div>
        ) : (
          <article className="bg-white/85 backdrop-blur-md rounded-3xl border border-white/70 shadow-2xl overflow-hidden p-6 md:p-12 space-y-8">
            {/* Üst Bilgi */}
            <div className="space-y-4 border-b border-slate-100 pb-6">
              <div className="flex items-center justify-between gap-4">
                <span className={`px-3.5 py-1 rounded-xl text-xs font-bold border backdrop-blur-md shadow-2xs ${getCategoryBadgeClass(newsletter.category)}`}>
                  {newsletter.category || 'Genel'}
                </span>
                <span className="text-xs text-text-muted font-medium">
                  {formatDate(newsletter.sentAt)}
                </span>
              </div>

              <h1 className="text-2xl md:text-4xl font-extrabold text-text leading-tight tracking-tight">
                {newsletter.subject}
              </h1>

              {newsletter.likesCount > 0 && (
                <div className="inline-flex items-center gap-1.5 px-3.5 py-1 bg-emerald-50 text-emerald-700 text-xs font-semibold rounded-full border border-emerald-200">
                  <span>👍 {newsletter.likesCount} okur bu bülteni faydalı buldu</span>
                </div>
              )}
            </div>

            {/* Kapak Görseli */}
            {newsletter.coverImageUrl && (
              <div className="rounded-2xl overflow-hidden shadow-md max-h-[420px] border border-slate-100">
                <img 
                  src={newsletter.coverImageUrl} 
                  alt={newsletter.subject} 
                  className="w-full h-full object-cover"
                />
              </div>
            )}

            {/* Bülten HTML Gövdesi */}
            <div 
              className="prose prose-slate max-w-none text-slate-800 leading-relaxed font-sans"
              dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(newsletter.htmlBody || '') }}
            />

            {/* Sayfa Sonu CTA */}
            <div className="pt-8 border-t border-slate-100 text-center space-y-3">
              <p className="text-sm font-bold text-text">
                Bu tarz bültenleri her hafta e-posta kutunuzda almak ister misiniz?
              </p>
              <Link
                to="/"
                className="inline-block px-8 py-3.5 bg-primary text-white rounded-2xl text-sm font-extrabold shadow-lg shadow-primary/20 hover:bg-primary-hover transition-all transform hover:scale-105"
              >
                Hemen Ücretsiz Abone Olun
              </Link>
            </div>
          </article>
        )}
      </motion.main>

      {/* Footer */}
      <footer className="py-6 text-center text-xs text-text-muted">
        <p>&copy; {new Date().getFullYear()} SUBMAIL. Tüm hakları saklıdır.</p>
      </footer>
    </div>
  );
}
