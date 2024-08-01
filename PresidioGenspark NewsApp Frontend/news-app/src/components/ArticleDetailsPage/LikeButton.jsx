import React from 'react';
import { useCommentsAndReactions } from '../../context/CommentsAndReactionsContext';
import { useAuth } from '../../context/AuthContext';
import "../../styles/LikeButton.css"; // Ensure this import path is correct

const LikeButton = ({ articleId }) => {
    const { user } = useAuth();
    const {
        likes,
        dislikes,
        reactions,
        handleAddReaction,
        handleUpdateReaction,
        handleRemoveReaction
    } = useCommentsAndReactions();

    const userReaction = reactions.find(r => r.userId === user.id);

    const handleLike = async () => {
        if (!user) return alert('Please log in to react.');
        if (userReaction?.reactionType === 0) {
            await handleRemoveReaction(articleId);
        } else if (userReaction?.reactionType === 1) {
            await handleUpdateReaction(articleId, 0);
        } else {
            await handleAddReaction(articleId, 0);
        }
    };

    const handleDislike = async () => {
        if (!user) return alert('Please log in to react.');
        if (userReaction?.reactionType === 1) {
            await handleRemoveReaction(articleId);
        } else if (userReaction?.reactionType === 0) {
            await handleUpdateReaction(articleId, 1);
        } else {
            await handleAddReaction(articleId, 1);
        }
    };

    return (
        <div className="like-button-container">
            <button onClick={handleLike}>Like ({likes})</button>
            <button onClick={handleDislike}>Dislike ({dislikes})</button>
        </div>
    );
};

export default LikeButton;
