import React from 'react';
import { KitchenProfile, WeeklyWorkingHours } from '../../types/profileTypes';
import { InputField } from './InputField';

interface KitchenProfileEditModalProps {
    isOpen: boolean;
    onClose: () => void;
    profile: KitchenProfile | null;
    setProfile: (profile: KitchenProfile) => void;
    onSubmit: () => void;
    loading: boolean;
    isNewProfile: boolean;
}

export const KitchenProfileEditModal: React.FC<KitchenProfileEditModalProps> = ({
    isOpen,
    onClose,
    profile,
    setProfile,
    onSubmit,
    loading,
    isNewProfile,
}) => {
    if (!isOpen || !profile) return null;

    const updateField = (field: keyof KitchenProfile, value: string) => {
        setProfile({ ...profile, [field]: value });
    };

    const updateAddress = (field: string, value: string) => {
        setProfile({
            ...profile,
            kitchenAddress: {
                ...profile.kitchenAddress,
                [field]: value,
            },
        });
    };

    const updateWorkingHours = (day: string, timeType: 'open' | 'close', value: string) => {
        setProfile({
            ...profile,
            weeklyWorkingHours: {
                ...profile.weeklyWorkingHours,
                [day]: {
                    ...profile.weeklyWorkingHours[day as keyof WeeklyWorkingHours],
                    [timeType]: value,
                },
            },
        });
    };

    return (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 overflow-y-auto">
            <div className="bg-white p-6 rounded-xl w-full max-w-3xl shadow-lg space-y-6 m-4 max-h-[90vh] overflow-y-auto">
                <h2 className="text-xl font-semibold sticky top-0 bg-white pb-2">
                    {isNewProfile ? 'Creează profil bucătărie' : 'Editează profil bucătărie'}
                </h2>
                <div className="space-y-4">
                    <InputField
                        label="Nume companie"
                        type="text"
                        placeholder="Nume companie"
                        value={profile.companyName}
                        onChange={(e) => updateField('companyName', e.target.value)}
                    />
                    <div className="border-t pt-4">
                        <h3 className="font-semibold mb-3">Adresă bucătărie</h3>
                        <div className="space-y-3">
                            <InputField
                                type="text"
                                placeholder="Stradă"
                                value={profile.kitchenAddress.street}
                                onChange={(e) => updateAddress('street', e.target.value)}
                            />
                            <InputField
                                type="text"
                                placeholder="Clădire/Număr"
                                value={profile.kitchenAddress.building}
                                onChange={(e) => updateAddress('building', e.target.value)}
                            />
                            <div className="grid grid-cols-2 gap-3">
                                <InputField
                                    type="text"
                                    placeholder="Oraș"
                                    value={profile.kitchenAddress.city}
                                    onChange={(e) => updateAddress('city', e.target.value)}
                                />
                                <InputField
                                    type="text"
                                    placeholder="Județ"
                                    value={profile.kitchenAddress.county}
                                    onChange={(e) => updateAddress('county', e.target.value)}
                                />
                            </div>
                        </div>
                    </div>
                    <div className="border-t pt-4">
                        <h3 className="font-semibold mb-3">Program săptămânal</h3>
                        <div className="space-y-3">
                            {Object.entries(profile.weeklyWorkingHours).map(([day, hours]) => (
                                <div key={day} className="grid grid-cols-3 gap-3 items-center">
                                    <label className="text-sm font-medium capitalize">{day}</label>
                                    <input
                                        type="time"
                                        className="p-2 border rounded-lg"
                                        value={hours.open}
                                        onChange={(e) => updateWorkingHours(day, 'open', e.target.value)}
                                    />
                                    <input
                                        type="time"
                                        className="p-2 border rounded-lg"
                                        value={hours.close}
                                        onChange={(e) => updateWorkingHours(day, 'close', e.target.value)}
                                    />
                                </div>
                            ))}
                        </div>
                    </div>
                </div>

                <div className="flex justify-end gap-3 sticky bottom-0 bg-white pt-4">
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
