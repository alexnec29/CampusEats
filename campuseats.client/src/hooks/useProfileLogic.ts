import { useState, useEffect, useCallback } from 'react';
import { userService } from '../services/userService';
import { profileService } from '../services/profileService';
import { UserInfo, BuyerProfile, KitchenProfile } from '../types/profileTypes';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';

export const useProfileLogic = () => {
    const { showToast } = useToast();
    const { confirm } = useConfirm();
    
    const [user, setUser] = useState<UserInfo | null>(null);
    const [buyerProfile, setBuyerProfile] = useState<BuyerProfile | null>(null);
    const [kitchenProfile, setKitchenProfile] = useState<KitchenProfile | null>(null);
    const [loyaltyPoints, setLoyaltyPoints] = useState<number | null>(null);
    const [loading, setLoading] = useState(true);

    const [showPasswordModal, setShowPasswordModal] = useState(false);
    const [currentPassword, setCurrentPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [loadingPassword, setLoadingPassword] = useState(false);
    const [passwordError, setPasswordError] = useState('');

    const [showBuyerEditModal, setShowBuyerEditModal] = useState(false);
    const [editBuyerProfile, setEditBuyerProfile] = useState<BuyerProfile | null>(null);
    const [loadingBuyerUpdate, setLoadingBuyerUpdate] = useState(false);

    const [showKitchenEditModal, setShowKitchenEditModal] = useState(false);
    const [editKitchenProfile, setEditKitchenProfile] = useState<KitchenProfile | null>(null);
    const [loadingKitchenUpdate, setLoadingKitchenUpdate] = useState(false);

    const loadBuyerProfile = useCallback(async () => {
        try {
            const profile = await profileService.getBuyerProfile();
            setBuyerProfile(profile);
        } catch (err) {
            console.error('Failed to load buyer profile', err);
        }
    }, []);

    const loadKitchenProfile = useCallback(async () => {
        try {
            const profile = await profileService.getKitchenProfile();
            setKitchenProfile(profile);
        } catch (err) {
            console.error('Failed to load kitchen profile', err);
        }
    }, []);

    const loadLoyaltyPoints = useCallback(async () => {
        try {
            const points = await profileService.getLoyaltyPoints();
            setLoyaltyPoints(points);
        } catch (err) {
            console.error('Failed to load loyalty account', err);
        }
    }, []);

    const loadUserData = useCallback(async () => {
        try {
            setLoading(true);
            const userData = await userService.checkAuth();
            if (userData) {
                setUser(userData);
                
                if (userData.role === 'Buyer') {
                    await Promise.all([
                        loadBuyerProfile(),
                        loadLoyaltyPoints(),
                    ]);
                } else if (userData.role === 'Kitchen') {
                    await loadKitchenProfile();
                }
            }
        } catch (err) {
            console.error('Failed to load user', err);
        } finally {
            setLoading(false);
        }
    }, [loadBuyerProfile, loadKitchenProfile, loadLoyaltyPoints]);

    useEffect(() => {
        loadUserData();
    }, [loadUserData]);

    const validatePasswordChange = (): string | null => {
        if (!currentPassword || !newPassword || !confirmPassword) {
            return 'Toate câmpurile pentru parolă sunt obligatorii.';
        }
        if (newPassword !== confirmPassword) {
            return 'Noua parolă și confirmarea nu coincid.';
        }
        if (newPassword.length < 6) {
            return 'Noua parolă trebuie să aibă cel puțin 6 caractere.';
        }
        return null;
    };

    const submitPasswordChange = async () => {
        setPasswordError('');
        
        const validationError = validatePasswordChange();
        if (validationError) {
            setPasswordError(validationError);
            return;
        }

        setLoadingPassword(true);
        try {
            const result = await userService.changePassword({
                currentPassword,
                newPassword,
                confirmNewPassword: confirmPassword,
            });

            if (result.success) {
                showToast('Parola a fost schimbată cu succes!', 'success');
                closePasswordModal();
            } else {
                setPasswordError(result.error || 'Eroare la schimbarea parolei.');
            }
        } catch (err) {
            console.error(err);
            setPasswordError('Eroare la schimbarea parolei.');
        } finally {
            setLoadingPassword(false);
        }
    };

    const closePasswordModal = () => {
        setShowPasswordModal(false);
        setPasswordError('');
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
    };

    const getDefaultBuyerProfile = (): BuyerProfile => ({
        lastName: '',
        firstName: '',
        age: 18,
        deliveryAddress: {
            street: '',
            building: '',
            city: '',
            county: '',
        },
    });

    const openBuyerEditModal = () => {
        if (buyerProfile) {
            setEditBuyerProfile({ ...buyerProfile });
        } else {
            setEditBuyerProfile(getDefaultBuyerProfile());
        }
        setShowBuyerEditModal(true);
    };

    const submitBuyerProfileUpdate = async () => {
        if (!editBuyerProfile) return;

        setLoadingBuyerUpdate(true);
        try {
            const result = await profileService.updateBuyerProfile(editBuyerProfile);

            if (result.success) {
                showToast('Profilul de cumpărător a fost actualizat cu succes!', 'success');
                setShowBuyerEditModal(false);
                await loadBuyerProfile();
            } else {
                showToast('Eroare: ' + result.error, 'error');
            }
        } catch (err) {
            console.error(err);
            showToast('Eroare la actualizarea profilului.', 'error');
        } finally {
            setLoadingBuyerUpdate(false);
        }
    };

    const getDefaultKitchenProfile = (): KitchenProfile => {
        const defaultHours = { open: '09:00', close: '17:00' };
        return {
            companyName: '',
            kitchenAddress: {
                street: '',
                building: '',
                city: '',
                county: '',
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
        };
    };

    const openKitchenEditModal = () => {
        if (kitchenProfile) {
            setEditKitchenProfile({ ...kitchenProfile });
        } else {
            setEditKitchenProfile(getDefaultKitchenProfile());
        }
        setShowKitchenEditModal(true);
    };

    const submitKitchenProfileUpdate = async () => {
        if (!editKitchenProfile) return;

        setLoadingKitchenUpdate(true);
        try {
            const result = await profileService.updateKitchenProfile(editKitchenProfile);

            if (result.success) {
                showToast('Profilul de bucătărie a fost actualizat cu succes!', 'success');
                setShowKitchenEditModal(false);
                await loadKitchenProfile();
            } else {
                showToast('Eroare: ' + result.error, 'error');
            }
        } catch (err) {
            console.error(err);
            showToast('Eroare la actualizarea profilului.', 'error');
        } finally {
            setLoadingKitchenUpdate(false);
        }
    };

    const handleDeleteAccount = async () => {
        const confirmed = await confirm({
            title: 'Șterge Cont',
            message: 'Funcționalitatea de ștergere cont nu este încă implementată. Ești sigur că vrei să continui?',
            confirmText: 'Șterge',
            type: 'danger'
        });
        
        if (confirmed) {
            showToast('Funcționalitatea de ștergere cont nu este încă implementată.', 'info');
        }
    };

    return {
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
    };
};
