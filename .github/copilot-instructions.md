# Instruções Gerais — Copilot

- Responda sempre em **português do Brasil**, salvo pedido explícito.
- Antes de propor mudanças grandes, explique o plano em 3–5 passos e pergunte se pode prosseguir.
- Prefira soluções simples, seguras e fáceis de manter. Não adicione abstrações sem necessidade concreta.
- Preserve o estilo do projeto ao editar código existente.
- Use **Conventional Commits** ao sugerir mensagens de commit: `feat:`, `fix:`, `refactor:`, `docs:`, `chore:`, `test:`.
- **Nunca use `DateTime.Now`** em exemplos C# — sempre `DateTime.UtcNow` ou `IClock` injetado.
- Não adicione comentários que explicam o que o código faz — só os que explicam *por quê*.
- Ao gerar testes, use o padrão `Metodo_Estado_ResultadoEsperado` com blocos AAA separados por linha em branco.
