// src/components/ProtectedRoute.jsx
import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const AdminProtectedRoute = ({ children }) => {
    const { user, profile } = useAuth();

    if (!user || profile?.role !== 'Admin') {
        return <Navigate to="/" />;
    }

    return children;
};

export default AdminProtectedRoute;
