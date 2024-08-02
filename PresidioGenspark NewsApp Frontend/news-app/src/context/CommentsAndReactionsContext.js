import React, { createContext, useState, useContext, useCallback } from 'react';
import { fetchArticleDetails, fetchArticleComments, postCommentOrReply, updateComment, deleteComment, addReaction, updateReaction, removeReaction, fetchArticleReactions } from '../services/api';
import { useAuth } from './AuthContext';

const CommentsAndReactionsContext = createContext();

export const CommentsAndReactionsProvider = ({ children }) => {
    const [comments, setComments] = useState([]);
    const [reactions, setReactions] = useState([]);
    const [article, setArticle] = useState(null);
    const [likes, setLikes] = useState(0);
    const [dislikes, setDislikes] = useState(0);
    const [likeUsers, setLikeUsers] = useState([]);
    const [dislikeUsers, setDislikeUsers] = useState([]);
    const [commentsCount, setCommentsCount] = useState(0);
    const { user } = useAuth();
    const token = user?.token;

    const fetchArticle = useCallback(async (articleId) => {
        const articleData = await fetchArticleDetails(articleId);
        // console.log(articleData);
        setArticle(articleData);
        setCommentsCount(articleData.totalComments);
        setLikes(articleData.totalLikes);
        setDislikes(articleData.totalDislikes);
    }, []);


    const fetchReactions = useCallback(async (articleId) => {
        try {
            // Fetch reactions and update state
            const reactionsData = await fetchArticleReactions(articleId);
            const likeUsersList = reactionsData.filter(r => r.reactionType === 0).map(r => r.userName);
            const dislikeUsersList = reactionsData.filter(r => r.reactionType === 1).map(r => r.userName);
            setLikeUsers(likeUsersList);
            setDislikeUsers(dislikeUsersList);

            setReactions(reactionsData);
        } catch (error) {
            console.error('Failed to fetch article Reactions:', error);
        }
    }, []);

    const fetchComments = useCallback(async (articleId) => {
        try {
            const comments = await fetchArticleComments(articleId);
            setComments(comments.map((comment, index) => ({
                ...comment,
                id: comment.id ?? `comment-${index}-${Date.now()}`
            })));
        } catch (error) {
            console.error('Failed to fetch article comments:', error);
        }
    }, []);

    const handlePostComment = useCallback(async (articleId, content, parentId = null) => {
        try {
            if (!token) {
                console.error('Token is not available');
                return;
            }
            const comment = await postCommentOrReply(articleId, content, token, parentId);
            setComments(prevComments => [
                ...prevComments,
                { ...comment, id: comment.id ?? `comment-${prevComments.length}-${Date.now()}` }
            ]);
            setCommentsCount(prevCount => prevCount + 1);
        } catch (error) {
            console.error('Error posting comment:', error);
        }
    }, [token]);
    

    const handleUpdateComment = useCallback(async (commentId, data) => {
        try {
            // console.log("comment id : ",commentId);
            // console.log(data);
            await updateComment(commentId, token, data);
            setComments(prevComments =>
                prevComments.map(comment => comment.id === commentId ? { ...comment, ...data } : comment)
            );
        } catch (error) {
            console.error('Error updating comment:', error);
        }
    }, [token]);

    const handleDeleteComment = useCallback(async (commentId) => {
        try {
            await deleteComment(commentId, token);
            setComments(prevComments => prevComments.filter(comment => comment.id !== commentId));
            setCommentsCount(prevCount => prevCount - 1);
        } catch (error) {
            console.error('Error deleting comment:', error);
        }
    }, [token]);

    const handleAddReaction = useCallback(async (articleId, reactionType) => {
        try {
            const updatedReaction = await addReaction(articleId, reactionType, token);
            setReactions(prevReactions => [...prevReactions, updatedReaction]);
            if (reactionType === 0) {
                setLikes(prevLikes => prevLikes + 1);
            } else {
                setDislikes(prevDislikes => prevDislikes + 1);
            }
        } catch (error) {
            console.error('Error adding reaction:', error);
        }
    }, [token]);

    const handleUpdateReaction = useCallback(async (articleId, reactionType) => {
        try {
            const updatedReaction = await updateReaction(articleId, reactionType, token);
            setReactions(prevReactions => prevReactions.map(reaction => reaction.articleId === articleId ? updatedReaction : reaction));
            // Adjust likes/dislikes counters accordingly
            if (reactionType === 0) {
                setLikes(prevLikes => prevLikes + 1);
                setDislikes(prevDislikes => prevDislikes - 1);
            } else {
                setLikes(prevLikes => prevLikes - 1);
                setDislikes(prevDislikes => prevDislikes + 1);
            }
        } catch (error) {
            console.error('Error updating reaction:', error);
        }
    }, [token]);

    const handleRemoveReaction = useCallback(async (articleId) => {
        try {
            await removeReaction(articleId, token);
            setReactions(prevReactions => prevReactions.filter(reaction => reaction.articleId !== articleId));
            // Adjust likes/dislikes counters accordingly
            const userReaction = reactions.find(r => r.articleId === articleId);
            if (userReaction.reactionType === 0) {
                setLikes(prevLikes => prevLikes - 1);
            } else {
                setDislikes(prevDislikes => prevDislikes - 1);
            }
        } catch (error) {
            console.error('Error removing reaction:', error);
        }
    }, [token, reactions]);

    return (
        <CommentsAndReactionsContext.Provider
            value={{
                comments,
                reactions,
                article,
                likes,
                dislikes,
                likeUsers,
                dislikeUsers,
                commentsCount,
                fetchArticle,
                fetchReactions,
                fetchComments,
                handlePostComment,
                handleUpdateComment,
                handleDeleteComment,
                handleAddReaction,
                handleUpdateReaction,
                handleRemoveReaction
            }}
        >
            {children}
        </CommentsAndReactionsContext.Provider>
    );
};

export const useCommentsAndReactions = () => useContext(CommentsAndReactionsContext);
