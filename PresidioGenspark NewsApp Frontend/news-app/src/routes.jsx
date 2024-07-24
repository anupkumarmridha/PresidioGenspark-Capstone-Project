import React from 'react';
import { Route, Routes } from 'react-router-dom';
import HomePage from './components/HomePage/HomePage';
import GoogleSignIn from './components/Authentication/GoogleSignIn';
import Profile from './components/Authentication/Profile';
import ProtectedRoute from './components/ProtectedRoute';

const RoutesConfig = () => (
    <Routes>
        <Route path="/signin" element={<GoogleSignIn />} />
        <Route path="/profile" element={<Profile />} />
        <Route path="/" element={<HomePage />} />
        <Route path="*" element={<HomePage />} /> {/* Default route */}
        {/* Add more routes as needed */}
    </Routes>
);

export default RoutesConfig;
