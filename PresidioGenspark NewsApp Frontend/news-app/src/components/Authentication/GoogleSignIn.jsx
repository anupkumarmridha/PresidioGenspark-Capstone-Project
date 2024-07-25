import React, { useState } from 'react';
import { useGoogleLogin } from '@react-oauth/google';
import { useAuth } from '../../context/AuthContext';
import '../../styles/GoogleSignIn.css';
import { authService } from '../../services/authService';
import { toast } from 'react-toastify';

const GoogleSignIn = () => {
    const { setUser, setProfile } = useAuth();
    const [loading, setLoading] = useState(false);

    const login = useGoogleLogin({
        onSuccess: async (response) => {
            setLoading(true); // Set loading state
            try {
                await authenticateUser(response.access_token); // Call the API to authenticate
                toast.success('Login successful!');
            } catch (error) {
                toast.error('Authentication failed. Please try again.');
            } finally {
                setLoading(false); // Reset loading state
            }
        },
        onError: (error) => {
            toast.error('Login failed. Please try again.');
            console.log('Login Failed:', error);
        }
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
            throw err; // Re-throw error to handle it in login onSuccess
        }
    };

    return (
        <button onClick={() => login()} className="google-signin-button" disabled={loading}>
            {loading ? 'Loading...' : 'Sign in with Google'}
        </button>
    );
};

export default GoogleSignIn;
