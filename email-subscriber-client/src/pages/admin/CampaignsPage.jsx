import React, { useState, useEffect } from 'react';
import api from '../../api/axiosInstance';
import { motion, AnimatePresence } from 'framer-motion';

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

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-text">Kampanya Geçmişi</h1>
      </div>

      <div className="bg-surface rounded-2xl border border-gray-100 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-100 text-sm font-medium text-text-muted">
                <th className="p-4">Konu</th>
                <th className="p-4">Gönderim Tarihi</th>
                <th className="p-4">Alıcı Sayısı</th>
                <th className="p-4 text-center">Açılan Sayısı</th>
                <th className="p-4 text-center w-32">Açılma Oranı (%)</th>
                <th className="p-4 text-center">Tıklanan Sayısı</th>
                <th className="p-4 text-center w-32">Tıklanma Oranı (%)</th>
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="7" className="p-8 text-center text-gray-400">Yükleniyor...</td>
                </tr>
              ) : campaigns.length === 0 ? (
                <tr>
                  <td colSpan="7" className="p-8 text-center text-gray-400">Henüz kampanya bulunmamaktadır.</td>
                </tr>
              ) : (
                <AnimatePresence>
                  {campaigns.map((camp, index) => (
                    <motion.tr
                      key={camp.id}
                      initial={{ opacity: 0, y: 10 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: index * 0.05 }}
                      className="border-b border-gray-50 hover:bg-gray-50/50"
                    >
                      <td className="p-4">
                        <button 
                          onClick={() => handleViewContent(camp)}
                          className="font-medium text-primary hover:underline text-left"
                        >
                          {camp.subject}
                        </button>
                      </td>
                      <td className="p-4 text-gray-500 text-sm">
                        {new Date(camp.sentAt).toLocaleString('tr-TR')}
                      </td>
                      <td className="p-4">
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                          {camp.recipientCount} Kişi
                        </span>
                      </td>
                      <td className="p-4 text-center">
                        <span className="font-semibold text-text">{camp.openedCount}</span>
                      </td>
                      <td className="p-4 text-center">
                        <div className="flex flex-col gap-1 items-center">
                          <span className="text-sm font-medium text-text">{camp.openRate}%</span>
                          <div className="w-full bg-gray-200 rounded-full h-1.5">
                            <div className="bg-blue-500 h-1.5 rounded-full" style={{ width: `${camp.openRate}%` }}></div>
                          </div>
                        </div>
                      </td>
                      <td className="p-4 text-center">
                        <span className="font-semibold text-text">{camp.clickedCount}</span>
                      </td>
                      <td className="p-4 text-center">
                        <div className="flex flex-col gap-1 items-center">
                          <span className="text-sm font-medium text-text">{camp.clickRate}%</span>
                          <div className="w-full bg-gray-200 rounded-full h-1.5">
                            <div className="bg-purple-500 h-1.5 rounded-full" style={{ width: `${camp.clickRate}%` }}></div>
                          </div>
                        </div>
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
            className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
            onClick={() => setSelectedCampaign(null)}
          >
            <motion.div
              initial={{ scale: 0.95 }}
              animate={{ scale: 1 }}
              exit={{ scale: 0.95 }}
              onClick={(e) => e.stopPropagation()}
              className="bg-white rounded-2xl shadow-xl w-full max-w-3xl max-h-[90vh] flex flex-col overflow-hidden"
            >
              <div className="p-4 border-b border-gray-100 flex justify-between items-center bg-gray-50">
                <h2 className="font-semibold text-lg">{selectedCampaign.subject}</h2>
                <button 
                  onClick={() => setSelectedCampaign(null)}
                  className="p-1 hover:bg-gray-200 rounded-full transition-colors"
                >
                  <svg className="w-6 h-6 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
              <div className="flex-1 overflow-y-auto p-4 bg-gray-100 flex justify-center">
                {contentLoading ? (
                  <div className="flex items-center justify-center h-full">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
                  </div>
                ) : (
                  <div 
                    className="bg-white shadow-sm" 
                    style={{ minWidth: '600px', maxWidth: '600px', minHeight: '400px' }}
                    dangerouslySetInnerHTML={{ __html: campaignContent }} 
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
