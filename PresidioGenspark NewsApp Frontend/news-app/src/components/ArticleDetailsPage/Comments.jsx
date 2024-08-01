// src/components/Comments.jsx
import React, { useState, useEffect } from 'react';
import { postCommentOrReply, fetchComments } from '../../services/api';

const Comments = ({ articleId, setCommentsCount, isLoggedIn }) => {
    console.log('articleId:', articleId);
    const [comments, setComments] = useState([]);
    const [newComment, setNewComment] = useState('');
    const [newReply, setNewReply] = useState({});

    useEffect(() => {
        const fetchArticleComments = async () => {
            const fetchedComments = await fetchComments(articleId);
            setComments(fetchedComments);
            setCommentsCount(fetchedComments.length);
        };

        fetchArticleComments();
    }, [articleId, setCommentsCount]);

    const handleCommentChange = (e) => setNewComment(e.target.value);

    const handleReplyChange = (commentId, e) => {
        setNewReply({ ...newReply, [commentId]: e.target.value });
    };

    const handlePostComment = async () => {
        try {
            const comment = await postCommentOrReply(articleId, newComment);
            setComments([...comments, comment]);
            setNewComment('');
            setCommentsCount(comments.length + 1);
        } catch (error) {
            console.error('Error posting comment:', error);
        }
    };

    const handlePostReply = async (commentId) => {
        try {
            const reply = await postCommentOrReply(articleId, newReply[commentId], commentId);
            setComments(comments.map(comment =>
                comment.id === commentId ? { ...comment, replies: [...comment.replies, reply] } : comment
            ));
            setNewReply({ ...newReply, [commentId]: '' });
        } catch (error) {
            console.error('Error posting reply:', error);
        }
    };

    return (
        <div>
            {isLoggedIn ? (
                <div>
                    <input
                        type="text"
                        value={newComment}
                        onChange={handleCommentChange}
                        placeholder="Add a comment"
                    />
                    <button onClick={handlePostComment}>Post Comment</button>
                </div>
            ) : (
                <p>Log in to post comments and replies.</p>
            )}
            {comments.map(comment => (
                <div key={comment.id}>
                    <p>{comment.content}</p>
                    {isLoggedIn && (
                        <div>
                            <input
                                type="text"
                                value={newReply[comment.id] || ''}
                                onChange={(e) => handleReplyChange(comment.id, e)}
                                placeholder="Reply"
                            />
                            <button onClick={() => handlePostReply(comment.id)}>Post Reply</button>
                        </div>
                    )}
                    {comment.replies && comment.replies.map(reply => (
                        <div key={reply.id}>
                            <p>{reply.content}</p>
                        </div>
                    ))}
                </div>
            ))}
        </div>
    );
};

export default Comments;
