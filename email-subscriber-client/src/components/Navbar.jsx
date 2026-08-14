import { Link, useLocation } from 'react-router-dom';
import Logo from './Logo';

export default function Navbar() {
  const location = useLocation();

  const getLinkClass = (path) => {
    const isActive = location.pathname === path;
    return `text-xs sm:text-sm font-semibold transition-colors whitespace-nowrap ${
      isActive 
        ? 'text-primary border-b-2 border-primary pb-0.5' 
        : 'text-text-muted hover:text-primary'
    }`;
  };

  return (
    <nav className="flex flex-col sm:flex-row items-center justify-between py-4 sm:py-6 px-4 sm:px-6 md:px-12 gap-3 sm:gap-0 bg-transparent max-w-7xl mx-auto w-full">
      <div className="flex items-center">
        <Link to="/">
          <Logo variant="full" className="h-8 md:h-10 cursor-pointer" />
        </Link>
      </div>
      <div className="flex items-center gap-4 sm:gap-6 overflow-x-auto max-w-full py-1 scrollbar-none">
        <Link to="/arsiv" className={getLinkClass('/arsiv')}>
          Bülten Arşivi
        </Link>
        <Link to="/hakkimizda" className={getLinkClass('/hakkimizda')}>
          Hakkımızda
        </Link>
        <Link to="/yardim" className={getLinkClass('/yardim')}>
          Yardım
        </Link>
        <Link to="/iletisim" className={getLinkClass('/iletisim')}>
          İletişim
        </Link>
      </div>
    </nav>
  );
}
