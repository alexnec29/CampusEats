import React from 'react';
import { Edit, MapPin, Clock, Building } from 'lucide-react';
import { KitchenProfile } from '../../types/profileTypes';

interface KitchenProfileCardProps {
    kitchenProfile: KitchenProfile | null;
    onEdit: () => void;
}

export const KitchenProfileCard: React.FC<KitchenProfileCardProps> = ({ kitchenProfile, onEdit }) => {
    return (
        <div className="bg-white p-8 rounded-2xl shadow-xl space-y-6 animate-fade-in-delay-2">
            <div className="flex justify-between items-center border-b-2 border-gray-200 pb-4">
                <h2 className="text-2xl font-bold text-gray-900">Profil bucătărie</h2>
                <button
                    onClick={onEdit}
                    className="flex items-center gap-2 px-5 py-3 rounded-lg bg-gradient-to-r from-blue-500 to-purple-500 hover:from-blue-600 hover:to-purple-600 transition text-white font-semibold shadow-lg transform hover:scale-105"
                >
                    <Edit className="w-4 h-4" />
                    {kitchenProfile ? 'Editează' : 'Creează'}
                </button>
            </div>
            {kitchenProfile ? (
                <div className="space-y-4">
                    <div className="flex items-center gap-3 text-gray-700">
                        <Building className="w-5 h-5 text-gray-500" />
                        <span className="font-medium">Nume companie:</span> {kitchenProfile.companyName}
                    </div>
                    <div className="flex items-start gap-3 text-gray-700">
                        <MapPin className="w-5 h-5 text-gray-500 mt-1" />
                        <div>
                            <span className="font-medium">Adresă bucătărie:</span>
                            <div className="mt-1 text-sm">
                                {kitchenProfile.kitchenAddress.street}, {kitchenProfile.kitchenAddress.building}
                                <br />
                                {kitchenProfile.kitchenAddress.city}, {kitchenProfile.kitchenAddress.county}
                            </div>
                        </div>
                    </div>
                    <div className="flex items-start gap-3 text-gray-700">
                        <Clock className="w-5 h-5 text-gray-500 mt-1" />
                        <div className="flex-1">
                            <span className="font-medium">Program săptămânal:</span>
                            <div className="mt-2 grid grid-cols-1 gap-2 text-sm">
                                {Object.entries(kitchenProfile.weeklyWorkingHours).map(([day, hours]) => (
                                    <div key={day} className="flex justify-between">
                                        <span className="capitalize">{day}:</span>
                                        <span>{hours.open} - {hours.close}</span>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>
            ) : (
                <div className="text-gray-500 text-center py-4">
                    Nu ați creat încă un profil de bucătărie. Faceți clic pe "Creează" pentru a adăuga detalii.
                </div>
            )}
        </div>
    );
};
