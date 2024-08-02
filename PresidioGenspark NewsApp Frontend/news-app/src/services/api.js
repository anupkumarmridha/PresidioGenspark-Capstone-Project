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

export const fetchArticleComments = async (articleId) => {
    try {
        const response = await axios.get(`${API_BASE_URL}/api/Comment/article/${articleId}`);
        return response.data;
    } catch (error) {
        console.error('Failed to fetch comments:', error);
        throw error;
    }
};

export const fetchArticleReactions = async (articleId)=> {
    const response = await fetch(`${API_BASE_URL}/api/Reaction/get?articleId=${articleId}`);
    if (!response.ok) {
        throw new Error('Failed to fetch article reactions');
    }
    return response.json();
}


export const postCommentOrReply = async (articleId, content, token, parentId = null) => {
    try {
        const response = await axios.post(`${API_BASE_URL}/api/Comment/add`, {
            articleId,
            content,
            parentId,
        }, {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });
        return response.data;
    } catch (error) {
        console.error('Failed to post comment or reply:', error);
        throw error;
    }
};

export const updateComment= async (commentId, token, data) => {
    // console.log("comment Id API: ",commentId);
    // console.log("data API: ",data);
    const response = await fetch(`${API_BASE_URL}/api/Comment/update/${commentId}`, {
        method: 'PUT',
        headers: {
            'Accept': 'text/plain',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(data)
    });
    if (!response.ok) {
        throw new Error('Failed to update comment');
    }
    return response.text();
}

export const deleteComment = async (commentId, token) => {
    const response = await fetch(`${API_BASE_URL}/api/Comment/delete/${commentId}`, {
        method: 'DELETE',
        headers: {
            'Accept': 'text/plain',
            'Authorization': `Bearer ${token}`
        }
    });
    if (!response.ok) {
        throw new Error('Failed to delete comment');
    }
    return response.text();
}


export const addReaction= async (articleId, reactionType, token) => {
    const response = await fetch(`${API_BASE_URL}/api/Reaction/add`, {
        method: 'POST',
        headers: {
            'Accept': 'text/plain',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ articleId, reactionType })
    });
    if (!response.ok) {
        throw new Error('Failed to add reaction');
    }
    return response.text();
}
export const updateReaction= async (articleId, reactionType, token) => {
    const response = await fetch(`${API_BASE_URL}/api/Reaction/update`, {
        method: 'PUT',
        headers: {
            'Accept': 'text/plain',
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify({ articleId, reactionType })
    });
    if (!response.ok) {
        throw new Error('Failed to update reaction');
    }
    return response.text();
}

export const removeReaction = async (articleId, token) => {
    const response = await fetch(`${API_BASE_URL}/api/Reaction/remove?articleId=${articleId}`, {
        method: 'DELETE',
        headers: {
            'Accept': 'text/plain',
            'Authorization': `Bearer ${token}`
        }
    });
    if (!response.ok) {
        throw new Error('Failed to remove reaction');
    }
    return response.text();
}

