import React from 'react';
import { Route, Routes } from 'react-router-dom';
import HomePage from './components/HomePage/HomePage';
import GoogleSignIn from './components/Authentication/GoogleSignIn';
import Profile from './components/Authentication/Profile';
import ProtectedRoute from './components/ProtectedRoute';
import ArticleManagement from './components/AdminPortal/ArticleManagement';
import AdminProtectedRoute from './components/AdminPortal/AdminProtectedRoute';
import Layout from './components/Layout';

const RoutesConfig = () => (
    <Routes>
        <Route path="/" element={<Layout />}>
            <Route index element={<HomePage />} />
            <Route path="/signin" element={<GoogleSignIn />} />
            <Route 
                path="/profile" 
                element={
                    <ProtectedRoute>
                        <Profile />
                    </ProtectedRoute>
                } 
            />
            <Route
                path="/admin-portal"
                element={
                    <AdminProtectedRoute>
                        <ArticleManagement />
                    </AdminProtectedRoute>
                }
            />
        </Route>
        <Route path="*" element={<HomePage />} /> {/* Default route */}
    </Routes>
);

export default RoutesConfig;
