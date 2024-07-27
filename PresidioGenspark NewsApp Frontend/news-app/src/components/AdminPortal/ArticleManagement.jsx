import '../../styles/ArticleManagement.css';
import React, { useState, useEffect } from 'react';
import { fetchArticles, updateArticleStatus } from '../../services/api';
import Modal from 'react-modal';
import { FaSortUp, FaSortDown } from 'react-icons/fa'; // Import icons

const categories = ["all", "business", "sports", "technology", "entertainment"];
const truncateContent = (content) => {
    const words = content.split(' ');
    if (words.length <= 50) return content;
    return words.slice(0, 50).join(' ') + '...';
};

const ArticleManagement = () => {
    const [articles, setArticles] = useState([]);
    const [selectedArticles, setSelectedArticles] = useState([]);
    const [filterStatus, setFilterStatus] = useState('all');
    const [category, setCategory] = useState('all');
    const [startDate, setStartDate] = useState('');
    const [endDate, setEndDate] = useState('');
    const [author, setAuthor] = useState('');
    const [title, setTitle] = useState('');
    const [contentKeyword, setContentKeyword] = useState('');
    const [selectAll, setSelectAll] = useState(false);
    const [status, setStatus] = useState('approved');
    const [modalIsOpen, setModalIsOpen] = useState(false);
    const [sortConfig, setSortConfig] = useState({ key: '', direction: 'asc' });

    useEffect(() => {
        fetchArticlesData();
    }, [filterStatus, category, startDate, endDate, author, title, contentKeyword, sortConfig]);

    const fetchArticlesData = async () => {
        try {
            const filters = {
                status: filterStatus,
                ...(category !== 'all' && { category }),
                startDate,
                endDate,
                author,
                title,
                contentKeyword,
            };

            let data = await fetchArticles(filters);

            if (sortConfig.key) {
                data.sort((a, b) => {
                    if (a[sortConfig.key] < b[sortConfig.key]) return sortConfig.direction === 'asc' ? -1 : 1;
                    if (a[sortConfig.key] > b[sortConfig.key]) return sortConfig.direction === 'asc' ? 1 : -1;
                    return 0;
                });
            }

            setArticles(data);
            setSelectAll(false);
            setSelectedArticles([]);
        } catch (error) {
            console.error('Error fetching articles:', error);
        }
    };

    const handleSelectArticle = (id) => {
        setSelectedArticles((prev) =>
            prev.includes(id) ? prev.filter((articleId) => articleId !== id) : [...prev, id]
        );
    };

    const handleBulkUpdate = async () => {
        setModalIsOpen(true); // Open the modal
    };

    const confirmUpdate = async () => {
        try {
            await updateArticleStatus(selectedArticles, status);
            alert('Articles updated successfully');
            fetchArticlesData();
        } catch (error) {
            console.error('Error updating articles:', error);
        }
        setModalIsOpen(false); // Close the modal
    };

    const cancelUpdate = () => {
        setModalIsOpen(false); // Close the modal
    };

    const handleSelectAll = () => {
        setSelectAll(!selectAll);
        setSelectedArticles(selectAll ? [] : articles.map((article) => article.id));
    };

    const handleSort = (column) => {
        const newDirection = sortConfig.key === column
            ? (sortConfig.direction === 'asc' ? 'desc' : 'asc')
            : 'asc';
        setSortConfig({ key: column, direction: newDirection });
    };

    return (
        <div className="article-management">
            <h1>Article Management</h1>
            <div className="filter-container">
                <div className="filter-group">
                    <label htmlFor="filterStatus">Filter Status:</label>
                    <select id="filterStatus" value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}>
                        <option value="all">All</option>
                        <option value="pending">Pending</option>
                        <option value="approved">Approved</option>
                        <option value="rejected">Rejected</option>
                    </select>
                </div>
                <div className="filter-group">
                    <label htmlFor="category">Category:</label>
                    <select id="category" value={category} onChange={(e) => setCategory(e.target.value)}>
                        {categories.map((cat) => (
                            <option key={cat} value={cat}>{cat}</option>
                        ))}
                    </select>
                </div>
                <div className="filter-group">
                    <label htmlFor="author">Author:</label>
                    <input type="text" id="author" value={author} onChange={(e) => setAuthor(e.target.value)} />
                </div>
                <div className="filter-group">
                    <label htmlFor="title">Title:</label>
                    <input type="text" id="title" value={title} onChange={(e) => setTitle(e.target.value)} />
                </div>
                <div className="filter-group">
                    <label htmlFor="contentKeyword">Content Keyword:</label>
                    <input type="text" id="contentKeyword" value={contentKeyword} onChange={(e) => setContentKeyword(e.target.value)} />
                </div>
                <div className="filter-group">
                    <label htmlFor="startDate">Start Date:</label>
                    <input type="date" id="startDate" value={startDate} onChange={(e) => setStartDate(e.target.value)} />
                </div>
                <div className="filter-group">
                    <label htmlFor="endDate">End Date:</label>
                    <input type="date" id="endDate" value={endDate} onChange={(e) => setEndDate(e.target.value)} />
                </div>
            </div>
            <div className="status-update">
                <label htmlFor="status">Status to Update:</label>
                <select id="status" value={status} onChange={(e) => setStatus(e.target.value)}>
                    <option value="approved">Approve</option>
                    <option value="rejected">Reject</option>
                </select>
                <button onClick={handleBulkUpdate}>Update Status</button>
            </div>
           <table className="article-table">
                <thead>
                    <tr>
                        <th>
                            <input
                                type="checkbox"
                                checked={selectAll}
                                onChange={handleSelectAll}
                            />
                        </th>
                        <th onClick={() => handleSort('title')}>
                            Title
                            {sortConfig.key === 'title' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
                        <th onClick={() => handleSort('author')}>
                            Author
                            {sortConfig.key === 'author' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
                        <th onClick={() => handleSort('date')}>
                            Date
                            {sortConfig.key === 'date' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
                        <th onClick={() => handleSort('category')}>
                            Category
                            {sortConfig.key === 'category' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
                        <th onClick={() => handleSort('status')}>
                            Status
                            {sortConfig.key === 'status' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
                        <th>Content</th>
                        <th>Image</th>
                    </tr>
                </thead>
                <tbody>
                    {articles.map((article) => (
                        <tr key={article.id}>
                            <td>
                                <input
                                    type="checkbox"
                                    checked={selectedArticles.includes(article.id)}
                                    onChange={() => handleSelectArticle(article.id)}
                                />
                            </td>
                            <td>{article.title}</td>
                            <td>{article.author}</td>
                            <td>{new Date(article.date).toLocaleDateString()}</td>
                            <td>{article.category}</td>
                            <td>{article.status}</td>
                            <td>
                                {truncateContent(article.content)}{' '}
                                <a href={article.readMoreUrl} target="_blank" rel="noopener noreferrer">
                                    Read More
                                </a>
                            </td>
                            <td>
                                <img
                                    src={article.imageUrl}
                                    alt={article.title}
                                    style={{ maxWidth: '100px', maxHeight: '75px' }}
                                />
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>

            <Modal
                isOpen={modalIsOpen}
                onRequestClose={() => setModalIsOpen(false)}
                contentLabel="Confirm Status Update"
                className="modal"
                overlayClassName="overlay"
            >
                <h2>Confirm Status Update</h2>
                <p>Do you want to continue?</p>
                <div style={{ textAlign: 'center' }}>
                    <button onClick={confirmUpdate}>Yes</button>
                    <button onClick={cancelUpdate}>No</button>
                </div>
            </Modal>

        </div>
    );
};

export default ArticleManagement;
