import React from 'react';
import { useGoogleLogin } from '@react-oauth/google';
import { useAuth } from '../../context/AuthContext';
import axios from 'axios';
import '../../styles/GoogleSignIn.css'; // Import the CSS file

const GoogleSignIn = () => {
    const { setUser, setProfile } = useAuth();

    const login = useGoogleLogin({
        onSuccess: async (response) => {
            setUser(response);
            await fetchProfile(response.access_token);
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
        <button onClick={() => login()} className="google-signin-button">
            Sign in with Google
        </button>
    );
};

export default GoogleSignIn;

