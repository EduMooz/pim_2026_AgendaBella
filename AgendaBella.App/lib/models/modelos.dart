// MODELOS: espelham os DTOs da Api (AgendaBella.Modelos/Dtos).
// O "fromJson" transforma o JSON recebido em objeto Dart;
// os nomes das chaves são os mesmos que a Api envia (camelCase).

class Usuario {
  final int id;
  final String nome;
  final String email;
  final String perfil;

  Usuario({required this.id, required this.nome, required this.email, required this.perfil});

  factory Usuario.fromJson(Map<String, dynamic> json) => Usuario(
        id: json['id'] as int,
        nome: json['nome'] as String,
        email: json['email'] as String,
        perfil: json['perfil'] as String,
      );

  Map<String, dynamic> toJson() => {'id': id, 'nome': nome, 'email': email, 'perfil': perfil};
}

class Servico {
  final int id;
  final String nome;
  final String? descricao;
  final double preco;
  final int duracao;

  Servico({required this.id, required this.nome, this.descricao, required this.preco, required this.duracao});

  factory Servico.fromJson(Map<String, dynamic> json) => Servico(
        id: json['id'] as int,
        nome: json['nome'] as String,
        descricao: json['descricao'] as String?,
        preco: (json['preco'] as num).toDouble(),
        duracao: json['duracao'] as int,
      );
}

class HorarioDisponivel {
  final int idHorario;
  final String horario; // "14:00"

  HorarioDisponivel({required this.idHorario, required this.horario});

  factory HorarioDisponivel.fromJson(Map<String, dynamic> json) =>
      HorarioDisponivel(idHorario: json['idHorario'] as int, horario: json['horario'] as String);
}

class Agendamento {
  final int id;
  final int idServico;
  final String servicoNome;
  final double preco;
  final int duracao;
  final String data; // "2026-10-15"
  final String horario; // "14:00"
  final String status;
  final String? imagemInspiracao;
  final String? observacao;
  final bool podeCancelar;
  final bool podeReagendar;

  Agendamento({
    required this.id,
    required this.idServico,
    required this.servicoNome,
    required this.preco,
    required this.duracao,
    required this.data,
    required this.horario,
    required this.status,
    this.imagemInspiracao,
    this.observacao,
    required this.podeCancelar,
    required this.podeReagendar,
  });

  factory Agendamento.fromJson(Map<String, dynamic> json) => Agendamento(
        id: json['id'] as int,
        idServico: json['idServico'] as int,
        servicoNome: json['servicoNome'] as String,
        preco: (json['preco'] as num).toDouble(),
        duracao: json['duracao'] as int,
        data: json['data'] as String,
        horario: json['horario'] as String,
        status: json['status'] as String,
        imagemInspiracao: json['imagemInspiracao'] as String?,
        observacao: json['observacao'] as String?,
        podeCancelar: json['podeCancelar'] as bool,
        podeReagendar: json['podeReagendar'] as bool,
      );
}

class EventoHistorico {
  final String evento;
  final String descricao;
  final DateTime dataEvento;

  EventoHistorico({required this.evento, required this.descricao, required this.dataEvento});

  factory EventoHistorico.fromJson(Map<String, dynamic> json) => EventoHistorico(
        evento: json['evento'] as String,
        descricao: json['descricao'] as String,
        dataEvento: DateTime.parse(json['dataEvento'] as String),
      );
}

class ItemListaEspera {
  final int id;
  final int idServico;
  final String servicoNome;
  final String dataDesejada;
  final String? horarioDesejado;
  final String status; // Aguardando | Notificado | Atendido | Cancelado

  ItemListaEspera({
    required this.id,
    required this.idServico,
    required this.servicoNome,
    required this.dataDesejada,
    this.horarioDesejado,
    required this.status,
  });

  bool get vagaLiberada => status == 'Notificado';
  bool get ativo => status == 'Aguardando' || status == 'Notificado';

  factory ItemListaEspera.fromJson(Map<String, dynamic> json) => ItemListaEspera(
        id: json['id'] as int,
        idServico: json['idServico'] as int,
        servicoNome: json['servicoNome'] as String,
        dataDesejada: json['dataDesejada'] as String,
        horarioDesejado: json['horarioDesejado'] as String?,
        status: json['status'] as String,
      );
}

class Preferencia {
  final String? formatoUnha;
  final String? corFavorita;
  final String? estilo;
  final String? observacao;

  Preferencia({this.formatoUnha, this.corFavorita, this.estilo, this.observacao});

  factory Preferencia.fromJson(Map<String, dynamic> json) => Preferencia(
        formatoUnha: json['formatoUnha'] as String?,
        corFavorita: json['corFavorita'] as String?,
        estilo: json['estilo'] as String?,
        observacao: json['observacao'] as String?,
      );

  Map<String, dynamic> toJson() => {
        'formatoUnha': formatoUnha,
        'corFavorita': corFavorita,
        'estilo': estilo,
        'observacao': observacao,
      };
}

class MovimentoPontos {
  final int quantidade; // positivo = crédito, negativo = débito
  final String? descricao;
  final DateTime data;

  MovimentoPontos({required this.quantidade, this.descricao, required this.data});

  factory MovimentoPontos.fromJson(Map<String, dynamic> json) => MovimentoPontos(
        quantidade: json['quantidade'] as int,
        descricao: json['descricao'] as String?,
        data: DateTime.parse(json['data'] as String),
      );
}

class SaldoPontos {
  final int saldo;
  final List<MovimentoPontos> extrato;

  SaldoPontos({required this.saldo, required this.extrato});

  factory SaldoPontos.fromJson(Map<String, dynamic> json) => SaldoPontos(
        saldo: json['saldo'] as int,
        extrato: (json['extrato'] as List<dynamic>)
            .map((e) => MovimentoPontos.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}
