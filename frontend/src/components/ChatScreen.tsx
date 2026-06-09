import { useState, useRef, useEffect, type KeyboardEvent } from "react";
import { SendHorizonal, CheckCircle2, X } from "lucide-react";
import api from "../lib/api";
import { brl, shortDate } from "../lib/format";
import { TransactionType, type TransactionProposal } from "../lib/types";
import { useCreateTransaction } from "../lib/hooks";

interface AssistantReply {
  reply: string;
  proposal: TransactionProposal | null;
}

interface ChatMessage {
  role: "user" | "assistant";
  text: string;
  proposal?: TransactionProposal;
}

const WELCOME: ChatMessage = {
  role: "assistant",
  text: 'Olá! Diga um gasto ou recebimento e eu lanço pra você. Ex: "gastei 50 no mercado" ou "recebi 3000 salário".',
};

export default function ChatScreen() {
  const [messages, setMessages] = useState<ChatMessage[]>([WELCOME]);
  const [input, setInput] = useState("");
  const [busy, setBusy] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);
  const create = useCreateTransaction();

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, busy]);

  const send = async () => {
    const text = input.trim();
    if (!text || busy) return;
    setInput("");
    setMessages((prev) => [...prev, { role: "user", text }]);
    setBusy(true);
    try {
      const { data } = await api.post<AssistantReply>("/assistant/interpret", { message: text });
      setMessages((prev) => [
        ...prev,
        { role: "assistant", text: data.reply, proposal: data.proposal ?? undefined },
      ]);
    } catch (err: any) {
      const detail =
        err?.response?.data?.detail ?? "Não consegui chamar a IA. Confira se o backend está rodando.";
      setMessages((prev) => [...prev, { role: "assistant", text: detail }]);
    } finally {
      setBusy(false);
    }
  };

  const confirm = async (proposal: TransactionProposal, idx: number) => {
    try {
      await create.mutateAsync({
        accountId: proposal.accountId,
        categoryId: proposal.categoryId,
        type: proposal.type,
        amount: proposal.amount,
        occurredOn: proposal.occurredOn,
        description: proposal.description,
      });
      // Remove o card de proposta da mensagem que o gerou
      setMessages((prev) => prev.map((m, i) => (i === idx ? { role: "assistant", text: m.text } : m)));
      setMessages((prev) => [...prev, { role: "assistant", text: "Lançado! ✓ Pode ver na aba Transações." }]);
    } catch {
      setMessages((prev) => [...prev, { role: "assistant", text: "Erro ao confirmar o lançamento. Tente de novo." }]);
    }
  };

  const dismiss = (idx: number) =>
    setMessages((prev) => prev.map((m, i) => (i === idx ? { role: "assistant", text: m.text } : m)));

  const handleKey = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      send();
    }
  };

  return (
    <div className="flex h-[calc(100vh-7rem)] flex-col">
      <h1 className="mb-4 text-xl font-bold text-white">Assistente IA</h1>

      <div className="flex-1 space-y-3 overflow-y-auto pr-1">
        {messages.map((m, i) => (
          <div key={i} className={`flex ${m.role === "user" ? "justify-end" : "justify-start"}`}>
            <div className={`flex max-w-[85%] flex-col gap-2 ${m.role === "user" ? "items-end" : "items-start"}`}>
              <div
                className={`rounded-2xl px-4 py-2 text-sm leading-relaxed ${
                  m.role === "user"
                    ? "rounded-br-sm bg-emerald-500 text-slate-900"
                    : "rounded-bl-sm bg-slate-700 text-white"
                }`}
              >
                {m.text}
              </div>

              {m.proposal && (
                <div className="w-full rounded-2xl rounded-tl-sm border border-slate-600 bg-slate-800 p-3">
                  <div className="mb-3 space-y-1 text-xs text-slate-300">
                    <p>
                      <span className="text-slate-500">Valor </span>
                      <span
                        className={`font-semibold ${
                          m.proposal.type === TransactionType.Income ? "text-emerald-400" : "text-rose-400"
                        }`}
                      >
                        {brl(m.proposal.amount)}
                      </span>
                    </p>
                    <p>
                      <span className="text-slate-500">Categoria </span>
                      {m.proposal.categoryName}
                    </p>
                    <p>
                      <span className="text-slate-500">Conta </span>
                      {m.proposal.accountName}
                    </p>
                    <p>
                      <span className="text-slate-500">Data </span>
                      {shortDate(m.proposal.occurredOn)}
                    </p>
                    {m.proposal.description !== m.proposal.categoryName && (
                      <p>
                        <span className="text-slate-500">Descrição </span>
                        {m.proposal.description}
                      </p>
                    )}
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => confirm(m.proposal!, i)}
                      disabled={create.isPending}
                      className="flex flex-1 items-center justify-center gap-1 rounded-xl bg-emerald-500 py-2 text-xs font-semibold text-slate-900 disabled:opacity-60"
                    >
                      <CheckCircle2 size={14} /> Confirmar
                    </button>
                    <button
                      onClick={() => dismiss(i)}
                      disabled={create.isPending}
                      className="flex items-center justify-center gap-1 rounded-xl bg-slate-700 px-3 py-2 text-xs text-slate-400"
                    >
                      <X size={14} /> Ignorar
                    </button>
                  </div>
                </div>
              )}
            </div>
          </div>
        ))}

        {busy && (
          <div className="flex justify-start">
            <div className="rounded-2xl rounded-bl-sm bg-slate-700 px-4 py-3 text-sm text-slate-400">
              <span className="animate-pulse">Pensando…</span>
            </div>
          </div>
        )}

        <div ref={bottomRef} />
      </div>

      <div className="mt-3 flex gap-2">
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={handleKey}
          disabled={busy}
          placeholder='Ex.: "gastei 50 no mercado"'
          className="flex-1 rounded-2xl bg-slate-700 px-4 py-3 text-sm text-white placeholder-slate-500 outline-none focus:ring-2 focus:ring-emerald-500 disabled:opacity-60"
        />
        <button
          onClick={send}
          disabled={busy || !input.trim()}
          className="flex items-center justify-center rounded-2xl bg-emerald-500 px-4 py-3 text-slate-900 disabled:opacity-40"
          aria-label="Enviar"
        >
          <SendHorizonal size={20} />
        </button>
      </div>
    </div>
  );
}
