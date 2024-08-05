# News Aggregation and Management System

## Introduction

This project is a news aggregation and management system that collects news articles from third-party sources, stores them in a database, and provides a platform for admins and users to interact with the content. Admins can review and approve or reject articles, while users can view, like, and comment on approved articles.


## Project Overview

The News Aggregation and Management System automates the collection of news articles from third-party sources and provides an administrative interface for reviewing and managing these articles. The system ensures that only quality and relevant news articles are displayed to the end-users, who can then engage with the content by viewing, liking, and commenting on the articles.

## System Architecture

### Components
1. **Backend Server**: Handles the API endpoints, background processes, and database interactions.
2. **Frontend Application**: A React-based user interface for both admins and regular users.
3. **Database**: Stores user data, articles, comments, and other relevant information.

### Data Flow
1. The background process fetches articles from third-party URLs.
2. The fetched articles are stored in the database with a status indicating they are pending review.
3. Admins review the pending articles and approve or reject them.
4. Approved articles are displayed to users, who can then like and comment on them.

## Features

### Background Process for News Collection
- Automatically fetches news articles from predefined third-party URLs.
- Stores articles in the database with a 'pending' status for admin review.

### Admin Management
- Admins can view a list of pending articles.
- Admins can approve or reject articles.
- Approved articles are made visible to users.

### User Interaction
- Users can view a list of approved articles.
- Users can like and comment on articles.
- Comments are displayed in FILO (First In, Last Out) order for comments and FIFO (First In, First Out) order for replies.

## Workflows

### News Collection and Storage
1. The background process fetches articles using predefined URLs.
2. The articles are parsed and stored in the database with metadata such as title, content, and source URL.
3. Articles are marked as 'pending' for admin review.

### Admin Article Review
1. Admins log in to the admin panel.
2. They can view a list of pending articles and their details.
3. Admins can approve or reject each article.
   - Approved articles are displayed to users.
   - Rejected articles are archived or deleted.

### User Engagement
1. Users can browse a list of approved articles.
2. They can view the details of each article, including likes and comments.
3. Users can like or dislike articles and leave comments.
4. Comments can be replied to, with each comment and reply thread displayed in the specified order.

## Database Schema

### Tables/Collections
- **Users**: Stores user information.
- **Articles**: Stores details about each article, including its status (pending, approved, rejected).
- **Comments**: Stores user comments and their associations with articles.

## Technologies Used
- **Frontend**: React, (Context API), CSS/SCSS
- **Backend**: .Net core, C#, Entity Framework Core
- **Database**: MS SQL Server
- **Authentication**: JWT, Google OAuth
- **Background Processing**: IHostedService, HttpClient