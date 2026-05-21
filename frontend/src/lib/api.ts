import axios from "axios";

// baseURL "/api" → o proxy do Vite (vite.config.ts) repassa para a API .NET.
const api = axios.create({ baseURL: "/api" });

export default api;
