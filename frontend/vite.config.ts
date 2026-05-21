import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import { VitePWA } from "vite-plugin-pwa";

export default defineConfig({
  plugins: [
    react(),
    VitePWA({
      registerType: "autoUpdate",
      includeAssets: ["icon.svg"],
      manifest: {
        name: "FinanceFlow",
        short_name: "FinanceFlow",
        description: "Controle de gastos e previsão financeira",
        theme_color: "#0f172a",
        background_color: "#0f172a",
        display: "standalone",
        start_url: "/",
        icons: [
          { src: "icon.svg", sizes: "any", type: "image/svg+xml", purpose: "any maskable" },
        ],
      },
    }),
  ],
  server: {
    port: 5173,
    open: true,
    // O front chama "/api/..." e o Vite repassa para a API .NET — sem dor de cabeça com CORS.
    proxy: {
      "/api": "http://localhost:5080",
    },
  },
});
