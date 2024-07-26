// src/components/Layout.jsx
import React from 'react';
import NavBar from './NavBar/NavBar';
import { Outlet } from 'react-router-dom'; // React Router v6

const Layout = () => {
    return (
        <div>
            <NavBar />
            <main>
                <Outlet /> {/* Renders the current route's component */}
            </main>
        </div>
    );
};

export default Layout;
