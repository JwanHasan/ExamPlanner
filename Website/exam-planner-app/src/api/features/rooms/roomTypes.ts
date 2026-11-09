export interface AvailableRoom {
    id: number;
    name: string;
    campus: string;
    capacity: number;
}

export interface AvailableRoomQuery {
    date?: string;
    campus?: string;
    search?: string;
}