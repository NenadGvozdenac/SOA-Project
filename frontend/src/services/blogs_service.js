import axios from 'axios';
import { BLOGS_URL } from './const_service.js';

export class BlogsService {
    
    // Get all blogs with pagination
    static async getAllBlogs(pageNumber = 1, pageSize = 10) {
        try {
            const jwt = localStorage.getItem('token');
            const response = await axios.get(`${BLOGS_URL}/blogs`, {
                params: {
                    pageNumber,
                    pageSize
                },
                headers: {
                    Authorization: `Bearer ${jwt}`
                }
            });

            return response.data.value;
        } catch (error) {
            console.error("Error fetching blogs:", error);
            throw error;
        }
    }

    // Get blog by ID
    static async getBlogById(blogId) {
        try {
            const jwt = localStorage.getItem('token');
            const response = await axios.get(`${BLOGS_URL}/blogs/${blogId}`, {
                headers: {
                    Authorization: `Bearer ${jwt}`
                }
            });

            return response.data.value;
        } catch (error) {
            console.error("Error fetching blog by ID:", error);
            throw error;
        }
    }
    
    // Create new blog
    static async createBlog(blogData) {
        try {
            const token = localStorage.getItem('token');
            const response = await axios.post(`${BLOGS_URL}/blogs`, blogData, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            return response;
        } catch (error) {
            console.error('Error creating blog:', error);
            throw error;
        }
    }

    // Get blogs by authors (internal endpoint)
    static async getBlogsByAuthors(authorIds) {
        try {
            const response = await axios.post(`${BLOGS_URL}/internal/blogs/by-authors`, {
                authorIds: authorIds
            }, {
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            return response;
        } catch (error) {
            console.error('Error fetching blogs by authors:', error);
            throw error;
        }
    }

    // Like blog
    static async likeBlog(blogId) {
        try {
            const jwt = localStorage.getItem('token');
            const response = await axios.post(`${BLOGS_URL}/blogs/like/${blogId}`, {}, {
                headers: {
                    Authorization: `Bearer ${jwt}`
                }
            });
            console.log("Blog liked:", response.data);
            return response.data;
        } catch (error) {
            console.error("Error liking blog:", error);
            throw error;
        }
    }

    // Dislike blog
    static async dislikeBlog(blogId) {
        try {
            const jwt = localStorage.getItem('token');
            const response = await axios.delete(`${BLOGS_URL}/blogs/dislike/${blogId}`, {
                headers: {
                    Authorization: `Bearer ${jwt}`
                }
            });

            return response.data;
        } catch (error) {
            console.error("Error disliking blog:", error);
            throw error;
        }
    }

    // Convert image to base64
    static fileToBase64(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => {
                // Remove prefix "data:image/type;base64,"
                const base64String = reader.result.split(',')[1];
                resolve(base64String);
            };
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    }

    // Validate blog data
    static validateBlogData(blogData) {
        const errors = [];
        
        if (!blogData.title || blogData.title.trim() === '') {
            errors.push('Title is required');
        }
        
        if (!blogData.descriptionMarkdown || blogData.descriptionMarkdown.trim() === '') {
            errors.push('Description is required');
        }
        
        if (blogData.title && blogData.title.length > 200) {
            errors.push('Title cannot be longer than 200 characters');
        }
        
        if (blogData.descriptionMarkdown && blogData.descriptionMarkdown.length > 10000) {
            errors.push('Description cannot be longer than 10000 characters');
        }
        
        return errors;
    }

    // Get comments for blog
    static async getComments(blogId) {
        try {
            const jwt = localStorage.getItem('token');
            const response = await axios.get(`${BLOGS_URL}/blogs/${blogId}/comments`, {
                headers: {
                    Authorization: `Bearer ${jwt}`
                }
            });
            
            console.log('Fetched comments:', response.data.value);
            return response.data.value || [];
        } catch (error) {
            console.error('Error fetching comments:', error);
            throw error;
        }
    }

    // Create comment
    static async createComment(blogId, content) {
        try {
            const token = localStorage.getItem('token');
            const response = await axios.post(`${BLOGS_URL}/blogs/comment`, {
                blogId,
                content
            }, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            return response.data;
        } catch (error) {
            console.error('Error creating comment:', error);
            throw error;
        }
    }

    // Ažuriranje komentara
    static async updateComment(commentId, content) {
        try {
            const token = localStorage.getItem('token');
            const response = await axios.put(`${BLOGS_URL}/blogs/comment/update`, {
                CommentId: commentId,  // PascalCase za .NET backend
                Content: content       // PascalCase za .NET backend
            }, {
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
            return response.data;
        } catch (error) {
            console.error('Error updating comment:', error);
            throw error;
        }
    }
}
