import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { authService } from '../../services/authService';

const Callback = () => {
    const { setUser } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        // Extract the token from the URL
        const urlParams = new URLSearchParams(window.location.search);
        const googleToken = urlParams.get('token');

        if (googleToken) {
            // Send the token to your backend for validation and login
            authService.googleLogin(googleToken)
                .then(response => {
                    setUser(response.data); // Save user info to context or state
                    navigate('/home'); // Redirect to home page
                })
                .catch(error => {
                    console.error("Google login failed", error);
                    // Handle errors, e.g., show a message to the user
                });
        } else {
            // Handle case where no token is present
            console.error("No token found in URL");
        }
    }, [navigate, setUser]);

    return (
        <div>
            <p>Processing authentication...</p>
        </div>
    );
};

export default Callback;
