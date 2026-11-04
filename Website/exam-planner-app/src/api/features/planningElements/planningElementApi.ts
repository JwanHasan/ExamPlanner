import api from "../../axiosInstance.ts";
import type {PlanningEducationElement, UpdatePlanningEducationElementRequest} from "./planningElementTypes.ts";

export const getPlanningEducationElements =
    async (): Promise<
        PlanningEducationElement[]
    > => {
        const response =
            await api.get<
                PlanningEducationElement[]
            >(
                "/planning-elements"
            );
        return response.data;
    };

export const getPlanningEducationElement =
    async (
        id: string
    ): Promise<
        PlanningEducationElement
    > => {
        const response =
            await api.get<
                PlanningEducationElement
            >(
                `/planning-elements/${id}`
            );
        return response.data;
    };

export const updatePlanningEducationElement =
    async (
        id: string,
        request:
        UpdatePlanningEducationElementRequest
    ): Promise<
        PlanningEducationElement
    > => {
        const response =
            await api.patch<
                PlanningEducationElement
            >(
                `/planning-elements/${id}`,
                request
            );
        return response.data;
    };