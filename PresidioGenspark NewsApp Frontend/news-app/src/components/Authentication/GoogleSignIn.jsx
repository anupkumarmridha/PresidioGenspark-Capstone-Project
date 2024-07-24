// src/components/Authentication/GoogleSignIn.jsx
import React from 'react';
import { useGoogleLogin } from '@react-oauth/google';
import { useAuth } from '../../context/AuthContext';
import axios from 'axios';

const GoogleSignIn = () => {
    const { setUser, setProfile } = useAuth();

    const login = useGoogleLogin({
        onSuccess: (response) => {
            setUser(response);
            fetchProfile(response.access_token);
        },
        onError: (error) => console.log('Login Failed:', error)
    });

    const fetchProfile = async (accessToken) => {
        try {
            const res = await axios.get(`https://www.googleapis.com/oauth2/v1/userinfo?access_token=${accessToken}`, {
                headers: {
                    Authorization: `Bearer ${accessToken}`,
                    Accept: 'application/json'
                }
            });
            setProfile(res.data);
        } catch (err) {
            console.log(err);
        }
    };

    return (
        <button onClick={() => login()}>Sign in with Google 🚀</button>
    );
};

export default GoogleSignIn;
