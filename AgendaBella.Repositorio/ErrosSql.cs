using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AgendaBella.Repositorio;

// Utilitário interno: reconhece o erro de "valor duplicado" do SQL Server
// (número 2601 = índice único, 2627 = restrição UNIQUE/PRIMARY KEY).
internal static class ErrosSql
{
    public static bool EhViolacaoDeUnicidade(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
