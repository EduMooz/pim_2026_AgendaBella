import 'package:flutter/material.dart';

import '../services/api_service.dart';
import 'agendar_page.dart';
import 'inicio_page.dart';
import 'lista_espera_page.dart';
import 'meus_agendamentos_page.dart';
import 'perfil_page.dart';

/// Tela principal depois do login. Tem 5 abas.
///
/// RESPONSIVIDADE: em telas estreitas (celular) as abas ficam embaixo (NavigationBar);
/// em telas largas (tablet, navegador, computador) ficam na lateral (NavigationRail).
class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  int _aba = 0;
  int? _servicoPreSelecionado;
  int _vagasLiberadas = 0;

  static const _titulos = ['Início', 'Agendar', 'Meus horários', 'Lista de espera', 'Perfil'];

  @override
  void initState() {
    super.initState();
    _atualizarAvisos();
  }

  /// Conta quantas vagas foram liberadas para a cliente (status "Notificado", gravado pela trigger).
  Future<void> _atualizarAvisos() async {
    try {
      final lista = await api.listaEspera();
      if (!mounted) return;
      setState(() => _vagasLiberadas = lista.where((i) => i.vagaLiberada).length);
    } on ApiException {
      // Sem conexão: o aviso só não aparece. As telas mostram o erro.
    }
  }

  void _trocarAba(int aba) {
    setState(() => _aba = aba);
    _atualizarAvisos();
  }

  /// Usado pela tela Início e pela Lista de espera: abre "Agendar" com o serviço já escolhido.
  void _agendarServico(int idServico) {
    setState(() {
      _servicoPreSelecionado = idServico;
      _aba = 1;
    });
  }

  Widget _paginaAtual() {
    switch (_aba) {
      case 0:
        return InicioPage(aoAgendar: _agendarServico);
      case 1:
        // A "key" recria a tela quando o serviço pré-selecionado muda.
        return AgendarPage(
          key: ValueKey(_servicoPreSelecionado ?? 0),
          idServicoInicial: _servicoPreSelecionado,
          aoConcluir: () => _trocarAba(2),
        );
      case 2:
        return const MeusAgendamentosPage();
      case 3:
        return ListaEsperaPage(aoAgendar: _agendarServico);
      default:
        return PerfilPage(aoAlterarDados: _atualizarAvisos);
    }
  }

  Widget _icone(IconData icone, int aba) {
    // Selo com o número de vagas liberadas na aba "Lista de espera".
    if (aba == 3 && _vagasLiberadas > 0) {
      return Badge(label: Text('$_vagasLiberadas'), child: Icon(icone));
    }
    return Icon(icone);
  }

  @override
  Widget build(BuildContext context) {
    const icones = [Icons.home_outlined, Icons.event_available, Icons.list_alt, Icons.hourglass_top, Icons.person_outline];

    return LayoutBuilder(
      builder: (context, restricoes) {
        final telaLarga = restricoes.maxWidth >= 720;

        if (telaLarga) {
          return Scaffold(
            appBar: AppBar(title: Text(_titulos[_aba])),
            body: Row(
              children: [
                NavigationRail(
                  selectedIndex: _aba,
                  onDestinationSelected: _trocarAba,
                  labelType: NavigationRailLabelType.all,
                  destinations: [
                    for (var i = 0; i < _titulos.length; i++)
                      NavigationRailDestination(icon: _icone(icones[i], i), label: Text(_titulos[i])),
                  ],
                ),
                const VerticalDivider(width: 1),
                Expanded(child: _paginaAtual()),
              ],
            ),
          );
        }

        return Scaffold(
          appBar: AppBar(title: Text(_titulos[_aba])),
          body: _paginaAtual(),
          bottomNavigationBar: NavigationBar(
            selectedIndex: _aba,
            onDestinationSelected: _trocarAba,
            destinations: [
              for (var i = 0; i < _titulos.length; i++)
                NavigationDestination(icon: _icone(icones[i], i), label: _titulos[i]),
            ],
          ),
        );
      },
    );
  }
}
