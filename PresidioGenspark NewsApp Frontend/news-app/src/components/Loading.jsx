import React from 'react';
import '../styles/Loading.css'; // Import CSS file for styling

const Loading = () => {
    return (
        <div className="loading-overlay">
            <div className="loading-spinner"></div>
            <div className="loading-text">Loading...</div>
        </div>
    );
};

export default Loading;
