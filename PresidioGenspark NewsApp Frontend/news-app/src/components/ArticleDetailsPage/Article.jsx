// src/components/Article.jsx
import React, { useEffect, useState } from 'react';
import { fetchArticleDetails } from '../../services/api';
import Comments from './Comments';
import LikeButton from './LikeButton';
import { useAuth } from '../../context/AuthContext';

const Article = ({ articleId }) => {
    const { user } = useAuth();
    const [article, setArticle] = useState(null);
    const [likes, setLikes] = useState(0);
    const [dislikes, setDislikes] = useState(0);
    const [commentsCount, setCommentsCount] = useState(0);

    useEffect(() => {
        const fetchDetails = async () => {
            try {
                const articleData = await fetchArticleDetails(articleId);
                setArticle(articleData);
                setLikes(articleData.likes);
                setDislikes(articleData.dislikes);
                setCommentsCount(articleData.comments.length);
            } catch (error) {
                console.error('Failed to fetch article details:', error);
            }
        };

        fetchDetails();
    }, [articleId]);

    if (!article) {
        return <p>Loading...</p>;
    }

    return (
        <div>
            <h1>{article.title}</h1>
            <p>By {article.author}</p>
            <p>{new Date(article.date).toLocaleDateString()}</p>
            <img src={article.imageUrl} alt={article.title} />
            <p>{article.content}</p>
            {user ? (
                <LikeButton
                    articleId={articleId}
                    likes={likes}
                    setLikes={setLikes}
                    dislikes={dislikes}
                    setDislikes={setDislikes}
                />
            ) : (
                <div>
                    <p>Likes: {likes}</p>
                    <p>Dislikes: {dislikes}</p>
                </div>
            )}
            <p>{commentsCount} Comments</p>
            <Comments articleId={articleId} setCommentsCount={setCommentsCount} isLoggedIn={!!user} />
        </div>
    );
};

export default Article;
