import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { fetchArticleDetails, fetchArticleReactions } from '../../services/api';
import Comments from './Comments';
import LikeButton from './LikeButton';
import { useAuth } from '../../context/AuthContext';

const Article = () => {
    const { articleId } = useParams();
    const { user, token } = useAuth(); // Assuming you have a token in your AuthContext
    const [article, setArticle] = useState(null);
    const [likes, setLikes] = useState(0);
    const [dislikes, setDislikes] = useState(0);
    const [likeUsers, setLikeUsers] = useState([]);
    const [dislikeUsers, setDislikeUsers] = useState([]);
    const [userReaction, setUserReaction] = useState(null); // Track user's current reaction
    const [commentsCount, setCommentsCount] = useState(0);

    useEffect(() => {
        const fetchDetails = async () => {
            try {
                const articleData = await fetchArticleDetails(articleId);
                setArticle(articleData);
                setLikes(articleData.likes);
                setDislikes(articleData.dislikes);
                setCommentsCount(articleData.comments.length);

                // Fetch reactions
                const reactions = await fetchArticleReactions(articleId);
                setLikeUsers(reactions.filter(r => r.reactionType === 0).map(r => r.userName));
                setDislikeUsers(reactions.filter(r => r.reactionType === 1).map(r => r.userName));
                setLikes(likeUsers.length);
                setDislikes(dislikeUsers.length);

                // Fetch user's current reaction
                const userReaction = reactions.find(r => r.userName === user.userName);
                setUserReaction(userReaction ? userReaction.reactionType : null);
            } catch (error) {
                console.error('Failed to fetch article details:', error);
            }
        };

        fetchDetails();
    }, [articleId, likeUsers, dislikeUsers, user.userName]);

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
                    userReaction={userReaction}
                    setUserReaction={setUserReaction}
                />
            ) : (
                <div>
                    <p>Likes: {likes}</p>
                    <p>Dislikes: {dislikes}</p>
                    <p>Liked by: {likeUsers.join(', ')}</p>
                    <p>Disliked by: {dislikeUsers.join(', ')}</p>
                </div>
            )}
            <p>{commentsCount} Comments</p>
            <Comments articleId={articleId} setCommentsCount={setCommentsCount} isLoggedIn={!!user} />
        </div>
    );
};

export default Article;
