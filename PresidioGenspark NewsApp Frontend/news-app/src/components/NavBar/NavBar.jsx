// src/components/NavBar/NavBar.jsx
import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import SearchBar from './SearchBar';
import GoogleSignIn from '../Authentication/GoogleSignIn';
import { useAuth } from '../../context/AuthContext';
import '../../styles/Navbar.css';

const NavBar = ({ onSearch }) => {
    const { user, profile, logOut } = useAuth();
    const [showProfileMenu, setShowProfileMenu] = useState(false);
    const [isMenuOpen, setIsMenuOpen] = useState(false);
    const navigate = useNavigate();

    const handleProfileMenuToggle = () => setShowProfileMenu(!showProfileMenu);
    const handleMenuToggle = () => setIsMenuOpen(!isMenuOpen);

    const handleCategoryChange = (category) => {
        navigate(`/?category=${category}`);
        setIsMenuOpen(false);
    };

    return (
        <nav className={`navbar ${isMenuOpen ? 'open' : ''}`}>
            <div className="navbar-toggle" onClick={handleMenuToggle}>
                <i className="fas fa-bars"></i>
            </div>
            <div className="navbar-brand">
                <Link to="/">NewsApp</Link>
            </div>
            <SearchBar onSearch={onSearch} />
            <div className={`navbar-buttons ${isMenuOpen ? 'open' : ''}`}>
                <button onClick={() => handleCategoryChange('sports')}>Sports</button>
                <button onClick={() => handleCategoryChange('business')}>Business</button>
                <button onClick={() => handleCategoryChange('entertainment')}>Entertainment</button>
                <button onClick={() => handleCategoryChange('technology')}>Technology</button>
            </div>
            <div className="navbar-auth">
                {user ? (
                    <div className="profile-menu">
                        <button onClick={handleProfileMenuToggle} className="profile-button">
                            <i className="fas fa-user"></i> Profile
                        </button>
                        {showProfileMenu && (
                            <div className="profile-dropdown">
                                <Link to="/profile">Profile</Link>
                                {profile?.role === 'Admin' && (
                                    <Link to="/admin-portal">Admin Portal</Link>
                                )}
                                <button onClick={logOut}>Logout</button>
                            </div>
                        )}
                    </div>
                ) : (
                    <GoogleSignIn />
                )}
            </div>
        </nav>
    );
};

export default NavBar;
