// src/context/AuthContext.js
import React, { createContext, useContext, useState, useEffect } from 'react';
import { googleLogout } from '@react-oauth/google';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(() => {
        // Check localStorage for persisted user data
        const savedUser = localStorage.getItem('user');
        return savedUser ? JSON.parse(savedUser) : null;
    });
    const [profile, setProfile] = useState(() => {
        // Check localStorage for persisted profile data
        const savedProfile = localStorage.getItem('profile');
        return savedProfile ? JSON.parse(savedProfile) : null;
    });

    const logOut = () => {
        googleLogout();
        localStorage.removeItem('user');
        localStorage.removeItem('profile');
        setUser(null);
        setProfile(null);
    };

    // Save user and profile to localStorage when they change
    useEffect(() => {
        if (user) {
            localStorage.setItem('user', JSON.stringify(user));
        }
        if (profile) {
            localStorage.setItem('profile', JSON.stringify(profile));
        }
    }, [user, profile]);

    return (
        <AuthContext.Provider value={{ user, setUser, profile, setProfile, logOut }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
