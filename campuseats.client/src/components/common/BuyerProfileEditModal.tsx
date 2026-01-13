import React from 'react';
import { BuyerProfile } from '../../types/profileTypes';
import { InputField } from './InputField';

interface BuyerProfileEditModalProps {
    isOpen: boolean;
    onClose: () => void;
    profile: BuyerProfile | null;
    setProfile: (profile: BuyerProfile) => void;
    onSubmit: () => void;
    loading: boolean;
    isNewProfile: boolean;
}

export const BuyerProfileEditModal: React.FC<BuyerProfileEditModalProps> = ({
    isOpen,
    onClose,
    profile,
    setProfile,
    onSubmit,
    loading,
    isNewProfile,
}) => {
    if (!isOpen || !profile) return null;

    const updateField = (field: keyof BuyerProfile, value: string | number) => {
        setProfile({ ...profile, [field]: value });
    };

    const updateAddress = (field: string, value: string) => {
        setProfile({
            ...profile,
            deliveryAddress: {
                ...profile.deliveryAddress,
                [field]: value,
            },
        });
    };

    return (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 overflow-y-auto">
            <div className="bg-white p-6 rounded-xl w-full max-w-2xl shadow-lg space-y-6 m-4">
                <h2 className="text-xl font-semibold">
                    {isNewProfile ? 'Creează profil cumpărător' : 'Editează profil cumpărător'}
                </h2>
                <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <InputField
                            label="Prenume"
                            type="text"
                            placeholder="Prenume"
                            value={profile.firstName}
                            onChange={(e) => updateField('firstName', e.target.value)}
                        />
                        <InputField
                            label="Nume"
                            type="text"
                            placeholder="Nume"
                            value={profile.lastName}
                            onChange={(e) => updateField('lastName', e.target.value)}
                        />
                    </div>
                    <InputField
                        label="Vârstă"
                        type="number"
                        placeholder="Vârstă"
                        value={profile.age}
                        onChange={(e) => updateField('age', parseInt(e.target.value) || 0)}
                    />
                    <div className="border-t pt-4">
                        <h3 className="font-semibold mb-3">Adresă de livrare</h3>
                        <div className="space-y-3">
                            <InputField
                                type="text"
                                placeholder="Stradă"
                                value={profile.deliveryAddress.street}
                                onChange={(e) => updateAddress('street', e.target.value)}
                            />
                            <InputField
                                type="text"
                                placeholder="Clădire/Număr"
                                value={profile.deliveryAddress.building}
                                onChange={(e) => updateAddress('building', e.target.value)}
                            />
                            <div className="grid grid-cols-2 gap-3">
                                <InputField
                                    type="text"
                                    placeholder="Oraș"
                                    value={profile.deliveryAddress.city}
                                    onChange={(e) => updateAddress('city', e.target.value)}
                                />
                                <InputField
                                    type="text"
                                    placeholder="Județ"
                                    value={profile.deliveryAddress.county}
                                    onChange={(e) => updateAddress('county', e.target.value)}
                                />
                            </div>
                        </div>
                    </div>
                </div>

                <div className="flex justify-end gap-3">
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
                        {loading ? 'Se salvează...' : 'Salvează'}
                    </button>
                </div>
            </div>
        </div>
    );
};
