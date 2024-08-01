import React, { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Comments from './Comments';
import LikeButton from './LikeButton';
import { useCommentsAndReactions } from '../../context/CommentsAndReactionsContext';
import { useAuth } from '../../context/AuthContext';
import "../../styles/ArticleDetails.css";

const Article = () => {
    const { articleId } = useParams();
    const { user } = useAuth();
    const {
        article,
        likes,
        dislikes,
        likeUsers,
        dislikeUsers,
        commentsCount,
        fetchArticle, // Fetch the article details
        fetchComments
    } = useCommentsAndReactions();

    useEffect(() => {
        fetchArticle(articleId);
        fetchComments(articleId);
    }, [articleId, fetchArticle, fetchComments]);

    if (!article) {
        return <p>Loading...</p>;
    }

    const handleReadMoreClick = (e) => {
        // Prevent default link behavior if needed
        if (article.readMoreUrl) {
            window.open(article.readMoreUrl, '_blank', 'noopener,noreferrer');
        }
    };


    return (
        <div className="article-container">
            <h1>{article.title}</h1>
            <p>By {article.author}</p>
            <p>{new Date(article.date).toLocaleDateString()}</p>
            <img src={article.imageUrl} alt={article.title} />
            <p>{article.content}</p>
            <p className="read-more-link">
                <a href={article.readMoreUrl || '#'} onClick={handleReadMoreClick}>
                    Read More
                </a>
            </p>
            {user ? (
                <LikeButton articleId={articleId} />
            ) : (
                <div>
                    <p>Likes: {likes}</p>
                    <p>Dislikes: {dislikes}</p>
                    <p>Liked by: {likeUsers.join(', ')}</p>
                    <p>Disliked by: {dislikeUsers.join(', ')}</p>
                </div>
            )}
            <p>{commentsCount} Comments</p>
            <Comments articleId={articleId} />
        </div>
    );
};

export default Article;
