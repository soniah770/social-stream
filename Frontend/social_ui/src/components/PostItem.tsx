import React, { useMemo, useCallback } from 'react';
import DOMPurify from 'dompurify';

interface PostItemProps {
  post: {
    id: string;
    content: string;
    author: string;
    platform: string;
    timestamp: string;
    likeCount: number;
    sentimentScore: string;
  };
}

const PostItem: React.FC<PostItemProps> = ({ post }) => {
  const sanitizedContent = useMemo(() => 
    DOMPurify.sanitize(post.content), [post.content]); // Sanitizes HTML to prevent XSS attacks

  const formattedDate = useMemo(() => 
    new Intl.DateTimeFormat('en-US', {  //localization
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(post.timestamp)), [post.timestamp]);

  const handleKeyDown = useCallback((event: React.KeyboardEvent) => {
    if (event.key === 'Enter' || event.key === ' ') {
      // Handle post interaction
       console.log(`Clicked post ${post.id}`);
    }
  }, []);

  return (
    <article 
      className="post-item"
      role="article"
      tabIndex={0}
      onKeyDown={handleKeyDown}
      aria-labelledby={`post-${post.id}-author`}
      aria-describedby={`post-${post.id}-content`}
    >
      <header className="post-header">
        <h3 id={`post-${post.id}-author`} className="author">
          {post.author}
        </h3>
        <time dateTime={post.timestamp} className="timestamp">
          {formattedDate}
        </time>
      </header>
      
      <div 
        id={`post-${post.id}-content`}
        className="post-content"
        dangerouslySetInnerHTML={{ __html: sanitizedContent }}
      />
      
      <div className="post-metrics" role="group" aria-label="Post metrics">
        <span className="likes" aria-label={`${post.likeCount} likes`}>
          <span aria-hidden="true">liked</span> {post.likeCount}
        </span>
        <span className="sentiment" aria-label={`Sentiment: ${post.sentimentScore}`}>
          {post.sentimentScore}
        </span>
      </div>
    </article>
  );
};

export default PostItem;