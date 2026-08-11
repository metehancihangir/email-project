import { Navigate } from 'react-router-dom';

/**
 * Admin sayfalarını korur.
 * JWT token localStorage'da yoksa /admin/login'e yönlendirir.
 */
export default function ProtectedRoute({ children }) {
  const token = localStorage.getItem('adminToken');
  if (!token) {
    return <Navigate to="/admin/login" replace />;
  }
  return children;
}
