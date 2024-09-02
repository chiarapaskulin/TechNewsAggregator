import axios from 'axios';

export const fetchNews = async () => {
    try {
        const response = await axios.get('http://localhost:5186/api/news');
        return response.data;
    } catch (error) {
        console.error('Error fetching news:', error);
        return [];
    }
};
