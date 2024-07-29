import React from 'react';
import { likeArticle, dislikeArticle } from '../../services/api';

const LikeButton = ({ articleId, likes, setLikes, dislikes, setDislikes }) => {
    const handleLike = async () => {
        try {
            await likeArticle(articleId);
            setLikes(likes + 1);
        } catch (error) {
            console.error('Failed to like article:', error);
        }
    };

    const handleDislike = async () => {
        try {
            await dislikeArticle(articleId);
            setDislikes(dislikes + 1);
        } catch (error) {
            console.error('Failed to dislike article:', error);
        }
    };

    return (
        <div>
            <button onClick={handleLike}>Like ({likes})</button>
            <button onClick={handleDislike}>Dislike ({dislikes})</button>
        </div>
    );
};

export default LikeButton;
