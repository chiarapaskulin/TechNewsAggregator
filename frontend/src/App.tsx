import React from 'react';
import './App.css';
import NewsList from './components/NewsList';

const App: React.FC = () => {
    return (
        <div className="App">
            <header className="App-header">
                <NewsList />
            </header>
        </div>
    );
};

export default App;
