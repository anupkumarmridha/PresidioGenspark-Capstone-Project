import React, { useState, useEffect, useCallback } from 'react';
import { useCommentsAndReactions } from '../../context/CommentsAndReactionsContext';
import { useAuth } from '../../context/AuthContext';
import "../../styles/LikeButton.css";

const LikeButton = ({ articleId }) => {
    const { user, profile } = useAuth();
    const {
        likes: initialLikes,
        dislikes: initialDislikes,
        reactions,
        handleAddReaction,
        handleUpdateReaction,
        handleRemoveReaction,
        fetchReactions
    } = useCommentsAndReactions();

    const [userReaction, setUserReaction] = useState(null);
    const [likes, setLikes] = useState(initialLikes);
    const [dislikes, setDislikes] = useState(initialDislikes);

    useEffect(() => {
        // Fetch reactions whenever articleId changes
        const fetchAndSetReactions = async () => {
            await fetchReactions(articleId);
        };
        fetchAndSetReactions();
    }, [articleId, fetchReactions]);

    useEffect(() => {
        // Update userReaction and counts whenever reactions or profile change
        if (profile && reactions) {
            const reaction = reactions.find(r => r.userId === profile.id);
            setUserReaction(reaction);
        }
        // Update local counts based on reactions
        const likeCount = reactions.filter(r => r.reactionType === 0).length;
        const dislikeCount = reactions.filter(r => r.reactionType === 1).length;
        setLikes(likeCount);
        setDislikes(dislikeCount);
    }, [reactions, profile]);

    const handleLike = useCallback(async () => {
        if (!user) return alert('Please log in to react.');
        try {
            if (userReaction?.reactionType === 0) {
                await handleRemoveReaction(articleId);
                setLikes(prevLikes => prevLikes - 1);
            } else if (userReaction?.reactionType === 1) {
                await handleUpdateReaction(articleId, 0);
                setLikes(prevLikes => prevLikes + 1);
                setDislikes(prevDislikes => prevDislikes - 1);
            } else {
                await handleAddReaction(articleId, 0);
                setLikes(prevLikes => prevLikes + 1);
            }
            await fetchReactions(articleId); // Ensure reactions are updated
        } catch (error) {
            console.error('Error handling like reaction:', error);
        }
    }, [user, userReaction, articleId, handleAddReaction, handleUpdateReaction, handleRemoveReaction, fetchReactions]);

    const handleDislike = useCallback(async () => {
        if (!user) return alert('Please log in to react.');
        try {
            if (userReaction?.reactionType === 1) {
                await handleRemoveReaction(articleId);
                setDislikes(prevDislikes => prevDislikes - 1);
            } else if (userReaction?.reactionType === 0) {
                await handleUpdateReaction(articleId, 1);
                setLikes(prevLikes => prevLikes - 1);
                setDislikes(prevDislikes => prevDislikes + 1);
            } else {
                await handleAddReaction(articleId, 1);
                setDislikes(prevDislikes => prevDislikes + 1);
            }
            await fetchReactions(articleId); // Ensure reactions are updated
        } catch (error) {
            console.error('Error handling dislike reaction:', error);
        }
    }, [user, userReaction, articleId, handleAddReaction, handleUpdateReaction, handleRemoveReaction, fetchReactions]);

    return (
        <div className="like-button-container">
            <button 
                onClick={handleLike} 
                className={`like-button ${userReaction?.reactionType === 0 ? 'liked' : ''}`}
            >
                <i className={`fas fa-thumbs-up ${userReaction?.reactionType === 0 ? 'active' : ''}`}></i> {likes}
            </button>
            <button 
                onClick={handleDislike} 
                className={`dislike-button ${userReaction?.reactionType === 1 ? 'disliked' : ''}`}
            >
                <i className={`fas fa-thumbs-down ${userReaction?.reactionType === 1 ? 'active' : ''}`}></i> {dislikes}
            </button>
        </div>
    );
};

export default LikeButton;
