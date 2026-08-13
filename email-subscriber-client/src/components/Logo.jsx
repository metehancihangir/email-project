import wordmarkSvg from '../assets/wordmark.svg';
import iconSvg from '../assets/logo-icon.svg';

export default function Logo({ variant = 'full', className = '' }) {
  const src = variant === 'icon' ? iconSvg : wordmarkSvg;
  return <img src={src} alt="Submail" className={className} />;
}
