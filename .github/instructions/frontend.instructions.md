---
applyTo: "**/*.tsx,**/*.ts,frontend/**"
---

# Regras para Frontend React/TypeScript

- **Mobile First** com Tailwind: comece sem breakpoint, expanda com `sm:`, `md:`, `lg:`.
- Use **TanStack Query** para fetch, cache e invalidação de dados do servidor.
- Formulários: validação client-side antes de POST; não dependa só do backend.
- DTOs/types como `interface` ou `type` imutáveis — sem mutação direta.
- Evite `any` — tipar corretamente ou usar `unknown` com narrowing.
- Componentes pequenos e focados; extraia hooks customizados para lógica de estado reutilizável.
