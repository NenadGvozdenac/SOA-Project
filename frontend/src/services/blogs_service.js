import axios from 'axios';
import { BLOGS_URL } from './const_service.js';

export class BlogsService {
    
    // Kreiranje novog bloga
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

    // Dobijanje blogova po autorima (internal endpoint)
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

    // Konvertovanje slike u base64
    static fileToBase64(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => {
                // Uklanjanje prefiks "data:image/type;base64,"
                const base64String = reader.result.split(',')[1];
                resolve(base64String);
            };
            reader.onerror = reject;
            reader.readAsDataURL(file);
        });
    }

    // Validacija blog podataka
    static validateBlogData(blogData) {
        const errors = [];
        
        if (!blogData.title || blogData.title.trim() === '') {
            errors.push('Naslov je obavezan');
        }
        
        if (!blogData.descriptionMarkdown || blogData.descriptionMarkdown.trim() === '') {
            errors.push('Opis je obavezan');
        }
        
        if (blogData.title && blogData.title.length > 200) {
            errors.push('Naslov ne može biti duži od 200 karaktera');
        }
        
        if (blogData.descriptionMarkdown && blogData.descriptionMarkdown.length > 10000) {
            errors.push('Opis ne može biti duži od 10000 karaktera');
        }
        
        return errors;
    }
}
