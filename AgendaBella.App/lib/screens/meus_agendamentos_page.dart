import 'package:flutter/material.dart';

import '../config.dart';
import '../models/modelos.dart';
import '../services/api_service.dart';
import '../tema.dart';
import '../util/formatos.dart';
import '../widgets/componentes.dart';
import 'reagendar_screen.dart';

/// Aba "Meus horários": lista os agendamentos com as ações de reagendar, cancelar e ver o histórico.
class MeusAgendamentosPage extends StatefulWidget {
  const MeusAgendamentosPage({super.key});

  @override
  State<MeusAgendamentosPage> createState() => _MeusAgendamentosPageState();
}

class _MeusAgendamentosPageState extends State<MeusAgendamentosPage> {
  List<Agendamento> _agendamentos = [];
  bool _carregando = true;
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
      final lista = await api.meusAgendamentos();
      if (!mounted) return;
      setState(() => _agendamentos = lista);
    } on ApiException catch (e) {
      if (mounted) setState(() => _erro = e.mensagem);
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  Future<void> _cancelar(Agendamento a) async {
    final confirmou = await showDialog<bool>(
      context: context,
      builder: (contexto) => AlertDialog(
        title: const Text('Cancelar agendamento?'),
        content: Text('${a.servicoNome} em ${dataBr(a.data)} às ${a.horario}.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(contexto, false), child: const Text('Manter')),
          FilledButton(onPressed: () => Navigator.pop(contexto, true), child: const Text('Cancelar agendamento')),
        ],
      ),
    );
    if (confirmou != true) return;

    try {
      await api.cancelar(a.id);
      if (!mounted) return;
      avisar(context, 'Agendamento cancelado.');
      await _carregar();
    } on ApiException catch (e) {
      if (mounted) avisar(context, e.mensagem, erro: true);
    }
  }

  Future<void> _reagendar(Agendamento a) async {
    final mudou = await Navigator.of(context).push<bool>(
      MaterialPageRoute(builder: (_) => ReagendarScreen(agendamento: a)),
    );
    if (mudou == true) await _carregar();
  }

  void _verHistorico(Agendamento a) {
    showModalBottomSheet<void>(
      context: context,
      showDragHandle: true,
      builder: (_) => _Historico(agendamento: a),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (_carregando) return const Center(child: CircularProgressIndicator());
    final erro = _erro;
    if (erro != null) return MensagemErro(mensagem: erro, aoTentarDeNovo: _carregar);

    return RefreshIndicator(
      onRefresh: _carregar,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          if (_agendamentos.isEmpty)
            const Padding(
              padding: EdgeInsets.only(top: 48),
              child: Center(child: Text('Você ainda não tem agendamentos.')),
            ),
          for (final a in _agendamentos) LarguraMaxima(child: _cartao(a)),
        ],
      ),
    );
  }

  Widget _cartao(Agendamento a) {
    final imagem = a.imagemInspiracao;

    return Card(
      color: Colors.white,
      margin: const EdgeInsets.only(bottom: 12),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(a.servicoNome,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
                ),
                StatusChip(a.status),
              ],
            ),
            const SizedBox(height: 8),
            Text('${dataBr(a.data)} às ${a.horario} • ${a.duracao} min • ${moeda(a.preco)}'),
            if (a.observacao != null) ...[
              const SizedBox(height: 6),
              Text('Observação: ${a.observacao}', style: const TextStyle(color: corVinho)),
            ],
            if (imagem != null) ...[
              const SizedBox(height: 8),
              ClipRRect(
                borderRadius: BorderRadius.circular(12),
                child: Image.network(
                  '${Config.apiUrl}/uploads/$imagem',
                  width: 88,
                  height: 88,
                  fit: BoxFit.cover,
                  semanticLabel: 'Foto de inspiração enviada',
                  errorBuilder: (contexto, erro, pilha) => const SizedBox(
                    width: 88,
                    height: 88,
                    child: Icon(Icons.broken_image_outlined),
                  ),
                ),
              ),
            ],
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              runSpacing: 4,
              children: [
                if (a.podeReagendar)
                  OutlinedButton(onPressed: () => _reagendar(a), child: const Text('Reagendar')),
                if (a.podeCancelar)
                  OutlinedButton(onPressed: () => _cancelar(a), child: const Text('Cancelar')),
                TextButton(onPressed: () => _verHistorico(a), child: const Text('Histórico')),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

/// Histórico do agendamento (registrado pela trigger TR_Agendamento_Historico).
class _Historico extends StatelessWidget {
  final Agendamento agendamento;

  const _Historico({required this.agendamento});

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<EventoHistorico>>(
      future: api.historico(agendamento.id),
      builder: (context, resultado) {
        if (resultado.connectionState != ConnectionState.done) {
          return const SizedBox(height: 160, child: Center(child: CircularProgressIndicator()));
        }
        if (resultado.hasError) {
          return Padding(padding: const EdgeInsets.all(24), child: Text(resultado.error.toString()));
        }

        final eventos = resultado.data ?? [];
        return SafeArea(
          child: ListView(
            shrinkWrap: true,
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
            children: [
              Text('Histórico - ${agendamento.servicoNome}',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(fontWeight: FontWeight.w700)),
              const SizedBox(height: 8),
              for (final e in eventos)
                ListTile(
                  leading: const Icon(Icons.circle, size: 12, color: corRosa),
                  title: Text(e.evento),
                  subtitle: Text('${e.descricao}\n${dataHoraBr(e.dataEvento)}'),
                  isThreeLine: true,
                ),
            ],
          ),
        );
      },
    );
  }
}
