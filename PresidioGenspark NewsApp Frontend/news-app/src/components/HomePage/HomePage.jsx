// src/components/HomePage/HomePage.jsx
import React, { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import ArticleList from './ArticleList';
import Pagination from './Pagination';
import { articleService } from '../../services/articleService';

const HomePage = () => {
    const [articles, setArticles] = useState([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [searchQuery, setSearchQuery] = useState('');
    const [category, setCategory] = useState('all'); // Default to all categories
    const location = useLocation(); // Get query parameters

    useEffect(() => {
        const queryParams = new URLSearchParams(location.search);
        const categoryParam = queryParams.get('category') || 'all';
        setCategory(categoryParam);

        const fetchArticlesData = async () => {
            try {
                const data = await articleService.fetchArticles(categoryParam, searchQuery);
                console.log(data);
                if (data.data) { // Ensure articles exist
                    setArticles(data.data);
                    const totalArticles = data.data.length;
                    setTotalPages(Math.ceil(totalArticles / 6)); // Calculate total pages based on articles
                } else {
                    setArticles([]); // Set to empty array if no articles are found
                }
            } catch (error) {
                console.error('Error fetching articles', error);
                setArticles([]); // Handle errors gracefully
            }
        };

        fetchArticlesData();
    }, [searchQuery, category, location.search]);

    const handleSearch = (query) => {
        setSearchQuery(query);
        setCurrentPage(1); // Reset to first page on new search
    };

    return (
        <div className="home-page">
            <ArticleList articles={articles} currentPage={currentPage} />
            <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={setCurrentPage}
            />
        </div>
    );
};

export default HomePage;
