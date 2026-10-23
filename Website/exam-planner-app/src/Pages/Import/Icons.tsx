export const FileIcon = ({ className }: { className?: string }) => (
    <svg className={className} viewBox="0 0 24 24" aria-hidden="true">
        <path d="M4 3.75h16v10.5l-5.75 6H4z" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinejoin="round" />
        <path d="M20 14.25h-5.75v6" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinejoin="round" />
    </svg>
);

export const CheckIcon = () => (
    <svg viewBox="0 0 16 16" aria-hidden="true">
        <path d="M3.5 8.5l3 3 6-7" fill="none" stroke="currentColor" strokeWidth="2.2" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
);
