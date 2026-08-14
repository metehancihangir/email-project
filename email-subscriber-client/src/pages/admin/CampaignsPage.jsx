import React, { useState, useEffect } from 'react';
import api from '../../api/axiosInstance';
import { motion, AnimatePresence } from 'framer-motion';
import DOMPurify from 'dompurify';

export default function CampaignsPage() {
  const [campaigns, setCampaigns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedCampaign, setSelectedCampaign] = useState(null);
  const [campaignContent, setCampaignContent] = useState('');
  const [contentLoading, setContentLoading] = useState(false);

  useEffect(() => {
    const fetchCampaigns = async () => {
      try {
        const res = await api.get('/api/admin/campaigns');
        setCampaigns(res.data);
      } catch (err) {
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchCampaigns();
  }, []);

  const handleViewContent = async (campaign) => {
    setSelectedCampaign(campaign);
    setContentLoading(true);
    setCampaignContent('');
    try {
      const res = await api.get(`/api/admin/campaigns/${campaign.id}/content`);
      setCampaignContent(res.data.htmlBody || 'İçerik bulunamadı.');
    } catch (err) {
      console.error(err);
      setCampaignContent('İçerik yüklenirken bir hata oluştu.');
    } finally {
      setContentLoading(false);
    }
  };

  const getCategoryBadge = (category) => {
    switch (category?.toLowerCase()) {
      case 'mitoloji': return 'bg-amber-50 text-amber-700 border-amber-200';
      case 'finans': return 'bg-emerald-50 text-emerald-700 border-emerald-200';
      case 'bilim': return 'bg-blue-50 text-blue-700 border-blue-200';
      case 'politika': return 'bg-purple-50 text-purple-700 border-purple-200';
      default: return 'bg-gray-50 text-gray-700 border-gray-200';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Kampanya Geçmişi & Analiz</h1>
          <p className="text-slate-500 text-sm mt-0.5">
            Gönderilen tüm bültenlerin açılma, tıklanma ve okur memnuniyet oranları.
          </p>
        </div>
      </div>

      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-slate-50/80 border-b border-gray-100 text-xs font-bold text-slate-400 uppercase tracking-wider">
                <th className="p-4">Konu & Kategori</th>
                <th className="p-4">Tarih</th>
                <th className="p-4">Alıcı</th>
                <th className="p-4 text-center">Açılma</th>
                <th className="p-4 text-center">Tıklanma</th>
                <th className="p-4 text-center">Memnuniyet (👍)</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="6" className="p-8 text-center text-slate-400">Yükleniyor...</td>
                </tr>
              ) : campaigns.length === 0 ? (
                <tr>
                  <td colSpan="6" className="p-8 text-center text-slate-400">Henüz kampanya bulunmamaktadır.</td>
                </tr>
              ) : (
                <AnimatePresence>
                  {campaigns.map((camp, index) => (
                    <motion.tr
                      key={camp.id}
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: index * 0.04 }}
                      className="border-b border-gray-50 hover:bg-slate-50/50"
                    >
                      <td className="p-4">
                        <div className="space-y-1">
                          <button 
                            onClick={() => handleViewContent(camp)}
                            className="font-bold text-slate-900 hover:text-primary transition-colors text-left text-sm cursor-pointer line-clamp-1"
                          >
                            {camp.subject}
                          </button>
                          {camp.category && (
                            <span className={`inline-block px-2 py-0.5 rounded-md text-xs font-semibold border ${getCategoryBadge(camp.category)}`}>
                              {camp.category}
                            </span>
                          )}
                        </div>
                      </td>
                      <td className="p-4 text-slate-500 text-xs whitespace-nowrap">
                        {new Date(camp.sentAt).toLocaleString('tr-TR')}
                      </td>
                      <td className="p-4 whitespace-nowrap">
                        <span className="inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-semibold bg-slate-100 text-slate-700">
                          {camp.recipientCount} Kişi
                        </span>
                      </td>
                      <td className="p-4 text-center whitespace-nowrap">
                        <div className="flex flex-col items-center">
                          <span className="text-xs font-bold text-slate-800">{camp.openRate}%</span>
                          <span className="text-[10px] text-slate-400 font-medium">({camp.openedCount})</span>
                        </div>
                      </td>
                      <td className="p-4 text-center whitespace-nowrap">
                        <div className="flex flex-col items-center">
                          <span className="text-xs font-bold text-slate-800">{camp.clickRate}%</span>
                          <span className="text-[10px] text-slate-400 font-medium">({camp.clickedCount})</span>
                        </div>
                      </td>
                      <td className="p-4 text-center whitespace-nowrap">
                        {camp.totalFeedbackCount > 0 ? (
                          <div className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-emerald-50 text-emerald-700 border border-emerald-200 text-xs font-bold">
                            <span>👍 %{camp.positiveFeedbackRate}</span>
                            <span className="text-[10px] text-emerald-600 font-normal">({camp.positiveFeedbackCount}/{camp.totalFeedbackCount})</span>
                          </div>
                        ) : (
                          <span className="text-slate-300 text-xs font-medium">-</span>
                        )}
                      </td>
                    </motion.tr>
                  ))}
                </AnimatePresence>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* İçerik Modalı */}
      <AnimatePresence>
        {selectedCampaign && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-xs"
            onClick={() => setSelectedCampaign(null)}
          >
            <motion.div
              initial={{ scale: 0.95 }}
              animate={{ scale: 1 }}
              exit={{ scale: 0.95 }}
              onClick={(e) => e.stopPropagation()}
              className="bg-white rounded-3xl shadow-2xl w-full max-w-3xl max-h-[90vh] flex flex-col overflow-hidden border border-gray-100"
            >
              <div className="p-5 border-b border-gray-100 flex justify-between items-center bg-slate-50">
                <div className="space-y-0.5">
                  <h2 className="font-bold text-base text-slate-900">{selectedCampaign.subject}</h2>
                  <span className="text-xs text-slate-400">
                    Gönderim: {new Date(selectedCampaign.sentAt).toLocaleString('tr-TR')}
                  </span>
                </div>
                <button 
                  onClick={() => setSelectedCampaign(null)}
                  className="w-8 h-8 rounded-full bg-white border border-gray-200 text-slate-400 hover:text-slate-700 flex items-center justify-center transition-colors cursor-pointer"
                >
                  ✕
                </button>
              </div>
              <div className="flex-1 overflow-y-auto p-6 bg-slate-50/50 flex justify-center">
                {contentLoading ? (
                  <div className="flex items-center justify-center h-48">
                    <div className="w-8 h-8 border-3 border-primary/30 border-t-primary rounded-full animate-spin"></div>
                  </div>
                ) : (
                  <div 
                    className="bg-white rounded-2xl p-6 shadow-sm border border-gray-200 max-w-2xl w-full prose prose-slate text-slate-800"
                    dangerouslySetInnerHTML={{ __html: DOMPurify.sanitize(campaignContent) }} 
                  />
                )}
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
}
