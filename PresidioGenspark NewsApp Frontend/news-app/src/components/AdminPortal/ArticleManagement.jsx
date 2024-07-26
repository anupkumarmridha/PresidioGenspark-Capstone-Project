import React, { useState, useEffect } from 'react';
import { fetchArticlesByStatus, updateArticleStatus } from '../../services/api';

const ArticleManagement = () => {
    const [articles, setArticles] = useState([]);
    const [selectedArticles, setSelectedArticles] = useState([]);
    const [status, setStatus] = useState('Pending'); // Default to 'pending'
    const [filterStatus, setFilterStatus] = useState('Pending'); // For filtering articles

    useEffect(() => {
        // Fetch articles based on current filterStatus
        fetchArticles(filterStatus);
    }, [filterStatus]);

    const fetchArticles = async (status) => {
        try {
            const data = await fetchArticlesByStatus(status);
            setArticles(data);
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
        try {
            await updateArticleStatus(selectedArticles, status);
            // Refresh article list or show success message
            alert('Articles updated successfully');
            fetchArticles(filterStatus); // Refresh the list
        } catch (error) {
            console.error('Error updating articles:', error);
        }
    };

    return (
        <div>
            <h1>Article Management</h1>
            <div>
                <label htmlFor="filterStatus">Filter Status: </label>
                <select id="filterStatus" value={filterStatus} onChange={(e) => setFilterStatus(e.target.value)}>
                    <option value="pending">Pending</option>
                    <option value="approved">Approved</option>
                    <option value="rejected">Rejected</option>
                </select>
            </div>
            <div>
                <label htmlFor="status">Status to Update: </label>
                <select id="status" value={status} onChange={(e) => setStatus(e.target.value)}>
                    <option value="approved">Approve</option>
                    <option value="rejected">Reject</option>
                </select>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>Select</th>
                        <th>Title</th>
                        <th>Author</th>
                        <th>Date</th>
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
                        </tr>
                    ))}
                </tbody>
            </table>
            <button onClick={handleBulkUpdate}>Update Status</button>
        </div>
    );
};

export default ArticleManagement;
