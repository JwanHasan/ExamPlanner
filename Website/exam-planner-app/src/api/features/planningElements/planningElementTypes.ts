export interface PlanningEducationElement {
    id: string;
    sourceChecksum: string;
    sourceModifiedAt: string;
    name: string;
    nickname: string;
    ects: number;
    executionElementCount: number;
    startDate: string;
    endDate: string;
    activityOffering: string;
    prefix: string;
    assessmentType:
        | "Internal"
        | "External";
    completionAssessment: boolean;
    gradingScale: string;
    combinedWrittenOral: boolean;
    oral: boolean;
    practicalExam: boolean;
    project: boolean;
    written: boolean;
}

export interface UpdatePlanningEducationElementRequest {
    name: string;
    nickname: string;
    ects: number;
    executionElementCount: number;
    startDate: string;
    endDate: string;
    activityOffering: string;
    prefix: string;
    assessmentType:
        | "Internal"
        | "External";
    completionAssessment: boolean;
    gradingScale: string;
    combinedWrittenOral: boolean;
    oral: boolean;
    practicalExam: boolean;
    project: boolean;
    written: boolean;
}