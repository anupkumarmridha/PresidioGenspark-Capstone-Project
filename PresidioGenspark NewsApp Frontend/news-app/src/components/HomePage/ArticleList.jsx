import React from 'react';
import ArticleCard from './ArticleCard'; // Adjust the import path as needed
import '../../styles/ArticleList.css';

const ArticleList = ({ articles, currentPage }) => {
    // Number of articles to display per page
    const articlesPerPage = 6;
    
    // Calculate start and end index for slicing articles
    const startIndex = (currentPage - 1) * articlesPerPage;
    const endIndex = startIndex + articlesPerPage;
    
    // Slice the articles to display only those for the current page
    const currentArticles = articles.slice(startIndex, endIndex);

    if (!currentArticles.length) {
        return <div>No articles available.</div>;
    }

    return (
        <div className="article-list">
            {currentArticles.map(article => (
                <ArticleCard key={article.id} article={article} />
            ))}
        </div>
    );
};

export default ArticleList;
