// src/components/Authentication/LoginPrompt.jsx
import React, { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import GoogleSignIn from './GoogleSignIn';
import '../../styles/LoginPrompt.css';
const LoginPrompt = () => {
    const { user } = useAuth();
    const [showPrompt, setShowPrompt] = useState(false);

    useEffect(() => {
        // Show the prompt only if the user is not logged in
        if (!user) {
            setShowPrompt(true);
        }
    }, [user]);

    const handleClose = () => {
        setShowPrompt(false);
    };

    return (
        showPrompt && (
            <div className="login-prompt">
                <div className="login-prompt-content">
                    <p>Sign in to access more features</p>
                    <GoogleSignIn />
                    <button className="close-button" onClick={handleClose}>
                        <i className="fas fa-times"></i>
                    </button>
                </div>
            </div>
        )
    );
};

export default LoginPrompt;
