import 'package:flutter/material.dart';

import '../models/modelos.dart';
import '../services/api_service.dart';
import '../tema.dart';
import '../util/formatos.dart';
import '../widgets/componentes.dart';

/// Aba "Lista de espera" — a FUNCIONALIDADE COMPLEMENTAR do aplicativo.
///
/// A cliente pede um dia que está lotado. Quando alguém cancela ou reagenda,
/// a trigger TR_Agendamento_ListaEspera (no banco) muda o pedido para "Notificado"
/// e o app mostra "Vaga liberada!" com o botão para agendar.
class ListaEsperaPage extends StatefulWidget {
  final void Function(int idServico) aoAgendar;

  const ListaEsperaPage({super.key, required this.aoAgendar});

  @override
  State<ListaEsperaPage> createState() => _ListaEsperaPageState();
}

class _ListaEsperaPageState extends State<ListaEsperaPage> {
  List<Servico> _servicos = [];
  List<ItemListaEspera> _itens = [];
  int? _idServico;
  DateTime? _data;
  TimeOfDay? _hora;
  bool _carregando = true;
  bool _enviando = false;
  String? _erro;

  @override
  void initState() {
    super.initState();
    _carregar();
  }

  Future<void> _carregar() async {
    setState(() {
      _carregando = true;
      _erro = null;
    });
    try {
      final resultados = await Future.wait([api.servicos(), api.listaEspera()]);
      if (!mounted) return;
      setState(() {
        _servicos = resultados[0] as List<Servico>;
        _itens = resultados[1] as List<ItemListaEspera>;
      });
    } on ApiException catch (e) {
      if (mounted) setState(() => _erro = e.mensagem);
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _escolherData() async {
    final hoje = DateTime.now();
    final escolhida = await showDatePicker(
      context: context,
      initialDate: _data ?? hoje,
      firstDate: DateTime(hoje.year, hoje.month, hoje.day),
      lastDate: hoje.add(const Duration(days: 90)),
      helpText: 'Dia desejado',
    );
    if (escolhida != null) setState(() => _data = escolhida);
  }

  Future<void> _escolherHora() async {
    final escolhida = await showTimePicker(
      context: context,
      initialTime: _hora ?? const TimeOfDay(hour: 9, minute: 0),
      helpText: 'Horário desejado (opcional)',
    );
    if (escolhida != null) setState(() => _hora = escolhida);
  }

  String? get _horaTexto {
    final h = _hora;
    if (h == null) return null;
    return '${h.hour.toString().padLeft(2, '0')}:${h.minute.toString().padLeft(2, '0')}';
  }

  Future<void> _entrar() async {
    final idServico = _idServico;
    final data = _data;
    if (idServico == null || data == null) return;

    setState(() => _enviando = true);
    try {
      await api.entrarListaEspera(idServico, data, _horaTexto);
      if (!mounted) return;
      avisar(context, 'Pronto! Avisaremos aqui quando abrir uma vaga.');
      setState(() {
        _data = null;
        _hora = null;
      });
      await _carregar();
    } on ApiException catch (e) {
      if (mounted) avisar(context, e.mensagem, erro: true);
    } finally {
      if (mounted) setState(() => _enviando = false);
    }
  }

  Future<void> _sair(ItemListaEspera item) async {
    try {
      await api.sairListaEspera(item.id);
      if (!mounted) return;
      avisar(context, 'Você saiu da lista de espera.');
      await _carregar();
    } on ApiException catch (e) {
      if (mounted) avisar(context, e.mensagem, erro: true);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_carregando) return const Center(child: CircularProgressIndicator());
    final erro = _erro;
    if (erro != null) return MensagemErro(mensagem: erro, aoTentarDeNovo: _carregar);

    final data = _data;

    return RefreshIndicator(
      onRefresh: _carregar,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          LarguraMaxima(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text('O dia que você quer está lotado? Entre na fila: '
                    'quando abrir uma vaga, ela aparece aqui e na tela inicial.'),
                const TituloSecao('Novo pedido'),
                InputDecorator(
                  decoration: campo('Serviço'),
                  child: DropdownButtonHideUnderline(
                    child: DropdownButton<int>(
                      isExpanded: true,
                      isDense: true,
                      value: _idServico,
                      hint: const Text('Selecione'),
                      items: [
                        for (final s in _servicos) DropdownMenuItem(value: s.id, child: Text(s.nome)),
                      ],
                      onChanged: (v) => setState(() => _idServico = v),
                    ),
                  ),
                ),
                const SizedBox(height: 12),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    OutlinedButton.icon(
                      onPressed: _escolherData,
                      icon: const Icon(Icons.calendar_month),
                      label: Text(data == null ? 'Escolher o dia' : dataBr(isoData(data))),
                    ),
                    OutlinedButton.icon(
                      onPressed: _escolherHora,
                      icon: const Icon(Icons.schedule),
                      label: Text(_horaTexto ?? 'Qualquer horário'),
                    ),
                    if (_hora != null)
                      TextButton(onPressed: () => setState(() => _hora = null), child: const Text('Limpar horário')),
                  ],
                ),
                const SizedBox(height: 12),
                FilledButton(
                  onPressed: (_idServico != null && data != null && !_enviando) ? _entrar : null,
                  child: const Text('Entrar na lista de espera'),
                ),

                const TituloSecao('Meus pedidos'),
                if (_itens.isEmpty) const Text('Você não está em nenhuma lista de espera.'),
                for (final item in _itens) _cartao(item),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _cartao(ItemListaEspera item) {
    return Card(
      color: item.vagaLiberada ? corRosaClaro : Colors.white,
      margin: const EdgeInsets.only(bottom: 10),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(child: Text(item.servicoNome, style: const TextStyle(fontWeight: FontWeight.w700))),
                StatusChip(item.status),
              ],
            ),
            const SizedBox(height: 6),
            Text('${dataBr(item.dataDesejada)} • ${item.horarioDesejado ?? 'qualquer horário'}'),
            if (item.ativo)
              Wrap(
                spacing: 8,
                children: [
                  if (item.vagaLiberada)
                    FilledButton(onPressed: () => widget.aoAgendar(item.idServico), child: const Text('Agendar agora')),
                  TextButton(onPressed: () => _sair(item), child: const Text('Sair da fila')),
                ],
              ),
          ],
        ),
      ),
    );
  }
}
