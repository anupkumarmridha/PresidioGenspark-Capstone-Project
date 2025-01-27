// src/App.jsx
import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';
import { AuthProvider } from './context/AuthContext';
import {CommentsAndReactionsProvider} from './context/CommentsAndReactionsContext';
import RoutesConfig from './routes';
import './styles/main.css';
import '@fortawesome/fontawesome-free/css/all.min.css';
import LoginPrompt from './components/Authentication/LoginPrompt';

const App = () => {
    return (
        <GoogleOAuthProvider clientId={process.env.REACT_APP_GOOGLE_CLIENT_ID}>
            <AuthProvider>
                <CommentsAndReactionsProvider>
                <BrowserRouter>
                    <RoutesConfig />
                    <LoginPrompt />
                </BrowserRouter>
            </CommentsAndReactionsProvider>
            </AuthProvider>
        </GoogleOAuthProvider>
    );
};

export default App;
