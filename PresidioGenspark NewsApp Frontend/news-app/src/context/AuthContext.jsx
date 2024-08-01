// src/context/AuthContext.js
import React, { createContext, useContext, useState, useEffect } from 'react';
import { googleLogout } from '@react-oauth/google';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(() => {
        const savedUser = sessionStorage.getItem('user');
        return savedUser ? JSON.parse(savedUser) : null;
    });
    const [profile, setProfile] = useState(() => {
        const savedProfile = sessionStorage.getItem('profile');
        return savedProfile ? JSON.parse(savedProfile) : null;
    });

    const logOut = () => {
        googleLogout();
        sessionStorage.removeItem('user');
        sessionStorage.removeItem('profile');
        setUser(null);
        setProfile(null);
    };

    useEffect(() => {
        console.log('User:', user);
        console.log('Profile:', profile);

        if (user) {
            sessionStorage.setItem('user', JSON.stringify(user));
            sessionStorage.setItem('token', user.token); // Save token from user
        }
        if (profile) {
            sessionStorage.setItem('profile', JSON.stringify(profile));
        }
    }, [user, profile]);

    return (
        <AuthContext.Provider value={{ user, setUser, profile, setProfile, logOut }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
