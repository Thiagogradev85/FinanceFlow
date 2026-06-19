// Garante que o Docker Engine está rodando antes de subir os containers.
// Ambiente canônico do projeto = Docker Engine no Ubuntu WSL2 (NÃO Docker Desktop).
const { execSync } = require("node:child_process");

function dockerIsUp() {
  try {
    execSync("docker info", { stdio: "ignore" });
    return true;
  } catch {
    return false;
  }
}

// Tenta subir o Engine via systemd. `sudo -n` = não-interativo: se exigir senha,
// falha na hora em vez de travar o script esperando input.
function tryStartEngine() {
  try {
    execSync("sudo -n systemctl start docker", { stdio: "ignore" });
    return true;
  } catch {
    return false;
  }
}

if (dockerIsUp()) {
  console.log("✓ Docker já está rodando.");
  process.exit(0);
}

console.log("… Docker não está rodando. Tentando iniciar o Docker Engine (systemctl)…");
if (tryStartEngine() && dockerIsUp()) {
  console.log("✓ Docker Engine iniciado.");
  process.exit(0);
}

console.error(
  [
    "",
    "✗ Docker Engine não está rodando.",
    "  Ambiente do projeto = Docker Engine no Ubuntu WSL2 (sem Docker Desktop).",
    "  Suba com:  sudo systemctl start docker",
    "  Confira:   systemctl is-active docker   (deve dizer 'active')",
    "  Detalhes no README.md → seção 'Como rodar' (WSL2).",
    "",
  ].join("\n"),
);
process.exit(1);
