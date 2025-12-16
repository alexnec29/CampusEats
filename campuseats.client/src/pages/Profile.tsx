import React, { useEffect, useState } from "react";
import { apiClient } from "../utils/apiClient";
import { Mail, Shield, Star, KeyRound, Trash2, Edit, MapPin, Clock, Building } from "lucide-react";

interface UserInfo {
    username: string;
    role: string;
    email?: string;
    loyaltyPoints?: number;
}

interface Address {
    street: string;
    building: string;
    city: string;
    county: string;
}

interface WorkingHours {
    open: string;
    close: string;
}

interface WeeklyWorkingHours {
    monday: WorkingHours;
    tuesday: WorkingHours;
    wednesday: WorkingHours;
    thursday: WorkingHours;
    friday: WorkingHours;
    saturday: WorkingHours;
    sunday: WorkingHours;
}

interface BuyerProfile {
    lastName: string;
    firstName: string;
    age: number;
    deliveryAddress: Address;
}

interface KitchenProfile {
    companyName: string;
    kitchenAddress: Address;
    weeklyWorkingHours: WeeklyWorkingHours;
}

const Profile: React.FC = () => {
    const [user, setUser] = useState<UserInfo | null>(null);
    const [buyerProfile, setBuyerProfile] = useState<BuyerProfile | null>(null);
    const [kitchenProfile, setKitchenProfile] = useState<KitchenProfile | null>(null);
    const [loading, setLoading] = useState(true);

    // Password modal state
    const [showPasswordModal, setShowPasswordModal] = useState(false);
    const [currentPassword, setCurrentPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [loadingPassword, setLoadingPassword] = useState(false);
    const [passwordError, setPasswordError] = useState("");

    // Buyer profile edit modal state
    const [showBuyerEditModal, setShowBuyerEditModal] = useState(false);
    const [editBuyerProfile, setEditBuyerProfile] = useState<BuyerProfile | null>(null);
    const [loadingBuyerUpdate, setLoadingBuyerUpdate] = useState(false);

    // Kitchen profile edit modal state
    const [showKitchenEditModal, setShowKitchenEditModal] = useState(false);
    const [editKitchenProfile, setEditKitchenProfile] = useState<KitchenProfile | null>(null);
    const [loadingKitchenUpdate, setLoadingKitchenUpdate] = useState(false);

    //Loyalty Profile
    const [loyaltyPoints, setLoyaltyPoints] = useState<number | null>(null);

    // Load user info and profile
    useEffect(() => {
        const loadUserData = async () => {
            try {
                setLoading(true);
                const res = await apiClient("/api/user/check-auth");
                if (res.ok) {
                    const data = await res.json();
                    setUser(data);
                    
                    // Load role-specific profile
                    if (data.role === "Buyer") {
                        await Promise.all([
                            loadBuyerProfile(),
                            loadLoyaltyAccount(),
                        ]);
                    } else if (data.role === "Kitchen") {
                        await loadKitchenProfile();
                    }
                }
            } catch (err) {
                console.error("Failed to load user", err);
            } finally {
                setLoading(false);
            }
        };
        loadUserData();
    }, []);

    const loadBuyerProfile = async () => {
        try {
            const res = await apiClient("/api/user/buyer-profile");
            if (res.ok) {
                const data = await res.json();
                setBuyerProfile(data);
            } else if (res.status === 404) {
                // Profile doesn't exist yet
                setBuyerProfile(null);
            }
        } catch (err) {
            console.error("Failed to load buyer profile", err);
        }
    };

    const loadKitchenProfile = async () => {
        try {
            const res = await apiClient("/api/user/kitchen-profile");
            if (res.ok) {
                const data = await res.json();
                setKitchenProfile(data);
            } else if (res.status === 404) {
                // Profile doesn't exist yet
                setKitchenProfile(null);
            }
        } catch (err) {
            console.error("Failed to load kitchen profile", err);
        }
    };

    const loadLoyaltyAccount = async () => {
        try {
            const res = await apiClient("/api/loyalty/account");
            if (res.ok) {
                const data = await res.json();
                setLoyaltyPoints(data.pointsBalance);
            }
        } catch (err) {
            console.error("Failed to load loyalty account", err);
        }
    };

    if (!user || loading) {
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-blue-500"></div>
            </div>
        );
    }

    const roleColors: Record<string, string> = {
        Admin: "bg-red-100 text-red-700 border-red-300",
        Kitchen: "bg-yellow-100 text-yellow-700 border-yellow-300",
        Buyer: "bg-green-100 text-green-700 border-green-300",
    };

    // -----------------------------
    // Change password handler
    // -----------------------------
    const submitPasswordChange = async () => {
        setPasswordError("");
        
        if (!currentPassword || !newPassword || !confirmPassword) {
            setPasswordError("Toate câmpurile pentru parolă sunt obligatorii.");
            return;
        }

        if (newPassword !== confirmPassword) {
            setPasswordError("Noua parolă și confirmarea nu coincid.");
            return;
        }

        if (newPassword.length < 6) {
            setPasswordError("Noua parolă trebuie să aibă cel puțin 6 caractere.");
            return;
        }

        setLoadingPassword(true);
        try {
            const res = await apiClient("/api/user/change-password", {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    currentPassword,
                    newPassword,
                    confirmNewPassword: confirmPassword,
                }),
            });

            if (res.ok) {
                alert("Parola a fost schimbată cu succes!");
                setShowPasswordModal(false);
                setCurrentPassword("");
                setNewPassword("");
                setConfirmPassword("");
                setPasswordError("");
            } else {
                const text = await res.text();
                setPasswordError(text || "Eroare la schimbarea parolei.");
            }
        } catch (err) {
            console.error(err);
            setPasswordError("Eroare la schimbarea parolei.");
        } finally {
            setLoadingPassword(false);
        }
    };

    // -----------------------------
    // Buyer profile handlers
    // -----------------------------
    const openBuyerEditModal = () => {
        if (buyerProfile) {
            setEditBuyerProfile({ ...buyerProfile });
        } else {
            // Initialize with default values
            setEditBuyerProfile({
                lastName: "",
                firstName: "",
                age: 18,
                deliveryAddress: {
                    street: "",
                    building: "",
                    city: "",
                    county: "",
                },
            });
        }
        setShowBuyerEditModal(true);
    };

    const submitBuyerProfileUpdate = async () => {
        if (!editBuyerProfile) return;

        setLoadingBuyerUpdate(true);
        try {
            const res = await apiClient("/api/user/update-buyer-profile", {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(editBuyerProfile),
            });

            if (res.ok || res.status === 204) {
                alert("Profilul de cumpărător a fost actualizat cu succes!");
                setShowBuyerEditModal(false);
                await loadBuyerProfile();
            } else {
                const text = await res.text();
                alert("Eroare: " + text);
            }
        } catch (err) {
            console.error(err);
            alert("Eroare la actualizarea profilului.");
        } finally {
            setLoadingBuyerUpdate(false);
        }
    };

    // -----------------------------
    // Kitchen profile handlers
    // -----------------------------
    const openKitchenEditModal = () => {
        if (kitchenProfile) {
            setEditKitchenProfile({ ...kitchenProfile });
        } else {
            // Initialize with default values
            const defaultHours = { open: "09:00", close: "17:00" };
            setEditKitchenProfile({
                companyName: "",
                kitchenAddress: {
                    street: "",
                    building: "",
                    city: "",
                    county: "",
                },
                weeklyWorkingHours: {
                    monday: { ...defaultHours },
                    tuesday: { ...defaultHours },
                    wednesday: { ...defaultHours },
                    thursday: { ...defaultHours },
                    friday: { ...defaultHours },
                    saturday: { ...defaultHours },
                    sunday: { ...defaultHours },
                },
            });
        }
        setShowKitchenEditModal(true);
    };

    const submitKitchenProfileUpdate = async () => {
        if (!editKitchenProfile) return;

        setLoadingKitchenUpdate(true);
        try {
            const res = await apiClient("/api/user/update-kitchen-profile", {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(editKitchenProfile),
            });

            if (res.ok || res.status === 204) {
                alert("Profilul de bucătărie a fost actualizat cu succes!");
                setShowKitchenEditModal(false);
                await loadKitchenProfile();
            } else {
                const text = await res.text();
                alert("Eroare: " + text);
            }
        } catch (err) {
            console.error(err);
            alert("Eroare la actualizarea profilului.");
        } finally {
            setLoadingKitchenUpdate(false);
        }
    };

    // -----------------------------
    // Delete account placeholder
    // -----------------------------
    const handleDeleteAccount = () => {
        alert("Funcționalitatea de ștergere cont nu este încă implementată.");
    };

    return (
        <div className="max-w-3xl mx-auto space-y-8 p-4">

            {/* Header */}
            <div className="bg-blue-600 text-white p-8 rounded-xl shadow-md flex items-center gap-6">
                <div className="w-20 h-20 rounded-full bg-white/20 flex items-center justify-center text-4xl font-bold">
                    {user.username.charAt(0).toUpperCase()}
                </div>
                <div>
                    <h1 className="text-3xl font-bold">{user.username}</h1>
                    <p className="text-blue-100">Profilul utilizatorului</p>
                </div>
            </div>

            {/* Info Card */}
            <div className="bg-white p-8 rounded-xl shadow space-y-6">
                <h2 className="text-xl font-semibold border-b pb-2">Informații generale</h2>
                <div className="space-y-4">
                    {user.email && (
                        <div className="flex items-center gap-3 text-gray-700">
                            <Mail className="w-5 h-5 text-gray-500" />
                            <span className="font-medium">Email:</span> {user.email}
                        </div>
                    )}
                    <div className="flex items-center gap-3 text-gray-700">
                        <Shield className="w-5 h-5 text-gray-500" />
                        <span className="font-medium">Rol:</span>
                        <span className={`px-3 py-1 rounded-full text-sm border ${roleColors[user.role]}`}>
                            {user.role}
                        </span>
                    </div>
                    {user.role === "Buyer" && loyaltyPoints !== null && (
                        <div className="flex items-center gap-3 text-gray-700">
                            <Star className="w-5 h-5 text-yellow-500" />
                            <span className="font-medium">Puncte loialitate:</span>
                            {loyaltyPoints}
                        </div>
                    )}
                </div>
            </div>

            {/* Buyer Profile Card */}
            {user.role === "Buyer" && (
                <div className="bg-white p-8 rounded-xl shadow space-y-6">
                    <div className="flex justify-between items-center border-b pb-2">
                        <h2 className="text-xl font-semibold">Profil cumpărător</h2>
                        <button
                            onClick={openBuyerEditModal}
                            className="flex items-center gap-2 px-4 py-2 rounded-lg bg-blue-100 hover:bg-blue-200 transition text-blue-700"
                        >
                            <Edit className="w-4 h-4" />
                            {buyerProfile ? "Editează" : "Creează"}
                        </button>
                    </div>
                    {buyerProfile ? (
                        <div className="space-y-4">
                            <div className="flex items-start gap-3 text-gray-700">
                                <Mail className="w-5 h-5 text-gray-500 mt-1" />
                                <div>
                                    <span className="font-medium">Nume complet:</span>{" "}
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
            )}

            {/* Kitchen Profile Card */}
            {user.role === "Kitchen" && (
                <div className="bg-white p-8 rounded-xl shadow space-y-6">
                    <div className="flex justify-between items-center border-b pb-2">
                        <h2 className="text-xl font-semibold">Profil bucătărie</h2>
                        <button
                            onClick={openKitchenEditModal}
                            className="flex items-center gap-2 px-4 py-2 rounded-lg bg-blue-100 hover:bg-blue-200 transition text-blue-700"
                        >
                            <Edit className="w-4 h-4" />
                            {kitchenProfile ? "Editează" : "Creează"}
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
            )}

            {/* Account Settings */}
            <div className="bg-white p-8 rounded-xl shadow space-y-6">
                <h2 className="text-xl font-semibold border-b pb-2">Setări cont</h2>
                <div className="flex flex-col gap-4">
                    <button
                        onClick={() => setShowPasswordModal(true)}
                        className="flex items-center justify-between px-5 py-3 rounded-lg bg-gray-100 hover:bg-gray-200 transition shadow-sm"
                    >
                        <span className="flex items-center gap-3 font-medium">
                            <KeyRound className="w-5 h-5" />
                            Schimbă parola
                        </span>
                        <Edit className="w-5 h-5 text-gray-500" />
                    </button>

                    <button
                        onClick={handleDeleteAccount}
                        className="flex items-center justify-between px-5 py-3 rounded-lg bg-red-100 hover:bg-red-200 transition shadow-sm text-red-700"
                    >
                        <span className="flex items-center gap-3 font-semibold">
                            <Trash2 className="w-5 h-5" />
                            Șterge contul
                        </span>
                    </button>
                </div>
            </div>

            {/* Password Modal */}
            {showPasswordModal && (
                <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
                    <div className="bg-white p-6 rounded-xl w-full max-w-md shadow-lg space-y-6">
                        <h2 className="text-xl font-semibold">Schimbare parolă</h2>
                        {passwordError && (
                            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
                                {passwordError}
                            </div>
                        )}
                        <div className="space-y-3">
                            <input
                                type="password"
                                placeholder="Parola curentă"
                                className="w-full p-3 border rounded-lg"
                                value={currentPassword}
                                onChange={(e) => setCurrentPassword(e.target.value)}
                            />
                            <input
                                type="password"
                                placeholder="Parola nouă"
                                className="w-full p-3 border rounded-lg"
                                value={newPassword}
                                onChange={(e) => setNewPassword(e.target.value)}
                            />
                            <input
                                type="password"
                                placeholder="Confirmă parola nouă"
                                className="w-full p-3 border rounded-lg"
                                value={confirmPassword}
                                onChange={(e) => setConfirmPassword(e.target.value)}
                            />
                        </div>

                        <div className="flex justify-end gap-3">
                            <button
                                onClick={() => {
                                    setShowPasswordModal(false);
                                    setPasswordError("");
                                    setCurrentPassword("");
                                    setNewPassword("");
                                    setConfirmPassword("");
                                }}
                                className="px-4 py-2 rounded-lg bg-gray-200 hover:bg-gray-300"
                            >
                                Anulează
                            </button>
                            <button
                                onClick={submitPasswordChange}
                                disabled={loadingPassword}
                                className="px-4 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
                            >
                                {loadingPassword ? "Se actualizează..." : "Confirmă"}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Buyer Profile Edit Modal */}
            {showBuyerEditModal && editBuyerProfile && (
                <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 overflow-y-auto">
                    <div className="bg-white p-6 rounded-xl w-full max-w-2xl shadow-lg space-y-6 m-4">
                        <h2 className="text-xl font-semibold">
                            {buyerProfile ? "Editează profil cumpărător" : "Creează profil cumpărător"}
                        </h2>
                        <div className="space-y-4">
                            <div className="grid grid-cols-2 gap-4">
                                <div>
                                    <label className="block text-sm font-medium mb-1">Prenume</label>
                                    <input
                                        type="text"
                                        placeholder="Prenume"
                                        className="w-full p-3 border rounded-lg"
                                        value={editBuyerProfile.firstName}
                                        onChange={(e) =>
                                            setEditBuyerProfile({ ...editBuyerProfile, firstName: e.target.value })
                                        }
                                    />
                                </div>
                                <div>
                                    <label className="block text-sm font-medium mb-1">Nume</label>
                                    <input
                                        type="text"
                                        placeholder="Nume"
                                        className="w-full p-3 border rounded-lg"
                                        value={editBuyerProfile.lastName}
                                        onChange={(e) =>
                                            setEditBuyerProfile({ ...editBuyerProfile, lastName: e.target.value })
                                        }
                                    />
                                </div>
                            </div>
                            <div>
                                <label className="block text-sm font-medium mb-1">Vârstă</label>
                                <input
                                    type="number"
                                    placeholder="Vârstă"
                                    className="w-full p-3 border rounded-lg"
                                    value={editBuyerProfile.age}
                                    onChange={(e) =>
                                        setEditBuyerProfile({ ...editBuyerProfile, age: parseInt(e.target.value) || 0 })
                                    }
                                />
                            </div>
                            <div className="border-t pt-4">
                                <h3 className="font-semibold mb-3">Adresă de livrare</h3>
                                <div className="space-y-3">
                                    <input
                                        type="text"
                                        placeholder="Stradă"
                                        className="w-full p-3 border rounded-lg"
                                        value={editBuyerProfile.deliveryAddress.street}
                                        onChange={(e) =>
                                            setEditBuyerProfile({
                                                ...editBuyerProfile,
                                                deliveryAddress: {
                                                    ...editBuyerProfile.deliveryAddress,
                                                    street: e.target.value,
                                                },
                                            })
                                        }
                                    />
                                    <input
                                        type="text"
                                        placeholder="Clădire/Număr"
                                        className="w-full p-3 border rounded-lg"
                                        value={editBuyerProfile.deliveryAddress.building}
                                        onChange={(e) =>
                                            setEditBuyerProfile({
                                                ...editBuyerProfile,
                                                deliveryAddress: {
                                                    ...editBuyerProfile.deliveryAddress,
                                                    building: e.target.value,
                                                },
                                            })
                                        }
                                    />
                                    <div className="grid grid-cols-2 gap-3">
                                        <input
                                            type="text"
                                            placeholder="Oraș"
                                            className="w-full p-3 border rounded-lg"
                                            value={editBuyerProfile.deliveryAddress.city}
                                            onChange={(e) =>
                                                setEditBuyerProfile({
                                                    ...editBuyerProfile,
                                                    deliveryAddress: {
                                                        ...editBuyerProfile.deliveryAddress,
                                                        city: e.target.value,
                                                    },
                                                })
                                            }
                                        />
                                        <input
                                            type="text"
                                            placeholder="Județ"
                                            className="w-full p-3 border rounded-lg"
                                            value={editBuyerProfile.deliveryAddress.county}
                                            onChange={(e) =>
                                                setEditBuyerProfile({
                                                    ...editBuyerProfile,
                                                    deliveryAddress: {
                                                        ...editBuyerProfile.deliveryAddress,
                                                        county: e.target.value,
                                                    },
                                                })
                                            }
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="flex justify-end gap-3">
                            <button
                                onClick={() => setShowBuyerEditModal(false)}
                                className="px-4 py-2 rounded-lg bg-gray-200 hover:bg-gray-300"
                            >
                                Anulează
                            </button>
                            <button
                                onClick={submitBuyerProfileUpdate}
                                disabled={loadingBuyerUpdate}
                                className="px-4 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
                            >
                                {loadingBuyerUpdate ? "Se salvează..." : "Salvează"}
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {/* Kitchen Profile Edit Modal */}
            {showKitchenEditModal && editKitchenProfile && (
                <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 overflow-y-auto">
                    <div className="bg-white p-6 rounded-xl w-full max-w-3xl shadow-lg space-y-6 m-4 max-h-[90vh] overflow-y-auto">
                        <h2 className="text-xl font-semibold sticky top-0 bg-white pb-2">
                            {kitchenProfile ? "Editează profil bucătărie" : "Creează profil bucătărie"}
                        </h2>
                        <div className="space-y-4">
                            <div>
                                <label className="block text-sm font-medium mb-1">Nume companie</label>
                                <input
                                    type="text"
                                    placeholder="Nume companie"
                                    className="w-full p-3 border rounded-lg"
                                    value={editKitchenProfile.companyName}
                                    onChange={(e) =>
                                        setEditKitchenProfile({ ...editKitchenProfile, companyName: e.target.value })
                                    }
                                />
                            </div>
                            <div className="border-t pt-4">
                                <h3 className="font-semibold mb-3">Adresă bucătărie</h3>
                                <div className="space-y-3">
                                    <input
                                        type="text"
                                        placeholder="Stradă"
                                        className="w-full p-3 border rounded-lg"
                                        value={editKitchenProfile.kitchenAddress.street}
                                        onChange={(e) =>
                                            setEditKitchenProfile({
                                                ...editKitchenProfile,
                                                kitchenAddress: {
                                                    ...editKitchenProfile.kitchenAddress,
                                                    street: e.target.value,
                                                },
                                            })
                                        }
                                    />
                                    <input
                                        type="text"
                                        placeholder="Clădire/Număr"
                                        className="w-full p-3 border rounded-lg"
                                        value={editKitchenProfile.kitchenAddress.building}
                                        onChange={(e) =>
                                            setEditKitchenProfile({
                                                ...editKitchenProfile,
                                                kitchenAddress: {
                                                    ...editKitchenProfile.kitchenAddress,
                                                    building: e.target.value,
                                                },
                                            })
                                        }
                                    />
                                    <div className="grid grid-cols-2 gap-3">
                                        <input
                                            type="text"
                                            placeholder="Oraș"
                                            className="w-full p-3 border rounded-lg"
                                            value={editKitchenProfile.kitchenAddress.city}
                                            onChange={(e) =>
                                                setEditKitchenProfile({
                                                    ...editKitchenProfile,
                                                    kitchenAddress: {
                                                        ...editKitchenProfile.kitchenAddress,
                                                        city: e.target.value,
                                                    },
                                                })
                                            }
                                        />
                                        <input
                                            type="text"
                                            placeholder="Județ"
                                            className="w-full p-3 border rounded-lg"
                                            value={editKitchenProfile.kitchenAddress.county}
                                            onChange={(e) =>
                                                setEditKitchenProfile({
                                                    ...editKitchenProfile,
                                                    kitchenAddress: {
                                                        ...editKitchenProfile.kitchenAddress,
                                                        county: e.target.value,
                                                    },
                                                })
                                            }
                                        />
                                    </div>
                                </div>
                            </div>
                            <div className="border-t pt-4">
                                <h3 className="font-semibold mb-3">Program săptămânal</h3>
                                <div className="space-y-3">
                                    {Object.entries(editKitchenProfile.weeklyWorkingHours).map(([day, hours]) => (
                                        <div key={day} className="grid grid-cols-3 gap-3 items-center">
                                            <label className="text-sm font-medium capitalize">{day}</label>
                                            <input
                                                type="time"
                                                className="p-2 border rounded-lg"
                                                value={hours.open}
                                                onChange={(e) =>
                                                    setEditKitchenProfile({
                                                        ...editKitchenProfile,
                                                        weeklyWorkingHours: {
                                                            ...editKitchenProfile.weeklyWorkingHours,
                                                            [day]: {
                                                                ...editKitchenProfile.weeklyWorkingHours[
                                                                    day as keyof WeeklyWorkingHours
                                                                ],
                                                                open: e.target.value,
                                                            },
                                                        },
                                                    })
                                                }
                                            />
                                            <input
                                                type="time"
                                                className="p-2 border rounded-lg"
                                                value={hours.close}
                                                onChange={(e) =>
                                                    setEditKitchenProfile({
                                                        ...editKitchenProfile,
                                                        weeklyWorkingHours: {
                                                            ...editKitchenProfile.weeklyWorkingHours,
                                                            [day]: {
                                                                ...editKitchenProfile.weeklyWorkingHours[
                                                                    day as keyof WeeklyWorkingHours
                                                                ],
                                                                close: e.target.value,
                                                            },
                                                        },
                                                    })
                                                }
                                            />
                                        </div>
                                    ))}
                                </div>
                            </div>
                        </div>

                        <div className="flex justify-end gap-3 sticky bottom-0 bg-white pt-4">
                            <button
                                onClick={() => setShowKitchenEditModal(false)}
                                className="px-4 py-2 rounded-lg bg-gray-200 hover:bg-gray-300"
                            >
                                Anulează
                            </button>
                            <button
                                onClick={submitKitchenProfileUpdate}
                                disabled={loadingKitchenUpdate}
                                className="px-4 py-2 rounded-lg bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
                            >
                                {loadingKitchenUpdate ? "Se salvează..." : "Salvează"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Profile;
