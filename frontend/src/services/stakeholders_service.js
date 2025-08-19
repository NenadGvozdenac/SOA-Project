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
}