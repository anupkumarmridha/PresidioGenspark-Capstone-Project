import React from 'react';
import { useGoogleLogin } from '@react-oauth/google';
import { useAuth } from '../../context/AuthContext';
import axios from 'axios';
import '../../styles/GoogleSignIn.css';
import { authService } from '../../services/authService';

const GoogleSignIn = () => {
    const { setUser, setProfile } = useAuth();

    const login = useGoogleLogin({
        onSuccess: async (response) => {
            await authenticateUser(response.access_token); // Call the API to authenticate
        },
        onError: (error) => console.log('Login Failed:', error)
    });

    const authenticateUser = async (googleToken) => {
        try {
            const res = await authService.googleLogin(googleToken);
            // Handle response from your backend (e.g., store user info, tokens)
            console.log('User authenticated:', res.data);

            // Assuming the response contains both token and profile
            const { token, profile } = res.data;

            // Set user and profile information
            setUser({ token });
            setProfile(profile);
        } catch (err) {
            console.log('Authentication failed:', err);
        }
    };

    return (
        <button onClick={() => login()} className="google-signin-button">
            Sign in with Google
        </button>
    );
};

export default GoogleSignIn;
