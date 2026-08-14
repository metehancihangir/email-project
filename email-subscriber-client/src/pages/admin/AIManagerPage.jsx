import React, { useState } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useNavigate } from 'react-router-dom';
import api from '../../api/axiosInstance';
import DOMPurify from 'dompurify';

export default function AIManagerPage() {
  const navigate = useNavigate();
  const [generating, setGenerating] = useState(false);
  const [triggering, setTriggering] = useState(false);
  const [sendingTest, setSendingTest] = useState(false);
  
  // Modallar
  const [showConfirm, setShowConfirm] = useState(false);
  const [showPreviewModal, setShowPreviewModal] = useState(false);
  
  const [selectedCategory, setSelectedCategory] = useState(null);
  const [activeDraft, setActiveDraft] = useState(null);
  const [testEmail, setTestEmail] = useState('');
  const [toast, setToast] = useState(null);

  const categories = [
    { 
      id: 'Mitoloji', 
      name: 'Mitoloji', 
      icon: '🏛️', 
      color: 'bg-amber-100 text-amber-700 border-amber-200',
      description: 'Yunan, İskandinav ve dünya mitolojisinden büyüleyici efsaneler ve Wikipedia görsel entegrasyonu.'
    },
    { 
      id: 'Bilim', 
      name: 'Bilim & Teknoloji', 
      icon: '🔬', 
      color: 'bg-blue-100 text-blue-700 border-blue-200',
      description: 'Evrim Ağacı, Webtekno ve güncel keşiflerin kaynakça kapak görselleriyle derlenmiş özeti.'
    },
    { 
      id: 'Finans', 
      name: 'Finans & Piyasa', 
      icon: '📈', 
      color: 'bg-emerald-100 text-emerald-700 border-emerald-200',
      description: 'Bloomberg HT ve Dünya Gazetesi kaynaklı anlık borsa, kur ve makroekonomi özeti.'
    },
    { 
      id: 'Politika', 
      name: 'Politika & Dünya', 
      icon: '🌍', 
      color: 'bg-purple-100 text-purple-700 border-purple-200',
      description: 'BBC Türkçe ve TRT Haber doğrulanmış dış politika haberleri ve diplomatik gelişmeler.'
    }
  ];

  const showToastMessage = (type, message) => {
    setToast({ type, message });
    setTimeout(() => setToast(null), 5000);
  };

  // 1. Taslak Üret
  const handleGenerateDraft = async (category) => {
    setSelectedCategory(category);
    setGenerating(true);
    try {
      const res = await api.post(`/api/admin/generate-draft?category=${encodeURIComponent(category.id)}`);
      setActiveDraft(res.data);
      setShowPreviewModal(true);
      showToastMessage('success', `${category.name} kategorisinde taslak başarıyla oluşturuldu!`);
    } catch (err) {
      console.error(err);
      const timeout = err.code === 'ECONNABORTED';
      showToastMessage(
        'error',
        timeout
          ? 'Taslak üretimi zaman aşımına uğradı. API çalışıyor mu ve tekrar deneyin.'
          : (err.response?.data?.message || 'Yapay zeka taslağı üretilirken bir hata oluştu.')
      );
    } finally {
      setGenerating(false);
    }
  };

  // 2. Doğrudan Gönderim Tıklama
  const handleDirectTriggerClick = (category) => {
    setSelectedCategory(category);
    setShowConfirm(true);
  };

  // 3. Doğrudan Gönderimi Onayla
  const confirmDirectTrigger = async () => {
    setTriggering(true);
    try {
      await api.post(`/api/admin/trigger-ai?category=${selectedCategory.id}`);
      setShowConfirm(false);
      showToastMessage('success', `${selectedCategory.name} kategorisinde bülten kuyruğa alındı! Arka planda abonelere ulaştırılacak.`);
    } catch (err) {
      console.error(err);
      showToastMessage('error', 'Yapay zeka tetiklenirken bir hata oluştu.');
      setShowConfirm(false);
    } finally {
      setTriggering(false);
    }
  };

  // 4. Test E-postası Gönder
  const handleSendTest = async () => {
    if (!testEmail || !activeDraft) {
      alert('Lütfen geçerli bir test e-posta adresi girin.');
      return;
    }

    setSendingTest(true);
    try {
      await api.post('/api/admin/send-test', {
        targetEmail: testEmail,
        subject: activeDraft.subject,
        htmlBody: activeDraft.htmlBody,
        category: activeDraft.category,
        coverImageUrl: activeDraft.coverImageUrl
      });
      showToastMessage('success', `Test e-postası ${testEmail} adresine başarıyla gönderildi!`);
    } catch (err) {
      console.error(err);
      showToastMessage('error', 'Test e-postası gönderilirken hata oluştu.');
    } finally {
      setSendingTest(false);
    }
  };

  // 5. Editöre Aktar
  const handleTransferToEditor = () => {
    if (!activeDraft) return;
    navigate('/admin/newsletter', {
      state: {
        initialSubject: activeDraft.subject,
        initialHtml: activeDraft.htmlBody,
        initialCategory: activeDraft.category,
        initialCoverImage: activeDraft.coverImageUrl
      }
    });
  };

  // 6. Taslağı Doğrudan Kategori Abonelerine Yayınla
  const handlePublishDraft = async () => {
    if (!activeDraft) return;
    if (!window.confirm(`Bu bülteni onaylayıp ${activeDraft.category ? `"${activeDraft.category}" kategorisini seçmiş olan` : 'tüm'} aktif abonelere göndermek istediğinize emin misiniz?`)) return;

    setTriggering(true);
    try {
      await api.post('/api/admin/newsletter', {
        subject: activeDraft.subject,
        htmlBody: activeDraft.htmlBody,
        category: activeDraft.category,
        coverImageUrl: activeDraft.coverImageUrl
      });
      setShowPreviewModal(false);
      showToastMessage('success', 'Bülten başarıyla kuyruğa alındı ve abonelere dağıtılmaya başlandı!');
    } catch (err) {
      console.error(err);
      showToastMessage('error', 'Bülten yayınlanırken hata oluştu.');
    } finally {
      setTriggering(false);
    }
  };

  return (
    <div className="space-y-6">
      {generating && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/40 backdrop-blur-sm">
          <div className="bg-white rounded-2xl shadow-xl px-8 py-6 flex flex-col items-center gap-3 max-w-sm mx-4">
            <div className="w-10 h-10 border-4 border-primary/20 border-t-primary rounded-full animate-spin"></div>
            <p className="font-semibold text-slate-900">Taslak üretiliyor</p>
            <p className="text-sm text-slate-500 text-center">
              {selectedCategory?.name || 'Seçilen kategori'} için içerik hazırlanıyor. Bu işlem birkaç saniye sürebilir.
            </p>
          </div>
        </div>
      )}
      {/* Üst Başlık */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Yapay Zeka Stüdyosu</h1>
          <p className="text-slate-500 text-sm mt-1">
            Wikipedia ve kaynakça görselleriyle zenginleştirilmiş akıllı bültenler üretin, önizleyin ve test edin.
          </p>
        </div>
      </div>

      {/* Toast Bildirimi */}
      <AnimatePresence>
        {toast && (
          <motion.div
            initial={{ opacity: 0, y: -10 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -10 }}
            className={`fixed top-4 right-4 z-[70] p-4 rounded-xl text-sm font-medium border flex items-center gap-3 shadow-lg max-w-md ${
              toast.type === 'success' 
                ? 'bg-emerald-50 text-emerald-800 border-emerald-200' 
                : 'bg-red-50 text-red-800 border-red-200'
            }`}
          >
            <span>{toast.type === 'success' ? '✅' : '⚠️'}</span>
            <span>{toast.message}</span>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Kategori Kartları */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {categories.map((cat) => (
          <motion.div 
            key={cat.id}
            whileHover={{ y: -3 }}
            className="bg-white rounded-2xl p-6 border border-gray-100 shadow-sm flex flex-col justify-between space-y-6"
          >
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <div className={`w-12 h-12 rounded-xl flex items-center justify-center text-2xl border ${cat.color}`}>
                  {cat.icon}
                </div>
                <span className="text-xs font-semibold text-slate-400 uppercase tracking-wider">
                  Otomatik Zamanlı
                </span>
              </div>
              <h3 className="font-bold text-lg text-slate-900">{cat.name}</h3>
              <p className="text-slate-500 text-sm leading-relaxed">
                {cat.description}
              </p>
            </div>

            <div className="flex flex-col sm:flex-row gap-2 pt-2 border-t border-gray-50">
              <button
                onClick={() => handleGenerateDraft(cat)}
                disabled={generating || triggering}
                className="flex-1 py-2.5 px-4 bg-primary text-white rounded-xl font-semibold text-sm hover:bg-primary-hover transition-colors flex items-center justify-center gap-2 cursor-pointer shadow-xs disabled:opacity-50"
              >
                {generating && selectedCategory?.id === cat.id ? (
                  <>
                    <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                    <span>Üretiliyor...</span>
                  </>
                ) : (
                  <>
                    <span>✨</span>
                    <span>Taslak Üret & İncele</span>
                  </>
                )}
              </button>
              
              <button
                onClick={() => handleDirectTriggerClick(cat)}
                disabled={generating || triggering}
                className="py-2.5 px-4 bg-gray-50 text-slate-700 border border-gray-200 rounded-xl font-medium text-sm hover:bg-gray-100 transition-colors cursor-pointer disabled:opacity-50"
                title="Taslağı görmeden doğrudan arka planda tüm abonelere gönderir"
              >
                ⚡ Hızlı Gönder
              </button>
            </div>
          </motion.div>
        ))}
      </div>

      {/* CANLI ÖNİZLEME MODALI */}
      <AnimatePresence>
        {showPreviewModal && activeDraft && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-xs overflow-y-auto"
          >
            <motion.div
              initial={{ scale: 0.95, y: 20 }}
              animate={{ scale: 1, y: 0 }}
              exit={{ scale: 0.95, y: 20 }}
              className="bg-white rounded-3xl shadow-2xl w-full max-w-4xl max-h-[90vh] flex flex-col overflow-hidden my-auto border border-gray-100"
            >
              {/* Modal Üst Çubuk */}
              <div className="p-6 border-b border-gray-100 flex items-center justify-between bg-slate-50">
                <div className="flex items-center gap-3">
                  <span className="text-2xl">✨</span>
                  <div>
                    <h2 className="font-bold text-lg text-slate-900">Yapay Zeka Bülten Önizlemesi</h2>
                    <span className="text-xs text-slate-500 font-medium">
                      Kategori: <b className="text-primary">{activeDraft.category}</b>
                    </span>
                  </div>
                </div>
                <button
                  onClick={() => setShowPreviewModal(false)}
                  className="w-8 h-8 rounded-full bg-white border border-gray-200 text-slate-400 hover:text-slate-700 flex items-center justify-center transition-colors cursor-pointer"
                >
                  ✕
                </button>
              </div>

              {/* Modal Gövde (Scrollable) */}
              <div className="p-6 overflow-y-auto space-y-6 flex-1 bg-slate-50/50">
                {/* Konu Başlığı Kartı */}
                <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-2xs space-y-1">
                  <span className="text-xs font-bold text-slate-400 uppercase tracking-wider">E-Posta Konu Başlığı</span>
                  <p className="font-bold text-slate-900 text-base md:text-lg">{activeDraft.subject}</p>
                </div>

                {/* Kapak Görseli ve Kaynak Bilgisi */}
                {activeDraft.coverImageUrl && (
                  <div className="bg-white p-4 rounded-2xl border border-gray-200 shadow-2xs space-y-3">
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-bold text-slate-400 uppercase tracking-wider">
                        {activeDraft.category === 'Mitoloji' ? '🏛️ Wikipedia Görseli' : '📸 Kaynakça Kapak Görseli'}
                      </span>
                      <a 
                        href={activeDraft.coverImageUrl} 
                        target="_blank" 
                        rel="noreferrer" 
                        className="text-xs text-primary font-medium hover:underline"
                      >
                        Orijinal Görseli Aç ↗
                      </a>
                    </div>
                    <div className="rounded-xl overflow-hidden max-h-72 border border-gray-100">
                      <img 
                        src={activeDraft.coverImageUrl} 
                        alt="Bülten Kapağı" 
                        className="w-full h-full object-cover"
                      />
                    </div>
                  </div>
                )}

                {/* Canlı E-posta Şablonu Önizlemesi */}
                <div className="bg-white p-6 rounded-2xl border border-gray-200 shadow-2xs space-y-4">
                  <span className="text-xs font-bold text-slate-400 uppercase tracking-wider">Bülten İçeriği (HTML Render)</span>
                  <div 
                    className="prose prose-slate max-w-none text-slate-800 leading-relaxed font-sans border-t border-gray-100 pt-4"
                    dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(activeDraft.htmlBody) }}
                  />
                </div>

                {/* Test Maili Gönderme Paneli */}
                <div className="bg-blue-50/70 border border-blue-200 rounded-2xl p-5 space-y-3">
                  <div className="flex items-center gap-2 text-blue-900 font-bold text-sm">
                    <span>🧪</span>
                    <span>Kendine Test E-postası Gönder</span>
                  </div>
                  <p className="text-xs text-blue-700">
                    Bültenin gelen kutunuzda nasıl göründüğünü kontrol etmek için e-posta adresinizi girin:
                  </p>
                  <div className="flex gap-2">
                    <input
                      type="email"
                      placeholder="admin@orneksite.com"
                      value={testEmail}
                      onChange={(e) => setTestEmail(e.target.value)}
                      className="flex-1 px-4 py-2.5 bg-white border border-blue-200 rounded-xl text-sm text-slate-900 focus:outline-none focus:ring-2 focus:ring-primary"
                    />
                    <button
                      onClick={handleSendTest}
                      disabled={sendingTest}
                      className="px-5 py-2.5 bg-blue-600 text-white rounded-xl text-sm font-semibold hover:bg-blue-700 transition-colors flex items-center gap-2 whitespace-nowrap cursor-pointer disabled:opacity-50"
                    >
                      {sendingTest ? 'Gönderiliyor...' : 'Test Maili At'}
                    </button>
                  </div>
                </div>
              </div>

              {/* Modal Alt Aksiyonlar */}
              <div className="p-5 border-t border-gray-200 bg-white flex flex-wrap gap-3 justify-between items-center">
                <button
                  onClick={() => setShowPreviewModal(false)}
                  className="px-5 py-2.5 text-slate-600 hover:bg-slate-100 rounded-xl font-medium text-sm transition-colors cursor-pointer"
                >
                  Kapat
                </button>

                <div className="flex gap-3">
                  <button
                    onClick={handleTransferToEditor}
                    className="px-5 py-2.5 bg-slate-100 text-slate-800 hover:bg-slate-200 rounded-xl font-semibold text-sm transition-colors flex items-center gap-2 cursor-pointer"
                  >
                    <span>✏️</span>
                    <span>Editörde Düzenle</span>
                  </button>

                  <button
                    onClick={handlePublishDraft}
                    disabled={triggering}
                    className="px-6 py-2.5 bg-primary text-white hover:bg-primary-hover rounded-xl font-bold text-sm shadow-md transition-colors flex items-center gap-2 cursor-pointer disabled:opacity-50"
                  >
                    <span>🚀</span>
                    <span>{triggering ? 'Gönderiliyor...' : (activeDraft.category ? `${activeDraft.category} Abonelerine Yayınla` : 'Abonelere Yayınla')}</span>
                  </button>
                </div>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>

      {/* Hızlı Gönderim Onay Modalı */}
      <AnimatePresence>
        {showConfirm && selectedCategory && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
          >
            <motion.div
              initial={{ scale: 0.95 }}
              animate={{ scale: 1 }}
              exit={{ scale: 0.95 }}
              className="bg-white rounded-3xl shadow-xl w-full max-w-md p-6 border border-gray-100"
            >
              <div className="flex items-center gap-4 mb-4 text-amber-500">
                <span className="text-3xl">⚠️</span>
                <h2 className="text-xl font-bold text-slate-900">Otomatik Gönderim</h2>
              </div>
              
              <p className="text-slate-600 text-sm mb-6 leading-relaxed">
                Yapay zeka <strong>{selectedCategory.name}</strong> konusunda anında yeni bir bülten hazırlayacak, kapak görselini belirleyecek ve tüm aktif abonelere iletecektir. <br/><br/>
                Devam etmek istiyor musunuz?
              </p>

              <div className="flex gap-3 justify-end">
                <button
                  onClick={() => setShowConfirm(false)}
                  disabled={triggering}
                  className="px-4 py-2 text-slate-500 hover:bg-gray-100 rounded-xl font-medium text-sm transition-colors cursor-pointer"
                >
                  İptal
                </button>
                <button
                  onClick={confirmDirectTrigger}
                  disabled={triggering}
                  className="px-5 py-2.5 bg-primary text-white rounded-xl font-bold text-sm hover:bg-primary-hover transition-colors flex items-center gap-2 cursor-pointer"
                >
                  {triggering ? 'Tetikleniyor...' : 'Evet, Bülteni Gönder'}
                </button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
