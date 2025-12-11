import React, { useEffect, useState } from "react";
import { apiClient } from "../utils/apiClient";
import { Mail, Shield, Star, KeyRound, Trash2, Edit } from "lucide-react";

interface UserInfo {
    username: string;
    role: string;
    email?: string;
    loyaltyPoints?: number;
}

const Profile: React.FC = () => {
    const [user, setUser] = useState<UserInfo | null>(null);

    // Password modal state
    const [showPasswordModal, setShowPasswordModal] = useState(false);
    const [currentPassword, setCurrentPassword] = useState("");
    const [newPassword, setNewPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");
    const [loadingPassword, setLoadingPassword] = useState(false);

    // Load user info
    useEffect(() => {
        const loadUser = async () => {
            try {
                const res = await apiClient("/api/user/check-auth");
                if (res.ok) {
                    const data = await res.json();
                    setUser(data);
                }
            } catch (err) {
                console.error("Failed to load user", err);
            }
        };
        loadUser();
    }, []);

    if (!user) {
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
        if (!currentPassword || !newPassword || !confirmPassword) {
            alert("Toate câmpurile pentru parolă sunt obligatorii.");
            return;
        }

        if (newPassword !== confirmPassword) {
            alert("Noua parolă și confirmarea nu coincid.");
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
            } else {
                const text = await res.text();
                alert("Eroare: " + text);
            }
        } catch (err) {
            console.error(err);
            alert("Eroare la schimbarea parolei.");
        } finally {
            setLoadingPassword(false);
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
                    {user.loyaltyPoints !== undefined && (
                        <div className="flex items-center gap-3 text-gray-700">
                            <Star className="w-5 h-5 text-yellow-500" />
                            <span className="font-medium">Puncte loialitate:</span>
                            {user.loyaltyPoints}
                        </div>
                    )}
                </div>
            </div>

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
                                onClick={() => setShowPasswordModal(false)}
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
        </div>
    );
};

export default Profile;
