import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import SearchBar from './SearchBar';
import '../../styles/Navbar.css';
const NavBar = ({ onSearch, onCategoryChange, isLoggedIn, onLogout }) => {
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
                {isLoggedIn ? (
                    <div className="profile-menu">
                        <button onClick={handleProfileMenuToggle} className="profile-button">
                            <i className="fas fa-user"></i> Profile
                        </button>
                        {showProfileMenu && (
                            <div className="profile-dropdown">
                                <Link to="/profile">Profile</Link>
                                <button onClick={onLogout}>Logout</button>
                            </div>
                        )}
                    </div>
                ) : (
                    <button className="login-button">Login</button>
                )}
            </div>
        </nav>
    );
};

export default NavBar;


