import { Link } from 'react-router-dom';

export default function Navbar() {
  return (
    <nav className="flex items-center justify-between py-4 px-6 md:px-12 bg-surface shadow-sm">
      <div className="flex items-center gap-2">
        <img 
          src="/logo_icon_hd.png?v=7" 
          alt="EmailSubscriber Logo" 
          className="h-7 md:h-8 w-auto object-contain" 
        />
        <span className="font-bold text-xl text-text tracking-tight hidden sm:block">EmailSubscriber</span>
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
