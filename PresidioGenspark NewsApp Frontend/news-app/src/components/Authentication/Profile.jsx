import React from 'react';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import '../../styles/Profile.css'; // Import the CSS file

const Profile = () => {
    const { profile, logOut } = useAuth();
    const navigate = useNavigate(); // Hook for programmatic navigation

    const handleLogout = () => {
        logOut();
        navigate('/'); // Redirect to HomePage after logout
    };

    return (
        profile ? (
            <div className="profile-container">
                <img src={profile.picture} alt="user" />
                <h3>User Logged In</h3>
                <p>Name: {profile.name}</p>
                <p>Email Address: {profile.email}</p>
                <button onClick={handleLogout}>Log Out</button>
            </div>
        ) : null
    );
};

export default Profile;
