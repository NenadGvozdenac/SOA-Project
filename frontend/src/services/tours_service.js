import axios from 'axios';
import { TOURS_URL } from './const_service.js';

export class ToursService {
    static async addCheckpoint(data, jwt) {
        // data: { tourId, name, description, latitude, longitude, imageBase64 }
        try {
            const response = await axios.post(`http://localhost:8082/api/tours/checkpoint`, data, {
                headers: {
                    Authorization: `Bearer ${jwt}`,
                    'Content-Type': 'application/json'
                }
            });
            return response.data;
        } catch (error) {
            throw error.response?.data || error;
        }
    }
}