import { Link } from 'react-router-dom';
import Logo from './Logo';

export default function Navbar() {
  return (
    <nav className="flex items-center justify-between py-6 px-6 md:px-12 bg-transparent">
        <div className="flex items-center">
          <Link to="/">
            <Logo variant="full" className="h-8 md:h-10 cursor-pointer" />
          </Link>
        </div>
        <div className="flex items-center gap-6">
          <Link to="/hakkimizda" className="text-sm font-medium text-text-muted hover:text-primary transition-colors">
            Hakkımızda
          </Link>
          <Link to="/yardim" className="text-sm font-medium text-text-muted hover:text-primary transition-colors">
            Yardım
          </Link>
          <Link to="/iletisim" className="text-sm font-medium text-text-muted hover:text-primary transition-colors">
            İletişim
          </Link>
        </div>
      </nav>
  );
}
