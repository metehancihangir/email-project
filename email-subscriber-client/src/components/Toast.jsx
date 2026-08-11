import { motion, AnimatePresence } from 'framer-motion';
import { useEffect } from 'react';

// type: 'success' | 'error' | 'info' | 'warning'
export default function Toast({ type, message, onClose }) {
  useEffect(() => {
    if (message) {
      const timer = setTimeout(() => {
        onClose();
      }, 4000);
      return () => clearTimeout(timer);
    }
  }, [message, onClose]);

  const bgColors = {
    success: 'bg-green-100 border-green-400 text-green-700',
    error: 'bg-red-100 border-red-400 text-red-700',
    info: 'bg-blue-100 border-blue-400 text-blue-700',
    warning: 'bg-yellow-100 border-yellow-400 text-yellow-700',
  };

  const currentBg = bgColors[type] || bgColors.info;

  return (
    <AnimatePresence>
      {message && (
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -20 }}
          className={`fixed top-5 left-1/2 -translate-x-1/2 px-4 py-3 rounded border shadow-md z-50 min-w-[300px] text-center ${currentBg}`}
          role="alert"
        >
          <span className="block sm:inline">{message}</span>
          <button
            onClick={onClose}
            className="absolute top-0 bottom-0 right-0 px-4 py-3 focus:outline-none"
          >
            <span className="text-xl">&times;</span>
          </button>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
