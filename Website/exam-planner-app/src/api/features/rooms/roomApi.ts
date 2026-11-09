import api from "../../axiosInstance.ts";

import type {AvailableRoom, AvailableRoomQuery} from "./roomTypes.ts";

export const getAvailableRooms =
    async (query: AvailableRoomQuery = {}): Promise<AvailableRoom[]> => {

        const params = new URLSearchParams();
        
        if (query.date) {
            params.set(
                "date",
                query.date
            );
        }
        
        if (query.campus) {
            params.set(
                "campus",
                query.campus
            );
        }
        
        if (query.search) {
            params.set(
                "search",
                query.search
            );
        }
        
        const queryString = params.toString();
        
        const url = queryString
            ? `/rooms/available?${queryString}`
            : "/rooms/available";


        const response = await api.get<AvailableRoom[]
            >(
                url
            );
        
        return response.data;
    };