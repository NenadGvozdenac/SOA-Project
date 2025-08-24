import axios from 'axios'; 

import { STAKEHOLDERS_URL } from './const_service.js';

export class StakeholdersService {
    static async getUsers(jwt) {
        try {
            const response = await axios.get(`${STAKEHOLDERS_URL}/users`, {
                headers: {
                    Authorization: `Bearer ${jwt}`
                }
            });

            return response.data;
        } catch (error) {
            console.error("Error fetching users:", error);
            throw error;
        }
    }

    static async updateUser(userId, userData) {
        try {
            const jwt = localStorage.getItem('token');
            const response = await axios.put(`${STAKEHOLDERS_URL}/users/${userId}`, userData, {
                headers: {
                    Authorization: `Bearer ${jwt}`,
                    'Content-Type': 'application/json'
                }
            });

            return response.data;
        } catch (error) {
            console.error("Error updating user:", error);
            throw error;
        }
    }

    static async getUserById(userId) {
        try {
            const jwt = localStorage.getItem('token');
            const response = await axios.get(`${STAKEHOLDERS_URL}/users/${userId}`, {
                headers: {
                    Authorization: `Bearer ${jwt}`
                }
            });

            return response.data;
        } catch (error) {
            console.error("Error fetching user by ID:", error);
            throw error;
        }
    }
}