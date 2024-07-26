import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL;

export const authService = {
    googleLogin: (googleToken) => {
        return axios.post(`${API_URL}/api/Auth/google-login`, { googleToken });
    },
};
