import React from 'react';
import { KeyRound, Trash2, Edit } from 'lucide-react';

interface AccountSettingsCardProps {
    onChangePassword: () => void;
    onDeleteAccount: () => void;
}

export const AccountSettingsCard: React.FC<AccountSettingsCardProps> = ({
    onChangePassword,
    onDeleteAccount
}) => {
    return (
        <div className="bg-white p-8 rounded-2xl shadow-xl space-y-6">
            <h2 className="text-2xl font-bold border-b-2 border-gray-200 pb-4">Setări cont</h2>
            <div className="flex flex-col gap-4">
                <button
                    onClick={onChangePassword}
                    className="flex items-center justify-between px-6 py-4 rounded-lg bg-gradient-to-r from-gray-100 to-gray-50 hover:from-gray-200 hover:to-gray-100 transition shadow-md transform hover:scale-105 border-2 border-gray-200"
                >
                    <span className="flex items-center gap-3 font-semibold text-gray-900">
                        <KeyRound className="w-5 h-5 text-blue-600" />
                        Schimbă parola
                    </span>
                    <Edit className="w-5 h-5 text-gray-500" />
                </button>

                <button
                    onClick={onDeleteAccount}
                    className="flex items-center justify-between px-5 py-3 rounded-lg bg-red-100 hover:bg-red-200 transition shadow-sm text-red-700"
                >
                    <span className="flex items-center gap-3 font-semibold">
                        <Trash2 className="w-5 h-5" />
                        Șterge contul
                    </span>
                </button>
            </div>
        </div>
    );
};
