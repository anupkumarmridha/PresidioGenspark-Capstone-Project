import React from 'react';
import '../../styles/ArticleManagement.css';

const Filters = ({
    filterStatus,
    category,
    categories,
    author,
    title,
    contentKeyword,
    startDate,
    endDate,
    onFilterChange,
}) => {
    return (
        <div className="filter-container">
            <div className="filter-group">
                <label htmlFor="filterStatus">Filter Status:</label>
                <select id="filterStatus" value={filterStatus} onChange={(e) => onFilterChange('filterStatus', e.target.value)}>
                    <option value="all">All</option>
                    <option value="pending">Pending</option>
                    <option value="approved">Approved</option>
                    <option value="rejected">Rejected</option>
                </select>
            </div>
            <div className="filter-group">
                <label htmlFor="category">Category:</label>
                <select id="category" value={category} onChange={(e) => onFilterChange('category', e.target.value)}>
                    {categories.map((cat) => (
                        <option key={cat} value={cat}>{cat}</option>
                    ))}
                </select>
            </div>
            <div className="filter-group">
                <label htmlFor="author">Author:</label>
                <input type="text" id="author" value={author} onChange={(e) => onFilterChange('author', e.target.value)} />
            </div>
            <div className="filter-group">
                <label htmlFor="title">Title:</label>
                <input type="text" id="title" value={title} onChange={(e) => onFilterChange('title', e.target.value)} />
            </div>
            <div className="filter-group">
                <label htmlFor="contentKeyword">Content Keyword:</label>
                <input type="text" id="contentKeyword" value={contentKeyword} onChange={(e) => onFilterChange('contentKeyword', e.target.value)} />
            </div>
            <div className="filter-group">
                <label htmlFor="startDate">Start Date:</label>
                <input type="date" id="startDate" value={startDate} onChange={(e) => onFilterChange('startDate', e.target.value)} />
            </div>
            <div className="filter-group">
                <label htmlFor="endDate">End Date:</label>
                <input type="date" id="endDate" value={endDate} onChange={(e) => onFilterChange('endDate', e.target.value)} />
            </div>
        </div>
    );
};

export default Filters;
