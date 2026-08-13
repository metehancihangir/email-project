import Logo from './Logo';

export default function Navbar() {
  return (
    <nav className="flex items-center justify-between py-4 px-6 md:px-12 bg-surface shadow-sm">
      <div className="flex items-center">
        <Logo variant="full" className="h-8 md:h-10" />
      </div>
    </nav>
  );
}
