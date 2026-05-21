// Garante que o Docker está rodando antes de subir os containers.
// Em Windows, tenta abrir o Docker Desktop e espera ficar pronto.
const { execSync, spawn } = require("node:child_process");

function dockerIsUp() {
  try {
    execSync("docker info", { stdio: "ignore" });
    return true;
  } catch {
    return false;
  }
}

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

async function main() {
  if (dockerIsUp()) {
    console.log("✓ Docker já está rodando.");
    return;
  }

  console.log("… Docker não está rodando. Tentando iniciar o Docker Desktop…");

  if (process.platform === "win32") {
    try {
      spawn("cmd", ["/c", "start", "", "Docker Desktop"], { detached: true, stdio: "ignore" }).unref();
    } catch {
      // Caminho alternativo, caso não esteja no PATH/menu
      try {
        spawn(`${process.env.ProgramFiles}\\Docker\\Docker\\Docker Desktop.exe`, [], {
          detached: true,
          stdio: "ignore",
        }).unref();
      } catch {
        /* ignora — cairá no timeout abaixo */
      }
    }
  } else if (process.platform === "darwin") {
    spawn("open", ["-a", "Docker"], { detached: true, stdio: "ignore" }).unref();
  }

  const timeoutMs = 120_000;
  const start = Date.now();
  while (Date.now() - start < timeoutMs) {
    if (dockerIsUp()) {
      console.log("✓ Docker está pronto.");
      return;
    }
    await sleep(3000);
    process.stdout.write(".");
  }

  console.error("\n✗ Docker não ficou pronto a tempo. Abra o Docker Desktop manualmente e rode de novo.");
  process.exit(1);
}

main();
