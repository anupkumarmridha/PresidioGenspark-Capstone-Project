// src/components/Comments.jsx
import React, { useState, useEffect } from 'react';
import { postComment, postReply, fetchComments } from '../../services/api';

const Comments = ({ articleId, setCommentsCount, isLoggedIn }) => {
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
        const comment = await postComment(articleId, newComment);
        setComments([...comments, comment]);
        setNewComment('');
        setCommentsCount(comments.length + 1);
    };

    const handlePostReply = async (commentId) => {
        const reply = await postReply(commentId, newReply[commentId]);
        setComments(comments.map(comment =>
            comment.id === commentId ? { ...comment, replies: [...comment.replies, reply] } : comment
        ));
        setNewReply({ ...newReply, [commentId]: '' });
    };

    return (
        <div>
            {isLoggedIn ? (
                <div>
                    <input type="text" value={newComment} onChange={handleCommentChange} placeholder="Add a comment" />
                    <button onClick={handlePostComment}>Post Comment</button>
                </div>
            ) : (
                <p>Log in to post comments and replies.</p>
            )}
            {comments.map(comment => (
                <div key={comment.id}>
                    <p>{comment.text}</p>
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
                    {comment.replies.map(reply => (
                        <div key={reply.id}>
                            <p>{reply.text}</p>
                        </div>
                    ))}
                </div>
            ))}
        </div>
    );
};

export default Comments;
