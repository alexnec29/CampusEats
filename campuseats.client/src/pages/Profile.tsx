import React from "react";
import { useProfileLogic } from "../hooks/useProfileLogic";
import { LoadingSpinner } from "../components/common/LoadingSpinner";
import { ProfileHeader, UserInfoCard } from "../components/common/ProfileComponents";
import { BuyerProfileCard } from "../components/common/BuyerProfileCard";
import { KitchenProfileCard } from "../components/common/KitchenProfileCard";
import { AccountSettingsCard } from "../components/common/AccountSettingsCard";
import { PasswordChangeModal } from "../components/common/PasswordChangeModal";
import { BuyerProfileEditModal } from "../components/common/BuyerProfileEditModal";
import { KitchenProfileEditModal } from "../components/common/KitchenProfileEditModal";

const Profile: React.FC = () => {
    const {
        user,
        buyerProfile,
        kitchenProfile,
        loyaltyPoints,
        loading,
        showPasswordModal,
        setShowPasswordModal,
        currentPassword,
        setCurrentPassword,
        newPassword,
        setNewPassword,
        confirmPassword,
        setConfirmPassword,
        loadingPassword,
        passwordError,
        submitPasswordChange,
        closePasswordModal,
        showBuyerEditModal,
        setShowBuyerEditModal,
        editBuyerProfile,
        setEditBuyerProfile,
        loadingBuyerUpdate,
        openBuyerEditModal,
        submitBuyerProfileUpdate,
        showKitchenEditModal,
        setShowKitchenEditModal,
        editKitchenProfile,
        setEditKitchenProfile,
        loadingKitchenUpdate,
        openKitchenEditModal,
        submitKitchenProfileUpdate,
        handleDeleteAccount,
    } = useProfileLogic();

    if (!user || loading) {
        return <LoadingSpinner />;
    }

    return (
        <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 page-transition">
            <div className="max-w-4xl mx-auto space-y-8 p-6">
                <ProfileHeader user={user} />

                <UserInfoCard user={user} loyaltyPoints={loyaltyPoints} />

                {user.role === "Buyer" && (
                    <BuyerProfileCard buyerProfile={buyerProfile} onEdit={openBuyerEditModal} />
                )}

                {user.role === "Kitchen" && (
                    <KitchenProfileCard kitchenProfile={kitchenProfile} onEdit={openKitchenEditModal} />
                )}

                <AccountSettingsCard
                    onChangePassword={() => setShowPasswordModal(true)}
                    onDeleteAccount={handleDeleteAccount}
                />

                <PasswordChangeModal
                    isOpen={showPasswordModal}
                    onClose={closePasswordModal}
                    currentPassword={currentPassword}
                    setCurrentPassword={setCurrentPassword}
                    newPassword={newPassword}
                    setNewPassword={setNewPassword}
                    confirmPassword={confirmPassword}
                    setConfirmPassword={setConfirmPassword}
                    onSubmit={submitPasswordChange}
                    loading={loadingPassword}
                    error={passwordError}
                />

                <BuyerProfileEditModal
                    isOpen={showBuyerEditModal}
                    onClose={() => setShowBuyerEditModal(false)}
                    profile={editBuyerProfile}
                    setProfile={setEditBuyerProfile}
                    onSubmit={submitBuyerProfileUpdate}
                    loading={loadingBuyerUpdate}
                    isNewProfile={!buyerProfile}
                />

                <KitchenProfileEditModal
                    isOpen={showKitchenEditModal}
                    onClose={() => setShowKitchenEditModal(false)}
                    profile={editKitchenProfile}
                    setProfile={setEditKitchenProfile}
                    onSubmit={submitKitchenProfileUpdate}
                    loading={loadingKitchenUpdate}
                    isNewProfile={!kitchenProfile}
                />
            </div>
        </div>
    );
};

export default Profile;
