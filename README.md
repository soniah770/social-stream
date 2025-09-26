# Social Stream

Real-time social media streaming platform built with microservices architecture.

## Architecture

### Services
- **Data Collector**: Background worker service that continuously fetches posts from social media APIs
- **Data Processor**: Console application that processes and enriches raw social media data  
- **API Gateway**: Web API service that provides REST endpoints and SignalR WebSocket communication

### Inter-Service Communication
- **Redis Pub/Sub**: Event-driven messaging between services for real-time data distribution
- **REST APIs**: HTTP endpoints for frontend and external integrations
- **WebSocket**: SignalR for real-time client connections

## Tech Stack

### Backend
- **Framework**: .NET 9, ASP.NET Core
- **Real-time**: SignalR WebSocket
- **Message Queue**: Redis Pub/Sub  
- **Background Processing**: Hosted Services
- **Data Processing**: Console Applications

### Frontend  
- **Framework**: React 18 with TypeScript
- **Real-time Client**: SignalR JavaScript Client
- **HTTP Client**: Axios

### Infrastructure
- **Containerization**: Docker and Docker Compose
- **Service Discovery**: Docker internal networking
- **Data Storage**: Redis for caching and messaging


*/
