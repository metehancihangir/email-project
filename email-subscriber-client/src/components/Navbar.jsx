import { useState } from 'react';
import { Link } from 'react-router-dom';
import Logo from './Logo';
import AboutModal from './AboutModal';

export default function Navbar() {
  const [isAboutOpen, setIsAboutOpen] = useState(false);

  return (
    <>
      <nav className="flex items-center justify-between py-6 px-6 md:px-12 bg-transparent">
        <div className="flex items-center">
          <Link to="/">
            <Logo variant="full" className="h-8 md:h-10 cursor-pointer" />
          </Link>
        </div>
        <div className="flex items-center gap-6">
          <button 
            onClick={() => setIsAboutOpen(true)}
            className="text-sm font-medium text-text-muted hover:text-primary transition-colors cursor-pointer"
          >
            Hakkımızda
          </button>
          <Link to="/yardim" className="text-sm font-medium text-text-muted hover:text-primary transition-colors">
            Yardım ve İletişim
          </Link>
        </div>
      </nav>

      <AboutModal 
        isOpen={isAboutOpen} 
        onClose={() => setIsAboutOpen(false)} 
      />
    </>
  );
}
