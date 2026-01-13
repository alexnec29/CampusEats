import React, { ReactNode } from 'react';

interface ModalProps {
    isOpen: boolean;
    onClose: () => void;
    title: string;
    children: ReactNode;
    footer?: ReactNode;
}

export const Modal: React.FC<ModalProps> = ({ isOpen, onClose, title, children, footer }) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 overflow-y-auto">
            <div className="bg-white p-6 rounded-xl w-full max-w-md shadow-lg space-y-6 m-4 max-h-[90vh] overflow-y-auto">
                <h2 className="text-xl font-semibold">{title}</h2>
                {children}
                {footer && <div className="flex justify-end gap-3">{footer}</div>}
            </div>
        </div>
    );
};
