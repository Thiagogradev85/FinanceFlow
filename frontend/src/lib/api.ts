import axios from "axios";

// Em dev: VITE_API_URL não definida → usa proxy do Vite (/api → localhost:5080).
// Em produção: VITE_API_URL="https://meuapp.railway.app" → chama direto.
const baseURL = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}/api`
  : "/api";

const api = axios.create({
  baseURL,
  headers: import.meta.env.VITE_API_KEY
    ? { "X-Api-Key": import.meta.env.VITE_API_KEY }
    : {},
});

export default api;
