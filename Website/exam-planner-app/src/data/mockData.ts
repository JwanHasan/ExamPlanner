export interface ExamSessionRow {
    id: number;
    course: string;
    classCode: string;
    date: string;
    time: string;
    room: string;

    status:
        | "Ready"
        | "Conflict"
        | "Locked";
}

export interface IssueRow {
    id: number;

    type:
        | "Student"
        | "Lecturer"
        | "Room";

    date: string;
    title: string;
    details: string;
    affected: number;
}

export interface RoomRow {
    code: string;
    campus: string;
    capacity: number;

    availability:
        | "Available"
        | "In use";
}

export interface ReviewRow {
    lecturer: string;
    completed: boolean;
    approved: boolean;
    remarks: string;
}

export const examSessions:
    ExamSessionRow[] = [

    {
        id: 1,

        course:
            "Database Systems",

        classCode:
            "SW3",

        date:
            "2027-01-11",

        time:
            "09:00",

        room:
            "C05.16",

        status:
            "Ready"
    },

    {
        id: 2,

        course:
            "Software Engineering",

        classCode:
            "SW3",

        date:
            "2027-01-13",

        time:
            "09:00",

        room:
            "C05.18",

        status:
            "Conflict"
    },

    {
        id: 3,

        course:
            "SEP",

        classCode:
            "SW3",

        date:
            "2027-01-18",

        time:
            "08:30",

        room:
            "C06.01",

        status:
            "Locked"
    },

    {
        id: 4,

        course:
            "Algorithms",

        classCode:
            "ICT4",

        date:
            "2027-01-20",

        time:
            "10:00",

        room:
            "C05.21",

        status:
            "Ready"
    }
];

export const issues:
    IssueRow[] = [

    {
        id: 1,

        type:
            "Student",

        date:
            "2027-01-13",

        title:
            "Student exam overlap",

        details:
            "Three students are assigned to two exams on the same day.",

        affected:
            3
    },

    {
        id: 2,

        type:
            "Lecturer",

        date:
            "2027-01-18",

        title:
            "Examiner overlap",

        details:
            "One lecturer is assigned to two sessions at overlapping times.",

        affected:
            1
    },

    {
        id: 3,

        type:
            "Room",

        date:
            "2027-01-20",

        title:
            "Room capacity",

        details:
            "The selected room is smaller than the expected number of students.",

        affected:
            38
    }
];

export const rooms:
    RoomRow[] = [

    {
        code:
            "C05.16",

        campus:
            "Horsens",

        capacity:
            32,

        availability:
            "Available"
    },

    {
        code:
            "C05.18",

        campus:
            "Horsens",

        capacity:
            48,

        availability:
            "In use"
    },

    {
        code:
            "C06.01",

        campus:
            "Horsens",

        capacity:
            120,

        availability:
            "Available"
    },

    {
        code:
            "C06.03",

        campus:
            "Horsens",

        capacity:
            80,

        availability:
            "Available"
    }
];

export const reviews:
    ReviewRow[] = [

    {
        lecturer:
            "MWA",

        completed:
            true,

        approved:
            true,

        remarks:
            ""
    },

    {
        lecturer:
            "KHS",

        completed:
            true,

        approved:
            false,

        remarks:
            "Please move the exam on 13 January."
    },

    {
        lecturer:
            "ZTHO",

        completed:
            false,

        approved:
            false,

        remarks:
            ""
    },

    {
        lecturer:
            "KRJ",

        completed:
            true,

        approved:
            true,

        remarks:
            "No conflicts found."
    }
];