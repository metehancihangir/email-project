import React from 'react';
import { Navigate } from 'react-router-dom';

/**
 * JWT payload'ını decode edip token'ın geçerli olup olmadığını kontrol eder.
 * Herhangi bir hata durumunda (malformed, expire vb.) false döner.
 */
function isTokenValid(token) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    // exp saniye cinsinden, Date.now() milisaniye — karşılaştırmak için 1000'e böl
    return payload.exp * 1000 > Date.now();
  } catch {
    return false;
  }
}

const ProtectedRoute = ({ children }) => {
  const token = localStorage.getItem('adminToken');

  if (!token || !isTokenValid(token)) {
    // Süresi dolmuş ya da geçersiz token varsa temizle ve login'e yönlendir
    if (token) localStorage.removeItem('adminToken');
    return <Navigate to="/admin/login" replace />;
  }

  return children;
};

export default ProtectedRoute;
