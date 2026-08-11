/**
 * E-posta format doğrulaması
 */
export const isValidEmail = (email) =>
  /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(email).trim());

/**
 * Boş string kontrolü
 */
export const isEmpty = (value) =>
  value === null || value === undefined || String(value).trim() === '';

/**
 * Token'ı URL query param'dan al
 * Örnek: getTokenFromUrl('token') → "abc123"
 */
export const getTokenFromUrl = (param = 'token') => {
  const search = window.location.search;
  return new URLSearchParams(search).get(param);
};
