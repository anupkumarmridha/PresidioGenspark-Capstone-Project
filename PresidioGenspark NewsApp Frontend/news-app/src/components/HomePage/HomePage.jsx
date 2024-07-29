// src/components/HomePage/HomePage.jsx
import React, { useState, useEffect } from 'react';
import { useLocation, useOutletContext } from 'react-router-dom';
import ArticleList from './ArticleList';
import Pagination from '../AdminPortal/Pagination';
import { articleService } from '../../services/articleService';

const HomePage = () => {
    const [articles, setArticles] = useState([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [category, setCategory] = useState('all');
    const [isLoading, setIsLoading] = useState(false);
    const location = useLocation();
    const { searchQuery } = useOutletContext(); // Get search query from Outlet context

    useEffect(() => {
        const queryParams = new URLSearchParams(location.search);
        const categoryParam = queryParams.get('category') || 'all';
        setCategory(categoryParam);
        setCurrentPage(1); // Reset to page 1 on category change

        fetchArticlesData(categoryParam, searchQuery, 1); // Fetch data with new page number
    }, [category, searchQuery, location.search]);

    const fetchArticlesData = async (categoryParam, searchQuery, pageNumber) => {
        setIsLoading(true);
        try {
            const data = await articleService.fetchArticles(categoryParam, searchQuery, pageNumber);
            console.log('Fetched data:', data);
            if (data.articles) {
                setArticles(data.articles);
                setTotalCount(data.totalCount);
            } else {
                setArticles([]);
                setTotalCount(0);
            }
        } catch (error) {
            console.error('Error fetching articles:', error);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="home-page">
            <ArticleList articles={articles} currentPage={currentPage} />
            <Pagination
                pageNumber={currentPage}
                pageSize={10} // Set the page size
                totalCount={totalCount}
                onPageChange={setCurrentPage}
                isLoading={isLoading}
            />
        </div>
    );
};

export default HomePage;
