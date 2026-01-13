import React from 'react';
import { Modal } from './Modal';
import { InputField } from './InputField';

interface PasswordChangeModalProps {
    isOpen: boolean;
    onClose: () => void;
    currentPassword: string;
    setCurrentPassword: (value: string) => void;
    newPassword: string;
    setNewPassword: (value: string) => void;
    confirmPassword: string;
    setConfirmPassword: (value: string) => void;
    onSubmit: () => void;
    loading: boolean;
    error: string;
}

export const PasswordChangeModal: React.FC<PasswordChangeModalProps> = ({
    isOpen,
    onClose,
    currentPassword,
    setCurrentPassword,
    newPassword,
    setNewPassword,
    confirmPassword,
    setConfirmPassword,
    onSubmit,
    loading,
    error,
}) => {
    const footer = (
        <>
            <button
                onClick={onClose}
                className="px-4 py-2 rounded-lg bg-gray-200 hover:bg-gray-300"
            >
                Anulează
            </button>
            <button
                onClick={onSubmit}
                disabled={loading}
                className="px-4 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
            >
                {loading ? 'Se actualizează...' : 'Confirmă'}
            </button>
        </>
    );

    return (
        <Modal isOpen={isOpen} onClose={onClose} title="Schimbare parolă" footer={footer}>
            {error && (
                <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
                    {error}
                </div>
            )}
            <div className="space-y-3">
                <InputField
                    type="password"
                    placeholder="Parola curentă"
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                />
                <InputField
                    type="password"
                    placeholder="Parola nouă"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                />
                <InputField
                    type="password"
                    placeholder="Confirmă parola nouă"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                />
            </div>
        </Modal>
    );
};
