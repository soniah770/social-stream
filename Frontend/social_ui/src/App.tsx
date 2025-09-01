import React from 'react';
import PostList from './components/PostList';
import './App.css';

function App() {
  return (
    <div className="App">
      <a href="#main-content" className="skip-link">
        Skip to main content
      </a>
      
      <header role="banner">
        <h1>Social Stream</h1>
        <p>Real-time social media posts</p>
      </header>
      
      <main id="main-content" role="main">
        <PostList />
      </main>
      
      <footer role="contentinfo">
        <p>&copy; 2025 Social Stream - Real-time social media aggregation</p>
      </footer>
    </div>
  );
}

export default App;
