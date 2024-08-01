import React, { useState, useEffect } from 'react';
import { addReaction, updateReaction, removeReaction } from '../../services/api';
import { useAuth } from '../../context/AuthContext';

const LikeButton = ({ articleId, likes, setLikes, dislikes, setDislikes, userReaction, setUserReaction }) => {
    const { token } = useAuth(); // Assuming you have a token in your AuthContext

    const handleLike = async () => {
        try {
            if (userReaction === 'Like') {
                // If already liked, remove like
                await removeReaction(articleId, token);
                setLikes(prevLikes => prevLikes - 1);
                setUserReaction(null);
            } else if (userReaction === 'Dislike') {
                // If disliked, update to like
                await updateReaction(articleId, 'Like', token);
                setLikes(prevLikes => prevLikes + 1);
                setDislikes(prevDislikes => prevDislikes - 1);
                setUserReaction('Like');
            } else {
                // If no reaction, add like
                await addReaction(articleId, 'Like', token);
                setLikes(prevLikes => prevLikes + 1);
                setUserReaction('Like');
            }
        } catch (error) {
            console.error('Failed to like article:', error);
        }
    };

    const handleDislike = async () => {
        try {
            if (userReaction === 'Dislike') {
                // If already disliked, remove dislike
                await removeReaction(articleId, token);
                setDislikes(prevDislikes => prevDislikes - 1);
                setUserReaction(null);
            } else if (userReaction === 'Like') {
                // If liked, update to dislike
                await updateReaction(articleId, 'Dislike', token);
                setLikes(prevLikes => prevLikes - 1);
                setDislikes(prevDislikes => prevDislikes + 1);
                setUserReaction('Dislike');
            } else {
                // If no reaction, add dislike
                await addReaction(articleId, 'Dislike', token);
                setDislikes(prevDislikes => prevDislikes + 1);
                setUserReaction('Dislike');
            }
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
