import React, { useState, useEffect } from 'react';
import { useLocation, useNavigate, useOutletContext } from 'react-router-dom';
import ArticleList from './ArticleList';
import Pagination from '../AdminPortal/Pagination';
import { articleService } from '../../services/articleService';
import Loading from '../Loading';

const HomePage = () => {
    const [articles, setArticles] = useState([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [category, setCategory] = useState('all');
    const [isLoading, setIsLoading] = useState(false);
    const location = useLocation();
    const navigate = useNavigate();
    const { searchQuery } = useOutletContext();

    useEffect(() => {
        const queryParams = new URLSearchParams(location.search);
        const categoryParam = queryParams.get('category') || 'all';
        const pageParam = parseInt(queryParams.get('page'), 10) || 1;
        setCategory(categoryParam);
        setCurrentPage(pageParam);
        fetchArticlesData(categoryParam, searchQuery, pageParam);
    }, [location.search]);

    useEffect(() => {
        fetchArticlesData(category, searchQuery, currentPage);
        navigate(`?category=${category}&page=${currentPage}`);
    }, [currentPage, category, searchQuery, navigate]);

    const fetchArticlesData = async (categoryParam, searchQuery, pageNumber) => {
        setIsLoading(true);
        try {
            const data = await articleService.fetchArticles(categoryParam, searchQuery, pageNumber);
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
            {isLoading && <Loading />} {/* Display loading overlay when isLoading is true */}
            <ArticleList articles={articles} currentPage={currentPage} />
            <Pagination
                pageNumber={currentPage}
                pageSize={10}
                totalCount={totalCount}
                onPageChange={setCurrentPage}
                isLoading={isLoading}
            />
        </div>
    );
};

export default HomePage;
