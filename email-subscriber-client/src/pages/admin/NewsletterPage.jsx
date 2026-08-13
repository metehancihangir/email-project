import React, { useState, useRef, useMemo, useCallback } from 'react';
import ReactQuill from 'react-quill-new';
import 'react-quill-new/dist/quill.snow.css';
import api from '../../api/axiosInstance';
import { motion } from 'framer-motion';

export default function NewsletterPage() {
  const [subject, setSubject] = useState('');
  const [htmlBody, setHtmlBody] = useState('');
  const [loading, setLoading] = useState(false);
  const [toast, setToast] = useState(null);
  const [uploadedImages, setUploadedImages] = useState([]);

  const quillRef = useRef(null);

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
          // E-posta uyumluluğu için resme max-width ekle
          quill.formatText(range.index, 1, 'width', '100%');

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
      await api.post('/api/admin/newsletter', { subject, htmlBody });
      setToast({ type: 'success', message: 'Bülten başarıyla kuyruğa alındı!' });
      setSubject('');
      setHtmlBody('');
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
    
    // Check for deleted images
    uploadedImages.forEach(async (url) => {
      if (!content.includes(url)) {
        // Image was deleted from editor
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
      <h1 className="text-2xl font-bold text-text">Bülten Oluştur</h1>

      {toast && (
        <motion.div
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
          className={`p-4 rounded-xl ${toast.type === 'success' ? 'bg-emerald-100 text-emerald-800' : 'bg-red-100 text-red-800'}`}
        >
          {toast.message}
        </motion.div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        
        {/* Sol Panel: Editör */}
        <div className="bg-surface p-6 rounded-2xl border border-gray-100 shadow-sm space-y-4">
          <div>
            <label className="block text-sm font-medium text-text-muted mb-1">Konu (Subject)</label>
            <input
              type="text"
              value={subject}
              onChange={(e) => setSubject(e.target.value)}
              className="w-full px-4 py-2 rounded-xl border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 outline-none transition-all"
              placeholder="E-posta konusu..."
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-text-muted mb-1">İçerik (HTML)</label>
            <div className="bg-white rounded-xl border border-gray-200">
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
              <label className="block text-sm font-medium text-text-muted mb-3">Eklenen Görseller (Silmek için üzerine gelin)</label>
              <div className="flex flex-wrap gap-4">
                {uploadedImages.map((url) => (
                  <div key={url} className="relative group w-20 h-20 rounded-xl border border-gray-200 overflow-hidden bg-white shadow-sm">
                    <img src={url} alt="Uploaded" className="w-full h-full object-cover" />
                    <button
                      onClick={() => {
                        const fileName = url.split('/').pop();
                        api.delete(`/api/admin/images/${fileName}`).catch(console.error);
                        setUploadedImages(prev => prev.filter(imgUrl => imgUrl !== url));
                        const newHtml = htmlBody.replace(new RegExp(`<img[^>]*src="${url}"[^>]*>`, 'g'), '');
                        setHtmlBody(newHtml);
                      }}
                      className="absolute top-1 right-1 bg-black/50 hover:bg-black/80 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center"
                      title="Resmi Sunucudan ve Editörden Sil"
                    >
                      <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12"></path></svg>
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="pt-8">
            <button
              onClick={handleSend}
              disabled={loading || !subject || !htmlBody}
              className="w-full py-3 px-4 bg-primary hover:bg-primary-dark disabled:bg-gray-300 disabled:cursor-not-allowed text-white font-medium rounded-xl shadow-lg shadow-primary/30 transition-all active:scale-95 flex justify-center items-center"
            >
              {loading ? (
                <svg className="animate-spin h-5 w-5 text-white" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
              ) : 'Gönder'}
            </button>
          </div>
        </div>

        {/* Sağ Panel: Canlı Önizleme */}
        <div className="bg-surface p-6 rounded-2xl border border-gray-100 shadow-sm flex flex-col h-[500px]">
          <h3 className="text-sm font-medium text-text-muted mb-4 border-b border-gray-100 pb-2">Canlı Önizleme</h3>
          <div className="flex-1 overflow-y-auto bg-gray-50 rounded-xl p-4 border border-gray-200">
             <div 
                className="prose max-w-none text-gray-800"
                dangerouslySetInnerHTML={{ __html: htmlBody || '<span class="text-gray-400 italic">İçerik önizlemesi burada görünecek...</span>' }} 
             />
          </div>
        </div>

      </div>
    </div>
  );
}
