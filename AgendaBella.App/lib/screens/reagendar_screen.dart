import 'package:flutter/material.dart';

import '../models/modelos.dart';
import '../services/api_service.dart';
import '../util/formatos.dart';
import '../widgets/componentes.dart';
import '../widgets/seletor_data_horario.dart';

/// Tela de reagendamento. Ao confirmar, volta para a lista devolvendo "true" (mudou).
class ReagendarScreen extends StatefulWidget {
  final Agendamento agendamento;

  const ReagendarScreen({super.key, required this.agendamento});

  @override
  State<ReagendarScreen> createState() => _ReagendarScreenState();
}

class _ReagendarScreenState extends State<ReagendarScreen> {
  DateTime? _data;
  String? _horario;
  bool _enviando = false;

  Future<void> _confirmar() async {
    final data = _data;
    final horario = _horario;
    if (data == null || horario == null) return;

    setState(() => _enviando = true);
    try {
      await api.reagendar(widget.agendamento.id, data, horario);
      if (!mounted) return;
      avisar(context, 'Agendamento reagendado.');
      Navigator.of(context).pop(true);
    } on ApiException catch (e) {
      if (mounted) avisar(context, e.mensagem, erro: true);
    } finally {
      if (mounted) setState(() => _enviando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final a = widget.agendamento;

    return Scaffold(
      appBar: AppBar(title: const Text('Reagendar')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          LarguraMaxima(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(a.servicoNome, style: Theme.of(context).textTheme.titleLarge),
                Text('Horário atual: ${dataBr(a.data)} às ${a.horario}'),
                const TituloSecao('Novo dia e horário'),
                SeletorDataHorario(
                  idServico: a.idServico,
                  ignorarAgendamento: a.id, // o horário atual não bloqueia o novo
                  aoMudar: (data, horario) => setState(() {
                    _data = data;
                    _horario = horario;
                  }),
                ),
                const SizedBox(height: 24),
                FilledButton(
                  onPressed: (_data != null && _horario != null && !_enviando) ? _confirmar : null,
                  child: const Text('Confirmar novo horário'),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
