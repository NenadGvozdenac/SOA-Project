import axios from 'axios';
import { FOLLOWINGS_URL } from './const_service.js';

export class FollowingsService {
    static getAuthHeader() {
        const token = localStorage.getItem('token');
        return token ? { Authorization: `Bearer ${token}` } : {};
    }

    static async getMyFollowings() {
        try {
            const response = await axios.get(`${FOLLOWINGS_URL}/followers/my/followings`, {
                headers: this.getAuthHeader()
            });
            return response.data;
        } catch (error) {
            throw error.response?.data || error;
        }
    }

    static async getMyFollowers() {
        try {
            const response = await axios.get(`${FOLLOWINGS_URL}/followers/my`, {
                headers: this.getAuthHeader()
            });
            return response.data;
        } catch (error) {
            throw error.response?.data || error;
        }
    }

    static async followUser(userId) {
        try {
            const response = await axios.post(`${FOLLOWINGS_URL}/followers/follow/${userId}`, {}, {
                headers: this.getAuthHeader()
            });
            return response.data;
        } catch (error) {
            throw error.response?.data || error;
        }
    }

    static async unfollowUser(userId) {
        try {
            const response = await axios.post(`${FOLLOWINGS_URL}/followers/unfollow/${userId}`, {}, {
                headers: this.getAuthHeader()
            });
            return response.data;
        } catch (error) {
            throw error.response?.data || error;
        }
    }

    static async getFollowSuggestions() {
        try {
            const response = await axios.get(`${FOLLOWINGS_URL}/followers/suggestions`, {
                headers: this.getAuthHeader()
            });
            return response.data;
        } catch (error) {
            throw error.response?.data || error;
        }
    }

    static async getBlogsFromFollowedUsers() {
        try {
            const response = await axios.get(`${FOLLOWINGS_URL}/followers/blogs`, {
                headers: this.getAuthHeader()
            });
            console.log(response.data)
            return response;
        } catch (error) {
            console.error('Error fetching blogs from followed users:', error);
            throw error.response?.data || error;
        }
    }

    static async getAllUsers() {
        try {
            const response = await axios.get(`${FOLLOWINGS_URL}/users/all`, {
                headers: this.getAuthHeader()
            });
            return response.data;
        } catch (error) {
            throw error.response?.data || error;
        }
    }
}
