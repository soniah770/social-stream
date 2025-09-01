import axios from 'axios';
import { ProcessedPost } from './signalRService';

const API_BASE_URL = 'http://localhost:5000/api';

class ApiService {
  private axiosInstance = axios.create({
    baseURL: API_BASE_URL,
    timeout: 10000,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  async getRecentPosts(limit: number = 20): Promise<ProcessedPost[]> {
    try {
      const response = await this.axiosInstance.get<ProcessedPost[]>(`/posts?limit=${limit}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching posts:', error);
      return [];
    }
  }

  async getPost(id: string): Promise<ProcessedPost | null> {
    try {
      const response = await this.axiosInstance.get<ProcessedPost>(`/posts/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching post:', error);
      return null;
    }
  }
}

export default new ApiService();