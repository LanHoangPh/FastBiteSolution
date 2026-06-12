
export function AuthBackground() {
  return (
    <div className="auth-background">
      {/* Decorative Arc Lines */}
      <svg
        className="auth-arcs"
        viewBox="0 0 1200 800"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <path
          d="M 100,500 Q 600,100 1100,500"
          stroke="currentColor"
          strokeWidth="1"
          strokeDasharray="4 4"
        />
        <path
          d="M 150,550 Q 600,180 1050,550"
          stroke="currentColor"
          strokeWidth="1.5"
        />
        <path
          d="M 50,450 Q 600,20 1150,450"
          stroke="currentColor"
          strokeWidth="0.5"
          className="opacity-50"
        />
      </svg>

      {/* Cloud Layer at the Bottom (using CSS radial-gradient defined in globals.css) */}
      <div className="auth-clouds" />
    </div>
  );
}
