import React, { createContext, useContext, useState, useCallback, useMemo } from 'react';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
}

interface ToastContextType {
  showToast: (message: string, type: ToastType) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

export const useToast = () => {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
};

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const removeToastById = useCallback((id: number) => {
    setToasts((prev) => prev.filter((toast) => toast.id !== id));
  }, []);

  const showToast = useCallback((message: string, type: ToastType) => {
    const id = Date.now() + Math.random();
    setToasts((prev) => [...prev, { id, message, type }]);

    setTimeout(() => {
      removeToastById(id);
    }, 3000);
  }, [removeToastById]);

  const removeToast = useCallback((id: number) => {
    removeToastById(id);
  }, [removeToastById]);

  const contextValue = useMemo(
    () => ({ showToast }),
    [showToast]
  );

  return (
    <ToastContext.Provider value={contextValue}>
      {children}
      <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2">
        {toasts.map((toast) => (
          <div
            key={toast.id}
            className={`
              min-w-[300px] p-4 rounded-lg shadow-lg text-white transform transition-all duration-300 animate-fade-in-up
              ${toast.type === 'success' ? 'bg-gradient-to-r from-green-500 to-emerald-600' : ''}
              ${toast.type === 'error' ? 'bg-gradient-to-r from-red-500 to-pink-600' : ''}
              ${toast.type === 'info' ? 'bg-gradient-to-r from-blue-500 to-cyan-600' : ''}
              ${toast.type === 'warning' ? 'bg-gradient-to-r from-yellow-500 to-orange-600' : ''}
            `}
          >
            <div className="flex justify-between items-center">
              <p className="font-medium">{toast.message}</p>
              <button
                onClick={() => removeToast(toast.id)}
                className="ml-4 text-white hover:text-gray-200 focus:outline-none"
              >
                ✕
              </button>
            </div>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
};
