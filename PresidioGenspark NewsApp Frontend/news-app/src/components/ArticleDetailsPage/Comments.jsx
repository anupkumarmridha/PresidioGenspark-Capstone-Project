import React, { useState, useEffect } from 'react';
import { useCommentsAndReactions } from '../../context/CommentsAndReactionsContext';
import { useAuth } from '../../context/AuthContext';
import "../../styles/Comment.css";

const Comments = ({ articleId }) => {
    const {
        comments,
        handlePostComment,
        fetchComments
    } = useCommentsAndReactions();
    const { user } = useAuth();
    const [newComment, setNewComment] = useState('');
    const [replyTo, setReplyTo] = useState(null);
    const [replyText, setReplyText] = useState('');

    useEffect(() => {
        fetchComments(articleId);
    }, [articleId, fetchComments]);

    const handleCommentChange = (e) => setNewComment(e.target.value);
    const handleReplyChange = (e) => setReplyText(e.target.value);

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
            handlePostComment(articleId, replyText, replyTo); // Assuming `handlePostComment` can take a `parentId` for replies
            setReplyTo(null);
            setReplyText('');
        } else {
            alert('Please log in to post a reply.');
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
                    <button onClick={handlePostCommentClick}>Post Comment</button>
                </div>
            ) : (
                <p>Please log in to post a comment.</p>
            )}
            {comments.map(comment => (
                <div key={comment.id} className="comment">
                    <p><strong>{comment.user.displayName}</strong>: {comment.content}</p>
                    <p><small>{new Date(comment.createdAt).toLocaleString()}</small></p>
                    {user && <button onClick={() => handleReplyClick(comment.id)}>Reply</button>}
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
                            <p><strong>{reply.user.displayName}</strong>: {reply.content}</p>
                            <p><small>{new Date(reply.createdAt).toLocaleString()}</small></p>
                        </div>
                    ))}
                </div>
            ))}
        </div>
    );
};

export default Comments;
