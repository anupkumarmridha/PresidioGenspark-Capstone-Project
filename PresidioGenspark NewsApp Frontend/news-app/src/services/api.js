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
