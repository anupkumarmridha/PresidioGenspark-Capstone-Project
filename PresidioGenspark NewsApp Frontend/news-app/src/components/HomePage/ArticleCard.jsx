import React from 'react';
import PropTypes from 'prop-types';
import '../../styles/ArticleCard.css';
import { Link } from 'react-router-dom';

const truncateContent = (content, wordLimit) => {
    const words = content.split(' ');
    if (words.length > wordLimit) {
        return words.slice(0, wordLimit).join(' ') + '...';
    }
    return content;
};

const ArticleCard = ({ article }) => {
    const handleReadMoreClick = (e) => {
        // Prevent default link behavior if needed
        if (article.readMoreUrl) {
            window.open(article.readMoreUrl, '_blank', 'noopener,noreferrer');
        }
    };

    return (
        <div className="article-card-container">
            <Link to={`/article/${article.id}`} className="article-card-link">
                <div className="article-card">
                    <img src={article.imageUrl} alt={article.title} className="article-image" />
                    <div className="article-content">
                        <h2 className="article-title">{article.title}</h2>
                        <p className="article-author">By {article.author}</p>
                        <p className="article-date">{new Date(article.date).toLocaleDateString()}</p>
                        <p className="article-summary">{truncateContent(article.content, 100)}</p>
                        <p className="read-more-link">
                            <a href={article.readMoreUrl || '#'} onClick={handleReadMoreClick}>
                                Read More
                            </a>
                        </p>
                    </div>
                </div>
            </Link>
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
        readMoreUrl: PropTypes.string,
        title: PropTypes.string.isRequired,
    }).isRequired
};

export default ArticleCard;
