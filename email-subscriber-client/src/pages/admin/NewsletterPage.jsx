import React, { useState, useRef, useMemo, useCallback, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import ReactQuill from 'react-quill-new';
import 'react-quill-new/dist/quill.snow.css';
import api from '../../api/axiosInstance';
import { motion } from 'framer-motion';
import DOMPurify from 'dompurify';

export default function NewsletterPage() {
  const location = useLocation();
  const [subject, setSubject] = useState('');
  const [htmlBody, setHtmlBody] = useState('');
  const [category, setCategory] = useState('');
  const [coverImageUrl, setCoverImageUrl] = useState('');
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState(null);
  const [uploadedImages, setUploadedImages] = useState([]);

  const quillRef = useRef(null);

  // Yapay Zeka Stüdyosu'ndan aktarılan verileri doldur
  useEffect(() => {
    if (location.state) {
      if (location.state.initialSubject) setSubject(location.state.initialSubject);
      if (location.state.initialHtml) setHtmlBody(location.state.initialHtml);
      if (location.state.initialCategory) setCategory(location.state.initialCategory);
      if (location.state.initialCoverImage) setCoverImageUrl(location.state.initialCoverImage);
      
      setToast({
        type: 'success',
        message: '✨ Yapay Zeka taslağı editöre başarıyla aktarıldı. İnceleyip düzenleyebilirsiniz.'
      });
    }
  }, [location.state]);

  const imageHandler = useCallback(() => {
    const input = document.createElement('input');
    input.setAttribute('type', 'file');
    input.setAttribute('accept', 'image/*');
    input.click();

    input.onchange = async () => {
      const file = input.files[0];
      if (file) {
        const formData = new FormData();
        formData.append('file', file);
        try {
          const res = await api.post('/api/admin/images/upload', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
          });
          const url = res.data.url;
          
          const quill = quillRef.current.getEditor();
          const range = quill.getSelection(true);
          quill.insertEmbed(range.index, 'image', url);
          quill.formatText(range.index, 1, 'width', '100%');
          quill.insertText(range.index + 1, '\n');
          quill.setSelection(range.index + 2);

          setUploadedImages(prev => [...prev, url]);
        } catch (err) {
          console.error(err);
          setToast({ type: 'error', message: 'Resim yüklenirken hata oluştu.' });
        }
      }
    };
  }, []);

  const modules = useMemo(() => ({
    toolbar: {
      container: [
        [{ 'header': [1, 2, 3, false] }],
        ['bold', 'italic', 'underline', 'strike'],
        [{ 'list': 'ordered'}, { 'list': 'bullet' }],
        ['link', 'image'],
        ['clean']
      ],
      handlers: {
        image: imageHandler
      }
    }
  }), [imageHandler]);

  const handleSend = async () => {
    if (!subject || !htmlBody) return;
    
    setLoading(true);
    setToast(null);

    try {
      await api.post('/api/admin/newsletter', { 
        subject, 
        htmlBody,
        category: category || null,
        coverImageUrl: coverImageUrl || null
      });
      setToast({ type: 'success', message: 'Bülten başarıyla kuyruğa alındı ve gönderilmeye başlandı!' });
      setSubject('');
      setHtmlBody('');
      setCategory('');
      setCoverImageUrl('');
      setUploadedImages([]);
    } catch (err) {
      console.error(err);
      setToast({ type: 'error', message: 'Bülten gönderilirken bir hata oluştu.' });
    } finally {
      setLoading(false);
    }
  };

  const handleEditorChange = (content) => {
    setHtmlBody(content);
    
    uploadedImages.forEach(async (url) => {
      if (!content.includes(url)) {
        try {
          const fileName = url.split('/').pop();
          await api.delete(`/api/admin/images/${fileName}`);
          setUploadedImages(prev => prev.filter(imgUrl => imgUrl !== url));
        } catch (error) {
          console.error("Resim silinirken hata:", error);
        }
      }
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Bülten Oluştur & Düzenle</h1>
          <p className="text-slate-500 text-sm mt-0.5">
            Zengin metin editörünü kullanarak abonelerinize özel e-posta bültenleri hazırlayın.
          </p>
        </div>
      </div>

      {toast && (
        <motion.div
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className={`p-4 rounded-xl text-sm font-medium border ${
            toast.type === 'success' 
              ? 'bg-emerald-50 text-emerald-800 border-emerald-200' 
              : 'bg-red-50 text-red-800 border-red-200'
          }`}
        >
          {toast.message}
        </motion.div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        
        {/* Sol Panel: Editör */}
        <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="md:col-span-2">
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">
                Konu Başlığı (Subject) *
              </label>
              <input
                type="text"
                value={subject}
                onChange={(e) => setSubject(e.target.value)}
                className="w-full px-4 py-2.5 rounded-xl border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none text-sm text-slate-800"
                placeholder="Örn: Haftanın Finans Özeti..."
              />
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">
                Kategori
              </label>
              <select
                value={category}
                onChange={(e) => setCategory(e.target.value)}
                className="w-full px-3 py-2.5 rounded-xl border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none text-sm text-slate-800 bg-white cursor-pointer"
              >
                <option value="">Seçiniz</option>
                <option value="Mitoloji">🏛️ Mitoloji</option>
                <option value="Bilim">🔬 Bilim</option>
                <option value="Finans">📈 Finans</option>
                <option value="Politika">🌍 Politika</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">
              Kapak Görseli URL (İsteğe Bağlı)
            </label>
            <input
              type="url"
              value={coverImageUrl}
              onChange={(e) => setCoverImageUrl(e.target.value)}
              className="w-full px-4 py-2.5 rounded-xl border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none text-sm text-slate-800"
              placeholder="https://images.unsplash.com/..."
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">
              İçerik (HTML) *
            </label>
            <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
               <ReactQuill 
                  ref={quillRef}
                  theme="snow" 
                  value={htmlBody} 
                  onChange={handleEditorChange} 
                  modules={modules}
                  className="newsletter-editor"
               />
            </div>
          </div>

          {uploadedImages.length > 0 && (
            <div className="mt-4 p-4 bg-gray-50 rounded-xl border border-gray-100">
              <label className="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-3">
                Eklenen Görseller (Silmek için üzerine gelin)
              </label>
              <div className="flex flex-wrap gap-3">
                {uploadedImages.map((url) => (
                  <div key={url} className="relative group w-16 h-16 rounded-xl border border-gray-200 overflow-hidden bg-white shadow-2xs">
                    <img src={url} alt="Uploaded" className="w-full h-full object-cover" />
                    <button
                      onClick={() => {
                        const fileName = url.split('/').pop();
                        api.delete(`/api/admin/images/${fileName}`).catch(console.error);
                        setUploadedImages(prev => prev.filter(imgUrl => imgUrl !== url));
                        const newHtml = htmlBody.replace(new RegExp(`<img[^>]*src="${url}"[^>]*>`, 'g'), '');
                        setHtmlBody(newHtml);
                      }}
                      className="absolute top-1 right-1 bg-black/60 hover:bg-black/90 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center cursor-pointer"
                      title="Sil"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12"></path></svg>
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="pt-4">
            <button
              onClick={handleSend}
              disabled={loading || !subject || !htmlBody}
              className="w-full py-3 px-4 bg-primary hover:bg-primary-hover disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-bold rounded-xl shadow-md transition-all active:scale-98 flex justify-center items-center gap-2 cursor-pointer"
            >
              {loading ? (
                <>
                  <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                  <span>Kuyruğa Ekleniyor...</span>
                </>
              ) : (
                <>
                  <span>🚀</span>
                  <span>Bülteni Abonelere Gönder</span>
                </>
              )}
            </button>
          </div>
        </div>

        {/* Sağ Panel: Canlı Önizleme */}
        <div className="bg-white p-6 rounded-2xl border border-gray-100 shadow-sm flex flex-col h-[650px] space-y-4">
          <div className="flex items-center justify-between border-b border-gray-100 pb-3">
            <h3 className="text-xs font-bold text-slate-400 uppercase tracking-wider">Canlı Önizleme</h3>
            {category && (
              <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-100">
                {category}
              </span>
            )}
          </div>

          <div className="flex-1 overflow-y-auto bg-slate-50/70 rounded-xl p-6 border border-gray-200 space-y-4">
             {coverImageUrl && (
               <div className="rounded-xl overflow-hidden shadow-2xs border border-gray-200 max-h-48">
                 <img src={coverImageUrl} alt="Kapak" className="w-full h-full object-cover" />
               </div>
             )}
             <h2 className="font-extrabold text-xl text-slate-900">
               {subject || <span className="text-slate-300 italic">Konu başlığı buraya gelecek...</span>}
             </h2>
             <div 
                className="prose prose-slate max-w-none text-slate-800"
                dangerouslySetInnerHTML={{ __html: htmlBody ? DOMPurify.sanitize(htmlBody) : '<span class="text-slate-400 italic text-sm">İçerik önizlemesi burada görünecek...</span>' }} 
             />
          </div>
        </div>

      </div>
    </div>
  );
}
