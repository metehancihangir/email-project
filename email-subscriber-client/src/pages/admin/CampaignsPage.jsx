import React, { useState, useEffect } from 'react';
import api from '../../api/axiosInstance';
import { motion, AnimatePresence } from 'framer-motion';

export default function CampaignsPage() {
  const [campaigns, setCampaigns] = useState([]);
  const [loading, setLoading] = useState(true);

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
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan="3" className="p-8 text-center text-gray-400">Yükleniyor...</td>
                </tr>
              ) : campaigns.length === 0 ? (
                <tr>
                  <td colSpan="3" className="p-8 text-center text-gray-400">Henüz kampanya bulunmamaktadır.</td>
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
                      <td className="p-4 font-medium text-text">{camp.subject}</td>
                      <td className="p-4 text-gray-500 text-sm">
                        {new Date(camp.sentAt).toLocaleString('tr-TR')}
                      </td>
                      <td className="p-4">
                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                          {camp.recipientCount} Kişi
                        </span>
                      </td>
                    </motion.tr>
                  ))}
                </AnimatePresence>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
