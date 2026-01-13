import React from 'react';
import { Edit, Mail, Star, MapPin } from 'lucide-react';
import { BuyerProfile } from '../../types/profileTypes';

interface BuyerProfileCardProps {
    buyerProfile: BuyerProfile | null;
    onEdit: () => void;
}

export const BuyerProfileCard: React.FC<BuyerProfileCardProps> = ({ buyerProfile, onEdit }) => {
    return (
        <div className="bg-white p-8 rounded-2xl shadow-xl space-y-6 animate-fade-in-delay-2">
            <div className="flex justify-between items-center border-b-2 border-gray-200 pb-4">
                <h2 className="text-2xl font-bold text-gray-900">Profil cumpărător</h2>
                <button
                    onClick={onEdit}
                    className="flex items-center gap-2 px-5 py-3 rounded-lg bg-gradient-to-r from-blue-500 to-purple-500 hover:from-blue-600 hover:to-purple-600 transition text-white font-semibold shadow-lg transform hover:scale-105"
                >
                    <Edit className="w-4 h-4" />
                    {buyerProfile ? 'Editează' : 'Creează'}
                </button>
            </div>
            {buyerProfile ? (
                <div className="space-y-4">
                    <div className="flex items-start gap-3 text-gray-700">
                        <Mail className="w-5 h-5 text-gray-500 mt-1" />
                        <div>
                            <span className="font-medium">Nume complet:</span>{' '}
                            {buyerProfile.firstName} {buyerProfile.lastName}
                        </div>
                    </div>
                    <div className="flex items-center gap-3 text-gray-700">
                        <Star className="w-5 h-5 text-gray-500" />
                        <span className="font-medium">Vârstă:</span> {buyerProfile.age}
                    </div>
                    <div className="flex items-start gap-3 text-gray-700">
                        <MapPin className="w-5 h-5 text-gray-500 mt-1" />
                        <div>
                            <span className="font-medium">Adresă de livrare:</span>
                            <div className="mt-1 text-sm">
                                {buyerProfile.deliveryAddress.street}, {buyerProfile.deliveryAddress.building}
                                <br />
                                {buyerProfile.deliveryAddress.city}, {buyerProfile.deliveryAddress.county}
                            </div>
                        </div>
                    </div>
                </div>
            ) : (
                <div className="text-gray-500 text-center py-4">
                    Nu ați creat încă un profil de cumpărător. Faceți clic pe "Creează" pentru a adăuga detalii.
                </div>
            )}
        </div>
    );
};
