import React, { useState, useEffect } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import api from '../../api/axiosInstance';
import { motion } from 'framer-motion';

export default function DashboardPage() {
  const [stats, setStats] = useState({ total: 0, active: 0, unconfirmed: 0, today: 0 });
  const [growthData, setGrowthData] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const [statsRes, growthRes] = await Promise.all([
          api.get('/api/admin/stats'),
          api.get('/api/admin/subscribers/growth')
        ]);
        setStats(statsRes.data);
        setGrowthData(growthRes.data);
      } catch (err) {
        console.error("Dashboard veri hatası:", err);
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, []);

  const statCards = [
    { label: 'Toplam Abone', value: stats.total, icon: '👥', color: 'bg-blue-100 text-blue-600' },
    { label: 'Aktif Aboneler', value: stats.active, icon: '✅', color: 'bg-emerald-100 text-emerald-600' },
    { label: 'Onay Bekleyen', value: stats.unconfirmed, icon: '⏳', color: 'bg-orange-100 text-orange-600' },
    { label: 'Bugün Eklenen', value: stats.today, icon: '📈', color: 'bg-purple-100 text-purple-600' }
  ];

  if (loading) {
    return <div className="animate-pulse flex space-x-4"><div className="flex-1 space-y-4 py-1"><div className="h-4 bg-gray-200 rounded w-3/4"></div></div></div>;
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-text">Dashboard</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {statCards.map((card, idx) => (
          <motion.div
            key={card.label}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: idx * 0.1 }}
            className="bg-surface p-6 rounded-2xl border border-gray-100 shadow-sm flex items-center space-x-4"
          >
            <div className={`w-12 h-12 rounded-xl flex items-center justify-center text-xl ${card.color}`}>
              {card.icon}
            </div>
            <div>
              <p className="text-sm font-medium text-text-muted">{card.label}</p>
              <h3 className="text-2xl font-bold text-text">{card.value}</h3>
            </div>
          </motion.div>
        ))}
      </div>

      <div className="bg-surface p-6 rounded-2xl border border-gray-100 shadow-sm">
        <h3 className="text-lg font-bold text-text mb-6">Son 30 Günlük Büyüme</h3>
        <div className="h-72">
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={growthData}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
              <XAxis dataKey="date" stroke="#9ca3af" fontSize={12} tickLine={false} axisLine={false} />
              <YAxis stroke="#9ca3af" fontSize={12} tickLine={false} axisLine={false} />
              <Tooltip 
                contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.1)' }}
              />
              <Line 
                type="monotone" 
                dataKey="count" 
                stroke="#10b981" 
                strokeWidth={3}
                dot={{ fill: '#10b981', strokeWidth: 2, r: 4 }}
                activeDot={{ r: 6 }} 
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div className="bg-surface p-6 rounded-2xl border border-gray-100 shadow-sm">
        <div className="flex justify-between items-center mb-6">
          <h3 className="text-lg font-bold text-text">Son Kampanyalar (Faz 5)</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-gray-50 border-b border-gray-100 text-sm font-medium text-text-muted">
                <th className="p-4">Konu</th>
                <th className="p-4">Gönderim Tarihi</th>
                <th className="p-4">Alıcı Sayısı</th>
                <th className="p-4">Durum</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td colSpan="4" className="p-8 text-center text-gray-400">
                  Henüz kampanya bulunmamaktadır. (Faz 5'te aktifleşecek)
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
