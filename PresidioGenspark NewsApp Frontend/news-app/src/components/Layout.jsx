// src/components/Layout.jsx
import React, { useState } from 'react';
import NavBar from './NavBar/NavBar';
import { Outlet } from 'react-router-dom'; // React Router v6

const Layout = () => {
    const [searchQuery, setSearchQuery] = useState('');

    const handleSearch = (query) => {
        setSearchQuery(query);
    };

    return (
        <div>
            <NavBar onSearch={handleSearch} />
            <main>
                <Outlet context={{ searchQuery }} /> {/* Pass searchQuery to child routes */}
            </main>
        </div>
    );
};

export default Layout;
