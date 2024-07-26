import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import SearchBar from './SearchBar';
import GoogleSignIn from '../Authentication/GoogleSignIn';
import { useAuth } from '../../context/AuthContext';
import '../../styles/Navbar.css';

const NavBar = ({ onSearch, onCategoryChange }) => {
    const { user, profile, logOut } = useAuth();
    const [showProfileMenu, setShowProfileMenu] = useState(false);

    const handleProfileMenuToggle = () => setShowProfileMenu(!showProfileMenu);

    return (
        <nav className="navbar">
            <div className="navbar-brand">
                <Link to="/">NewsApp</Link>
            </div>
            <SearchBar onSearch={onSearch} />
            <div className="navbar-buttons">
                <button onClick={() => onCategoryChange('sports')}>Sports</button>
                <button onClick={() => onCategoryChange('business')}>Business</button>
                <button onClick={() => onCategoryChange('technology')}>Technology</button>
                {/* Add more buttons as needed */}
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
