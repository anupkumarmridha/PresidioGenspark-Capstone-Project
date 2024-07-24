import React, { useState, useEffect } from 'react';
import ArticleList from './ArticleList';
import Pagination from './Pagination';
import NavBar from '../NavBar/NavBar';
import { articleService } from '../../services/articleService';

const HomePage = () => {
    const [articles, setArticles] = useState([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [searchQuery, setSearchQuery] = useState('');
    const [category, setCategory] = useState('all'); // Default to all categories

    useEffect(() => {
        const fetchArticlesData = async () => {
            try {
                const data = await articleService.fetchArticles(category, searchQuery);
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
    }, [searchQuery, category]);

    const handleSearch = (query) => {
        setSearchQuery(query);
        setCurrentPage(1); // Reset to first page on new search
    };

    const handleCategoryChange = (newCategory) => {
        setCategory(newCategory);
        setCurrentPage(1); // Reset to first page on category change
    };

    return (
        <div className="home-page">
            <NavBar onSearch={handleSearch} onCategoryChange={handleCategoryChange} />
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
