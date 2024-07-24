import React from 'react';
import PropTypes from 'prop-types';
import '../../styles/ArticleCard.css';

const truncateContent = (content, wordLimit) => {
    const words = content.split(' ');
    if (words.length > wordLimit) {
        return words.slice(0, wordLimit).join(' ') + '...';
    }
    return content;
};

const ArticleCard = ({ article }) => {
    return (
        <div className="article-card">
            <img src={article.imageUrl} alt={article.title} className="article-image" />
            <div className="article-content">
                <h2 className="article-title">{article.title}</h2>
                <p className="article-author">By {article.author}</p>
                <p className="article-date">{article.date} at {article.time}</p>
                <p className="article-summary">{truncateContent(article.content, 100)}</p>
                <a href={article.readMoreUrl} className="read-more-link">Read more</a>
            </div>
        </div>
    );
};

ArticleCard.propTypes = {
    article: PropTypes.shape({
        author: PropTypes.string.isRequired,
        content: PropTypes.string.isRequired,
        date: PropTypes.string.isRequired,
        id: PropTypes.string.isRequired,
        imageUrl: PropTypes.string.isRequired,
        readMoreUrl: PropTypes.string.isRequired,
        time: PropTypes.string.isRequired,
        title: PropTypes.string.isRequired,
        url: PropTypes.string.isRequired
    }).isRequired
};

export default ArticleCard;
