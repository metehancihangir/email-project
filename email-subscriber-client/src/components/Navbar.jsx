import { Link } from 'react-router-dom';

export default function Navbar() {
  return (
    <nav className="flex items-center justify-between py-4 px-6 md:px-12 bg-surface shadow-sm">
      <div className="flex items-center gap-2">
        <div className="w-8 h-8 rounded bg-primary text-white flex items-center justify-center font-bold text-xl">
          E
        </div>
        <span className="font-bold text-lg text-text">EmailSubscriber</span>
      </div>
      <Link 
        to="/admin/login" 
        className="text-text-muted hover:text-primary transition-colors text-sm font-medium"
      >
        Admin Girişi
      </Link>
    </nav>
  );
}
