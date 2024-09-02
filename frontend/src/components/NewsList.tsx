import React, { useEffect, useState } from 'react';
import { fetchNews } from '../services/newsService';

interface NewsItem {
    category: string;
    title: string;
    author: string;
    publishedDate: string;
}

const NewsList: React.FC = () => {
    const [news, setNews] = useState<NewsItem[]>([]);

    useEffect(() => {
        const getNews = async () => {
            const newsData = await fetchNews();
            setNews(newsData);
        };

        getNews();
    }, []);

    return (
        <div className="news-list-container">
            <h1 className="news-list-title">Tech News Aggregator</h1>
            <div className="news-items">
                {news.map((item, index) => (
                    <div key={index} className="news-item">
                        <div className="news-category">{item.category}</div>
                        <h2 className="news-title">{item.title}</h2>
                        <p className="news-author-date">
                            <span className="news-author">{item.author}</span>
                            <span className="news-date">{new Date(item.publishedDate).toLocaleDateString()}</span>
                        </p>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default NewsList;
