import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import ScrollToTop from './components/ScrollToTop';
import SubscribePage from './pages/SubscribePage';
import ConfirmPage from './pages/ConfirmPage';
import UnsubscribePage from './pages/UnsubscribePage';
import PreferencesPage from './pages/PreferencesPage';
import PrivacyPolicyPage from './pages/PrivacyPolicyPage';
import HelpPage from './pages/HelpPage';
import ContactPage from './pages/ContactPage';
import AboutPage from './pages/AboutPage';
import ArchivePage from './pages/ArchivePage';
import NewsletterDetailPage from './pages/NewsletterDetailPage';
import AdminLoginPage from './pages/admin/AdminLoginPage';
import AdminLayout from './components/admin/AdminLayout';
import ProtectedRoute from './components/admin/ProtectedRoute';
import DashboardPage from './pages/admin/DashboardPage';
import SubscribersPage from './pages/admin/SubscribersPage';
import NewsletterPage from './pages/admin/NewsletterPage';
import CampaignsPage from './pages/admin/CampaignsPage';
import AIManagerPage from './pages/admin/AIManagerPage';

export default function App() {
  return (
    <BrowserRouter>
      <ScrollToTop />
      <Routes>
        {/* ─── Public Routes ─── */}
        <Route path="/" element={<SubscribePage />} />
        <Route path="/confirm" element={<ConfirmPage />} />
        <Route path="/unsubscribe" element={<UnsubscribePage />} />
        <Route path="/preferences" element={<PreferencesPage />} />
        <Route path="/arsiv" element={<ArchivePage />} />
        <Route path="/arsiv/:id" element={<NewsletterDetailPage />} />
        <Route path="/gizlilik-politikasi" element={<PrivacyPolicyPage />} />
        <Route path="/kullanim-kosullari" element={<PrivacyPolicyPage />} />
        <Route path="/hakkimizda" element={<AboutPage />} />
        <Route path="/yardim" element={<HelpPage />} />
        <Route path="/iletisim" element={<ContactPage />} />

        {/* ─── Admin Routes ─── */}
        <Route path="/admin/login" element={<AdminLoginPage />} />
        <Route
          path="/admin"
          element={
            <ProtectedRoute>
              <AdminLayout />
            </ProtectedRoute>
          }
        >
          <Route index element={<DashboardPage />} />
          <Route path="subscribers" element={<SubscribersPage />} />
          <Route path="newsletter" element={<NewsletterPage />} />
          <Route path="campaigns" element={<CampaignsPage />} />
          <Route path="ai-manager" element={<AIManagerPage />} />
        </Route>

        {/* ─── Fallback ─── */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
