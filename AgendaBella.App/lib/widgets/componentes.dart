import 'package:flutter/material.dart';

import '../tema.dart';
import '../util/formatos.dart';

/// Selo colorido com o status (do agendamento ou da lista de espera).
/// A cor nunca é a única informação: o texto do status aparece junto (acessibilidade).
class StatusChip extends StatelessWidget {
  final String status;

  const StatusChip(this.status, {super.key});

  @override
  Widget build(BuildContext context) {
    Color fundo = const Color(0x33FFC107);
    Color texto = const Color(0xFF7A5B00);
    var rotulo = textoStatus(status);

    if (status == 'Confirmado') {
      fundo = const Color(0x26198754);
      texto = const Color(0xFF146C43);
    } else if (status == 'Concluido' || status == 'Atendido') {
      fundo = const Color(0x1F0D6EFD);
      texto = const Color(0xFF0A58CA);
    } else if (status == 'Cancelado') {
      fundo = const Color(0x24DC3545);
      texto = const Color(0xFFB02A37);
    } else if (status == 'Notificado') {
      fundo = const Color(0x26198754);
      texto = const Color(0xFF146C43);
      rotulo = 'Vaga liberada!';
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 5),
      decoration: BoxDecoration(color: fundo, borderRadius: BorderRadius.circular(30)),
      child: Text(rotulo, style: TextStyle(color: texto, fontWeight: FontWeight.w600)),
    );
  }
}

/// Mensagem de erro com botão "Tentar de novo".
class MensagemErro extends StatelessWidget {
  final String mensagem;
  final VoidCallback aoTentarDeNovo;

  const MensagemErro({super.key, required this.mensagem, required this.aoTentarDeNovo});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.wifi_off, size: 48, color: corVinho),
            const SizedBox(height: 12),
            Text(mensagem, textAlign: TextAlign.center),
            const SizedBox(height: 16),
            OutlinedButton(onPressed: aoTentarDeNovo, child: const Text('Tentar de novo')),
          ],
        ),
      ),
    );
  }
}

/// Título de seção dentro das telas.
class TituloSecao extends StatelessWidget {
  final String texto;

  const TituloSecao(this.texto, {super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 20, bottom: 10),
      child: Semantics(
        header: true,
        child: Text(
          texto,
          style: Theme.of(context).textTheme.titleMedium?.copyWith(color: corVinho, fontWeight: FontWeight.w700),
        ),
      ),
    );
  }
}

/// Limita a largura do conteúdo em telas grandes (tablet, navegador),
/// para o texto não ficar esticado de ponta a ponta.
class LarguraMaxima extends StatelessWidget {
  final Widget child;
  final double largura;

  const LarguraMaxima({super.key, required this.child, this.largura = 640});

  @override
  Widget build(BuildContext context) {
    return Align(
      alignment: Alignment.topCenter,
      child: ConstrainedBox(constraints: BoxConstraints(maxWidth: largura), child: child),
    );
  }
}

/// Mostra uma mensagem rápida no rodapé da tela.
void avisar(BuildContext context, String mensagem, {bool erro = false}) {
  ScaffoldMessenger.of(context)
    ..hideCurrentSnackBar()
    ..showSnackBar(SnackBar(
      content: Text(mensagem),
      backgroundColor: erro ? const Color(0xFFB02A37) : corVinho,
    ));
}
