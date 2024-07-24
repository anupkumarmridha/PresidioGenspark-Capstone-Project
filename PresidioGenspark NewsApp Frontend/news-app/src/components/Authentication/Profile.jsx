// src/components/Authentication/Profile.jsx
import React from 'react';
import { useAuth } from '../../context/AuthContext';

const Profile = () => {
    const { profile, logOut } = useAuth();

    return (
        profile ? (
            <div>
                <img src={profile.picture} alt="user image" />
                <h3>User Logged in</h3>
                <p>Name: {profile.name}</p>
                <p>Email Address: {profile.email}</p>
                <br />
                <button onClick={logOut}>Log out</button>
            </div>
        ) : null
    );
};

export default Profile;
