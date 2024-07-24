import axios from 'axios';

const API_URL = 'https://inshortsapi.vercel.app/news';

export const articleService = {
    /**
     * Fetches articles based on category and search query.
     * @param {string} category - The category of the news (e.g., 'sports').
     * @param {string} searchQuery - The search query for filtering articles (optional).
     * @param {number} page - The page number for pagination (optional).
     * @returns {Promise<Object>} - The response containing articles and pagination info.
     */
    fetchArticles: async (category = 'sports', searchQuery = '', page = 1) => {
        try {
            const response = await axios.get(API_URL, {
                params: {
                    category,
                    q: searchQuery, // The search query parameter
                    page, // Pagination parameter
                },
            });
            return response.data;
        } catch (error) {
            console.error('Failed to fetch articles', error);
            throw error; // Rethrow the error to be handled by the caller
        }
    },
};
