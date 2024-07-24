import React from 'react';
import '../../styles/SearchBar.css'; // Ensure you have a CSS file for styling
import '@fortawesome/fontawesome-free/css/all.min.css';

const SearchBar = ({ onSearch }) => {
    const handleSearchChange = (event) => {
        onSearch(event.target.value);
    };

    return (
        <div className="search-bar">
            <input
                type="text"
                placeholder="Search articles..."
                onChange={handleSearchChange}
            />
            <button className="search-button">
                <i className="fas fa-search"></i>
            </button>
        </div>
    );
};

export default SearchBar;
