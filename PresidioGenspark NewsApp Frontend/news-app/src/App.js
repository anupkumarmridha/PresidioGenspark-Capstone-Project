// src/App.jsx
import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { AuthProvider } from './context/AuthContext';
import RoutesConfig from './routes';
import './styles/main.css';
import '@fortawesome/fontawesome-free/css/all.min.css';

const App = () => {
    return (
        <GoogleOAuthProvider clientId={process.env.REACT_APP_GOOGLE_CLIENT_ID}>
            <AuthProvider>
                <BrowserRouter>
                    <RoutesConfig />
                </BrowserRouter>
            </AuthProvider>
        </GoogleOAuthProvider>
    );
};

export default App;