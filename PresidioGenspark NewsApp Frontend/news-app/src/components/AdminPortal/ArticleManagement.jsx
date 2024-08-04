import '../../styles/ArticleManagement.css';
import React, { useState, useEffect } from 'react';
import { fetchArticles, updateArticleStatus } from '../../services/api';
import Modal from 'react-modal';
import { FaSortUp, FaSortDown } from 'react-icons/fa';
import Pagination from './Pagination';
import Filters from './Filters';
import { toast } from 'react-toastify';
import { useOutletContext } from 'react-router-dom';
import Loading from '../Loading';
import { useAuth } from '../../context/AuthContext';

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
    const [pageNumber, setPageNumber] = useState(1);
    const [pageSize] = useState(10); // Adjust the page size as needed
    const [totalCount, setTotalCount] = useState(0);
    const [isLoading, setIsLoading] = useState(false);
    const { searchQuery } = useOutletContext(); // Get search query from Outlet context
    const { user } = useAuth();
    const token = user?.token;


    useEffect(() => {
        console.log("Search Query:", searchQuery);
        fetchArticlesData();
    }, [filterStatus, category, startDate, endDate, author, title, contentKeyword, searchQuery, sortConfig, pageNumber]);

    const fetchArticlesData = async () => {
        setIsLoading(true);
        try {
            const filters = {
                status: filterStatus,
                ...(category !== 'all' && { category }),
                startDate,
                endDate,
                author,
                title,
                contentKeyword: searchQuery || contentKeyword,
                pageNumber,
                pageSize
            };

            let data = await fetchArticles(filters);
            if (sortConfig.key) {
                data.articles.sort((a, b) => {
                    if (a[sortConfig.key] < b[sortConfig.key]) return sortConfig.direction === 'asc' ? -1 : 1;
                    if (a[sortConfig.key] > b[sortConfig.key]) return sortConfig.direction === 'asc' ? 1 : -1;
                    return 0;
                });
            }

            setArticles(data.articles);
            setTotalCount(data.totalCount);
            setSelectAll(false);
            setSelectedArticles([]);
        } catch (error) {
            console.error('Error fetching articles:', error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleSelectArticle = (id) => {
        setSelectedArticles((prev) =>
            prev.includes(id) ? prev.filter((articleId) => articleId !== id) : [...prev, id]
        );
    };

    const handleBulkUpdate = async () => {
        setModalIsOpen(true);
    };

    const confirmUpdate = async () => {
        setIsLoading(true); // Start loading for update
        try {
            await updateArticleStatus(selectedArticles, status, token);
            toast.success('Articles updated successfully');
    
            // Update the local state to reflect the changes
            setArticles((prevArticles) => 
                prevArticles.map((article) => 
                    selectedArticles.includes(article.id) ? { ...article, status } : article
                )
            );
    
            setSelectAll(false);
            setSelectedArticles([]);
        } catch (error) {
            toast.error('Error updating articles:', error);
        }
        setIsLoading(false); // Stop loading for update
        setModalIsOpen(false);
    };


    const cancelUpdate = () => {
        setModalIsOpen(false);
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

    const handleFilterChange = (filterName, value) => {
        switch (filterName) {
            case 'filterStatus':
                setFilterStatus(value);
                break;
            case 'category':
                setCategory(value);
                break;
            case 'author':
                setAuthor(value);
                break;
            case 'title':
                setTitle(value);
                break;
            case 'contentKeyword':
                setContentKeyword(value);
                break;
            case 'startDate':
                setStartDate(value);
                break;
            case 'endDate':
                setEndDate(value);
                break;
            default:
                break;
        }
        setPageNumber(1);
    };

    return (
        <div className="article-management">
            <h1>Article Management</h1>
            {isLoading && <Loading />}
            <Filters
                filterStatus={filterStatus}
                category={category}
                categories={categories}
                author={author}
                title={title}
                contentKeyword={contentKeyword}
                startDate={startDate}
                endDate={endDate}
                onFilterChange={handleFilterChange}
            />
            <div className="status-update">
                <label htmlFor="status">Status to Update:</label>
                <select id="status" value={status} onChange={(e) => setStatus(e.target.value)}>
                    <option value="approved">Approve</option>
                    <option value="rejected">Reject</option>
                </select>
                <button onClick={handleBulkUpdate} disabled={selectedArticles.length === 0}>
                    Update Status
                </button>
            </div>
            <table className="article-table">
                <thead>
                    <tr>
                        <th>
                            <input type="checkbox" checked={selectAll} onChange={handleSelectAll} />
                        </th>
                        <th onClick={() => handleSort('title')}>
                            Title {sortConfig.key === 'title' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
                        <th onClick={() => handleSort('author')}>
                            Author {sortConfig.key === 'author' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
                        <th onClick={() => handleSort('publishedAt')}>
                            Date {sortConfig.key === 'publishedAt' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
                        <th>Content</th>
                        <th onClick={() => handleSort('category')}>
                            Category {sortConfig.key === 'category' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
                        <th onClick={() => handleSort('status')}>
                            Status {sortConfig.key === 'status' && (sortConfig.direction === 'asc' ? <FaSortUp /> : <FaSortDown />)}
                        </th>
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
                            <td>   {truncateContent(article.content)}{' '}
                                <a href={article.readMoreUrl} target="_blank" rel="noopener noreferrer">
                                    Read More
                                </a></td>
                            <td>{article.category}</td>
                            <td>{article.status}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
            <Pagination
                pageNumber={pageNumber}
                pageSize={pageSize}
                totalCount={totalCount}
                onPageChange={setPageNumber}
                isLoading={isLoading}
            />
            <Modal
                isOpen={modalIsOpen}
                onRequestClose={cancelUpdate}
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
