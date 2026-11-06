import 'package:flutter/material.dart';

import '../models/modelos.dart';
import '../services/api_service.dart';
import '../util/formatos.dart';

/// Escolha de DATA + HORÁRIO LIVRE, usada em "Agendar" e em "Reagendar".
///
/// Ao escolher a data, pergunta à Api quais horários estão livres
/// (a Api executa a stored procedure sp_Horarios_Disponiveis) e mostra
/// cada horário como um botão. Avisa a tela "mãe" a cada mudança pelo [aoMudar].
class SeletorDataHorario extends StatefulWidget {
  final int idServico;

  /// No reagendamento: id do próprio agendamento, para ele não bloquear o novo horário.
  final int? ignorarAgendamento;

  final void Function(DateTime? data, String? horario) aoMudar;

  /// O que mostrar quando o dia não tem nenhum horário (ex.: botão da lista de espera).
  final Widget Function(DateTime data)? quandoSemHorario;

  const SeletorDataHorario({
    super.key,
    required this.idServico,
    required this.aoMudar,
    this.ignorarAgendamento,
    this.quandoSemHorario,
  });

  @override
  State<SeletorDataHorario> createState() => _SeletorDataHorarioState();
}

class _SeletorDataHorarioState extends State<SeletorDataHorario> {
  DateTime? _data;
  String? _horario;
  List<HorarioDisponivel>? _horarios;
  bool _carregando = false;
  String? _erro;

  Future<void> _escolherData() async {
    final hoje = DateTime.now();
    final escolhida = await showDatePicker(
      context: context,
      initialDate: _data ?? hoje,
      firstDate: DateTime(hoje.year, hoje.month, hoje.day),
      lastDate: hoje.add(const Duration(days: 90)),
      helpText: 'Data do atendimento',
    );
    if (escolhida == null) return;

    setState(() {
      _data = escolhida;
      _horario = null;
    });
    widget.aoMudar(escolhida, null);
    await _carregarHorarios();
  }

  Future<void> _carregarHorarios() async {
    final data = _data;
    if (data == null) return;

    setState(() {
      _carregando = true;
      _erro = null;
    });
    try {
      final lista = await api.horarios(data, widget.idServico, ignorar: widget.ignorarAgendamento);
      if (!mounted) return;
      setState(() => _horarios = lista);
    } on ApiException catch (e) {
      if (!mounted) return;
      setState(() => _erro = e.mensagem);
    } finally {
      if (mounted) setState(() => _carregando = false);
    }
  }

  void _escolherHorario(String horario) {
    setState(() => _horario = horario);
    widget.aoMudar(_data, horario);
  }

  @override
  Widget build(BuildContext context) {
    final data = _data;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        OutlinedButton.icon(
          onPressed: _escolherData,
          icon: const Icon(Icons.calendar_month),
          label: Text(data == null ? 'Escolher a data' : 'Data: ${dataBr(isoData(data))}'),
        ),
        const SizedBox(height: 16),
        if (data != null) _areaHorarios(data),
      ],
    );
  }

  Widget _areaHorarios(DateTime data) {
    if (_carregando) {
      return const Padding(
        padding: EdgeInsets.all(12),
        child: Center(child: CircularProgressIndicator()),
      );
    }

    final erro = _erro;
    if (erro != null) {
      return Row(
        children: [
          Expanded(child: Text(erro, style: const TextStyle(color: Color(0xFFB02A37)))),
          TextButton(onPressed: _carregarHorarios, child: const Text('Tentar de novo')),
        ],
      );
    }

    final horarios = _horarios ?? [];
    if (horarios.isEmpty) {
      return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text('Não há horários livres neste dia.'),
          if (widget.quandoSemHorario != null) widget.quandoSemHorario!(data),
        ],
      );
    }

    return Wrap(
      spacing: 8,
      runSpacing: 8,
      children: [
        for (final h in horarios)
          ChoiceChip(
            label: Text(h.horario),
            selected: h.horario == _horario,
            onSelected: (_) => _escolherHorario(h.horario),
          ),
      ],
    );
  }
}
