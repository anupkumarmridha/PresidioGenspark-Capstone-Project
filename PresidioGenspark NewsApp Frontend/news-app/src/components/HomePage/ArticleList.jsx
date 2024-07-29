import React from 'react';
import ArticleCard from './ArticleCard'; // Adjust the import path as needed
import '../../styles/ArticleList.css';

const ArticleList = ({ articles }) => {
    if (!articles.length) {
        return <div>No articles available.</div>;
    }

    return (
        <div className="article-list">
            {articles.map(article => (
                <ArticleCard key={article.id} article={article} />
            ))}
        </div>
    );
};

export default ArticleList;
