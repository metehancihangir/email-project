import React from 'react';

export default function SubmailLogo({ className = "h-24 w-auto" }) {
  return (
    <svg 
      className={className} 
      viewBox="0 0 420 100" 
      fill="none" 
      xmlns="http://www.w3.org/2000/svg"
    >
      {/* Green Envelope Icon with Arrow */}
      <g stroke="#10b981" strokeWidth="6" strokeLinecap="round" strokeLinejoin="round">
        <rect x="8" y="18" width="84" height="60" rx="10" fill="#10b981" fillOpacity="0.05" />
        <path d="M8 26L45 53C48 55 52 55 55 53L92 26" />
        {/* Up-Right Arrow inside bottom-left */}
        <path d="M22 62L44 40" strokeWidth="6" />
        <path d="M30 40H44V54" strokeWidth="6" />
      </g>
      
      {/* SUBMAIL Text */}
      <text 
        x="112" 
        y="54" 
        fill="#1f2937" 
        fontSize="44" 
        fontWeight="800" 
        fontFamily="Inter, system-ui, -apple-system, sans-serif" 
        letterSpacing="-0.5px"
      >
        SUB<tspan fill="#10b981">MAIL</tspan>
      </text>

      {/* Subtitle Text */}
      <text 
        x="114" 
        y="78" 
        fill="#6b7280" 
        fontSize="17" 
        fontWeight="500" 
        fontFamily="Inter, system-ui, -apple-system, sans-serif" 
        letterSpacing="0.2px"
      >
        Email Subscription App
      </text>
    </svg>
  );
}
