// Carrega os horários livres de uma data e serviço (Agendamento/Index e Agendamento/Reagendar).
// A tela marca os elementos com data-atributos; este arquivo não conhece nenhuma tela específica.
//
//   #data            campo de data (input type="date")
//   #servico         select OU input hidden com o id do serviço
//   #slots           área onde os horários aparecem como botões de rádio
//   data-ignorar     (opcional, em #slots) id do agendamento que está sendo reagendado
//   data-nome        nome do campo enviado no formulário (ex.: Dados.Horario)
//   data-selecionado (opcional) horário que já vem marcado
//   data-url-espera  (opcional) endereço da lista de espera, mostrado quando o dia está lotado
(function () {
    const campoData = document.getElementById("data");
    const campoServico = document.getElementById("servico");
    const area = document.getElementById("slots");
    if (!campoData || !campoServico || !area) return;

    const nomeCampo = area.dataset.nome || "Horario";
    const ignorar = area.dataset.ignorar || "";
    const urlEspera = area.dataset.urlEspera || "";
    let marcado = area.dataset.selecionado || "";

    function mensagem(texto) {
        area.innerHTML = "";
        area.removeAttribute("role");
        const p = document.createElement("p");
        p.className = "slots-aviso mb-0";
        p.textContent = texto;
        area.appendChild(p);
        return p;
    }

    async function carregar() {
        const data = campoData.value;
        const servico = campoServico.value;
        if (!data || !servico) { mensagem("Escolha o serviço e a data para ver os horários."); atualizarResumo(); return; }

        mensagem("Buscando horários...");
        try {
            let url = `/Agendamento/Horarios?data=${encodeURIComponent(data)}&idServico=${encodeURIComponent(servico)}`;
            if (ignorar) url += `&ignorar=${encodeURIComponent(ignorar)}`;
            const resposta = await fetch(url, { headers: { "Accept": "application/json" } });
            if (!resposta.ok) throw new Error("falha");
            const horarios = await resposta.json();

            if (horarios.length === 0) {
                const p = mensagem("Não há horários livres neste dia. ");
                if (urlEspera) {
                    const a = document.createElement("a");
                    a.href = `${urlEspera}?servico=${encodeURIComponent(servico)}&data=${encodeURIComponent(data)}`;
                    a.textContent = "Entrar na lista de espera";
                    p.appendChild(a);
                }
                atualizarResumo();
                return;
            }

            area.innerHTML = "";
            area.setAttribute("role", "radiogroup");
            horarios.forEach(function (h) {
                const rotulo = document.createElement("label");
                rotulo.className = "slot";
                const entrada = document.createElement("input");
                entrada.type = "radio";
                entrada.name = nomeCampo;
                entrada.value = h.horario;
                entrada.required = true;
                entrada.checked = (h.horario === marcado);
                entrada.addEventListener("change", function () { marcado = h.horario; atualizarResumo(); });
                const texto = document.createElement("span");
                texto.textContent = h.horario;
                rotulo.appendChild(entrada);
                rotulo.appendChild(texto);
                area.appendChild(rotulo);
            });
        } catch (e) {
            mensagem("Não foi possível carregar os horários. Tente novamente.");
        }
        atualizarResumo();
    }

    function atualizarResumo() {
        const alvo = document.getElementById("resumo");
        if (!alvo) return;
        const opcao = campoServico.options ? campoServico.options[campoServico.selectedIndex] : null;
        const nome = opcao && opcao.value ? opcao.textContent.trim() : "";
        const partes = campoData.value ? campoData.value.split("-") : [];
        const dataBr = partes.length === 3 ? `${partes[2]}/${partes[1]}/${partes[0]}` : "—";
        const pedacos = [];
        if (nome) pedacos.push(nome);
        pedacos.push(dataBr, marcado || "—");
        alvo.textContent = pedacos.join(" • ");
    }

    campoData.addEventListener("change", function () { marcado = ""; carregar(); });
    campoServico.addEventListener("change", function () { marcado = ""; carregar(); });
    carregar();
})();
