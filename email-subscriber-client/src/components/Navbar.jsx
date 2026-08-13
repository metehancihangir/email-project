import { Link, useLocation } from 'react-router-dom';
import Logo from './Logo';

export default function Navbar() {
  const location = useLocation();

  const getLinkClass = (path) => {
    const isActive = location.pathname === path;
    return `text-sm font-medium transition-colors ${
      isActive 
        ? 'text-primary border-b-2 border-primary pb-1' 
        : 'text-text-muted hover:text-primary'
    }`;
  };

  return (
    <nav className="flex items-center justify-between py-6 px-6 md:px-12 bg-transparent">
        <div className="flex items-center">
          <Link to="/">
            <Logo variant="full" className="h-8 md:h-10 cursor-pointer" />
          </Link>
        </div>
        <div className="flex items-center gap-6">
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
