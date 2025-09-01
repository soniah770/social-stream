import React, { useState, useEffect } from 'react';
import signalRService, { ProcessedPost } from '../services/signalRService';
import apiService from '../services/apiService';
import PostItem from './PostItem';  // Fixed import

interface PostListProps {}  // Added missing interface

const PostList: React.FC<PostListProps> = () => {
  const [posts, setPosts] = useState<ProcessedPost[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [connected, setConnected] = useState<boolean>(false);

  useEffect(() => {
    const initializeConnection = async () => {
      try {
        const initialPosts = await apiService.getRecentPosts(10);
        setPosts(initialPosts);

        await signalRService.connect();
        setConnected(true);

        signalRService.onNewPosts((newPosts: ProcessedPost[]) => {
          setPosts(prevPosts => {
            const combined = [...newPosts, ...prevPosts];
            return combined.slice(0, 50);
          });
        });
      } catch (error) {
        console.error('Failed to initialize:', error);
      } finally {
        setLoading(false);
      }
    };

    initializeConnection();

    return () => {
      signalRService.disconnect();
    };
  }, []);

  if (loading) {
    return <div className="loading">Loading posts...</div>;
  }

  return (
    <div className="post-list">
      <div className="connection-status">
        Status: {connected ? ' Connected' : ' Disconnected'}
      </div>
      
      {posts.length === 0 ? (
        <div className="no-posts">No posts available</div>
      ) : (
        posts.map(post => (
          <PostItem key={post.id} post={post} />
        ))
      )}
    </div>
  );
};

export default PostList;