import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL;

export const fetchArticlesByStatus = async (status) => {
    try {
        const response = await axios.get(`${API_BASE_URL}/api/Article/status/${status}`);
        return response.data;
    } catch (error) {
        console.error('Failed to fetch articles:', error);
        throw error;
    }
};

export const fetchArticles = async (filters) => {
    try {
        const queryParams = new URLSearchParams(filters).toString();
        const response = await axios.get(`${API_BASE_URL}/api/Article?${queryParams}`);
        return response.data;
    } catch (error) {
        console.error('Failed to fetch articles:', error);
        throw error;
    }
};

export const updateArticleStatus = async (ids, status) => {
    try {
        const response = await axios.patch(`${API_BASE_URL}/api/Article/bulk/status`, {
            ids,
            status
        });
        return response.data;
    } catch (error) {
        console.error('Failed to update article status:', error);
        throw error;
    }
};


export const fetchArticleDetails = async (id) => {
    try {
        const response = await axios.get(`${API_BASE_URL}/api/Article/${id}`);
        return response.data;
    } catch (error) {
        console.error('Failed to fetch article details:', error);
        throw error;
    }
};

export const fetchComments = async (articleId) => {
    try {
        const response = await axios.get(`${API_BASE_URL}/api/Article/${articleId}/comments`);
        return response.data;
    } catch (error) {
        console.error('Failed to fetch comments:', error);
        throw error;
    }
};


export const postComment = async (articleId, comment) => {
    try {
        const response = await axios.post(`/api/articles/${articleId}/comments`, { comment });
        return response.data;
    } catch (error) {
        console.error('Failed to post comment:', error);
        throw error;
    }
};

export const postReply = async (commentId, reply) => {
    try {
        const response = await axios.post(`/api/comments/${commentId}/replies`, { reply });
        return response.data;
    } catch (error) {
        console.error('Failed to post reply:', error);
        throw error;
    }
};

export const likeArticle = async (articleId) => {
    try {
        const response = await axios.post(`${API_BASE_URL}/api/Article/${articleId}/like`);
        return response.data;
    } catch (error) {
        console.error('Failed to like article:', error);
        throw error;
    }
};

export const dislikeArticle = async (articleId) => {
    try {
        const response = await axios.post(`${API_BASE_URL}/api/Article/${articleId}/dislike`);
        return response.data;
    } catch (error) {
        console.error('Failed to dislike article:', error);
        throw error;
    }
};
