import React, { useState, useEffect } from 'react';
import { useCommentsAndReactions } from '../../context/CommentsAndReactionsContext';
import { useAuth } from '../../context/AuthContext';
import "../../styles/Comment.css";

const Comments = ({ articleId }) => {
    const {
        comments,
        handlePostComment,
        handleUpdateComment,
        handleDeleteComment,
        fetchComments
    } = useCommentsAndReactions();
    const { user, profile } = useAuth();
    const [newComment, setNewComment] = useState('');
    const [replyTo, setReplyTo] = useState(null);
    const [replyText, setReplyText] = useState('');
    const [editCommentId, setEditCommentId] = useState(null);
    const [editCommentText, setEditCommentText] = useState('');

    useEffect(() => {
        fetchComments(articleId);
    }, [articleId, fetchComments, comments]);

    const handleCommentChange = (e) => setNewComment(e.target.value);
    const handleReplyChange = (e) => setReplyText(e.target.value);
    const handleEditChange = (e) => setEditCommentText(e.target.value);

    const handlePostCommentClick = () => {
        if (user) {
            handlePostComment(articleId, newComment);
            setNewComment('');
        } else {
            alert('Please log in to post a comment.');
        }
    };

    const handleReplyClick = (commentId) => {
        setReplyTo(commentId);
        setReplyText('');
    };

    const handlePostReplyClick = () => {
        if (user && replyTo !== null) {
            handlePostComment(articleId, replyText, replyTo);
            setReplyTo(null);
            setReplyText('');
        } else {
            alert('Please log in to post a reply.');
        }
    };

    const handleEditClick = (commentId, currentText) => {
        setEditCommentId(commentId);
        setEditCommentText(currentText);
    };

    const handleUpdateCommentClick = () => {
        if (editCommentId && editCommentText.trim()) {
            // Find the comment or reply to be updated
            const commentOrReply = comments.find(comment =>
                comment.id === editCommentId ||
                (comment.replies && comment.replies.some(reply => reply.id === editCommentId))
            );

            if (commentOrReply) {
                let parentId = null;

                // Check if editCommentId is for a reply
                if (commentOrReply.replies && commentOrReply.replies.some(reply => reply.id === editCommentId)) {
                    parentId = commentOrReply.id;
                }

                const data = {
                    articleId: articleId,
                    content: editCommentText,
                    parentId: parentId
                };

                handleUpdateComment(editCommentId, data);
                setEditCommentId(null);
                setEditCommentText('');
            }
        }
    };


    const handleDeleteClick = (commentId) => {
        if (window.confirm('Are you sure you want to delete this comment?')) {
            handleDeleteComment(commentId);
        }
    };

    return (
        <div className="comments-container">
            {user ? (
                <div>
                    <input
                        type="text"
                        value={newComment}
                        onChange={handleCommentChange}
                        placeholder="Add a comment"
                    />
                    <button onClick={handlePostCommentClick}>
                        <i className="fas fa-paper-plane">
                        </i> Post Comment</button>
                </div>
            ) : (
                <p>Please log in to post a comment.</p>
            )}
            {comments.map(comment => (
                <div key={comment.id} className="comment">
                    {editCommentId === comment.id ? (
                        <div>
                            <input
                                type="text"
                                value={editCommentText}
                                onChange={handleEditChange}
                                placeholder="Edit your comment"
                            />
                            <button onClick={handleUpdateCommentClick}>
                                <i className="fas fa-save"></i> Update
                            </button>
                            <button onClick={() => setEditCommentId(null)}>
                                <i className="fas fa-times"></i> Cancel
                            </button>
                        </div>
                    ) : (
                        <div>
                            <p><strong>{comment.userName}</strong>: {comment.content}</p>
                            <p><small>{new Date(comment.createdAt).toLocaleString()}</small></p>
                            {user && (
                                <div className="comment-actions">
                                    <button onClick={() => handleReplyClick(comment.id)}>
                                        <i className="fas fa-reply"></i> Reply
                                    </button>
                                    {profile.id === comment.userId && (
                                        <>
                                            <button
                                                className="edit-button"
                                                onClick={() => handleEditClick(comment.id, comment.content)}>
                                                <i className="fas fa-edit"></i> Edit
                                            </button>
                                            <button
                                                className="delete-button"
                                                onClick={() => handleDeleteClick(comment.id)}>
                                                <i className="fas fa-trash"></i> Delete
                                            </button>
                                        </>
                                    )}
                                </div>
                            )}
                        </div>
                    )}
                    {replyTo === comment.id && (
                        <div className="reply-box">
                            <input
                                type="text"
                                value={replyText}
                                onChange={handleReplyChange}
                                placeholder="Add a reply"
                            />
                            <button onClick={handlePostReplyClick}>Post Reply</button>
                        </div>
                    )}
                    {comment.replies && comment.replies.map(reply => (
                        <div key={reply.id} className="reply-container">
                            {editCommentId === reply.id ? (
                                <div>
                                    <input
                                        type="text"
                                        value={editCommentText}
                                        onChange={handleEditChange}
                                        placeholder="Edit your reply"
                                    />
                                    <button onClick={handleUpdateCommentClick}>
                                        <i className="fas fa-save"></i> Update                                        </button>
                                    <button onClick={() => setEditCommentId(null)}>
                                        <i className="fas fa-times"></i> Cancel
                                    </button>
                                </div>
                            ) : (
                                <div>
                                    <p><strong>{reply.userName}</strong>: {reply.content}</p>
                                    <p><small>{new Date(reply.createdAt).toLocaleString()}</small></p>
                                    {user && profile.id === reply.userId && (
                                        <div className="comment-actions">
                                            <button
                                                className="edit-button"
                                                onClick={() => handleEditClick(reply.id, reply.content)}>
                                                <i className="fas fa-edit"></i> Edit
                                            </button>
                                            <button
                                                className="delete-button"
                                                onClick={() => handleDeleteClick(reply.id)}>
                                                <i className="fas fa-trash"></i> Delete
                                            </button>
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            ))}
        </div>
    );
};

export default Comments;
