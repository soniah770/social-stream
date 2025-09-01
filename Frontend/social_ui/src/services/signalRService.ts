import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

// Post data structure
export interface ProcessedPost {
  id: string;           
  content: string;      
  author: string;       
  platform: string;    
  timestamp: string;    // Creation time
  hashtags: string[]; 
  likeCount: number;   
  retweetCount: number; // Shares count
  sentimentScore: string; // Emotional tone
  processedAt: string; 
}

// Real-time connection service
class SignalRService {
  // SignalR connection
  private connection: HubConnection | null = null;
  
  // Reconnection tracking
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;

  // Establish connection
  async connect(): Promise<void> {
    this.connection = new HubConnectionBuilder()
      .withUrl('http://localhost:5000/postHub')
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    try {
      await this.connection.start();
      await this.connection.invoke('JoinPostStream');
      console.log('Connected to SignalR hub');
    } catch (error) {
      console.error('SignalR connection error:', error);
    }
  }

  // Listen for new posts
  onNewPosts(callback: (posts: ProcessedPost[]) => void): void {
    this.connection?.on('NewPosts', callback);
  }

  // Disconnect from hub
  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.invoke('LeavePostStream');
      await this.connection.stop();
    }
  }
}


export default new SignalRService();