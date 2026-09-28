import axios from "axios";
import Cookies from "js-cookie";

export const TOKEN_COOKIE =
    "examplan_token";

const api = axios.create({
    baseURL:
        import.meta.env.VITE_API_URL ??
        "/api",

    headers: {
        "Content-Type":
            "application/json"
    }
});

api.interceptors.request.use(
    config => {

        const token =
            Cookies.get(
                TOKEN_COOKIE
            );

        if (token) {
            config.headers.Authorization =
                `Bearer ${token}`;
        }

        return config;
    }
);

api.interceptors.response.use(

    response => response,

    error => {

        if (
            error.response?.status ===
            401 &&
            Cookies.get(TOKEN_COOKIE)
        ) {
            Cookies.remove(
                TOKEN_COOKIE
            );

            window.location.hash =
                "#/";
        }

        return Promise.reject(error);
    }
);

export default api;