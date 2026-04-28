// See https://aka.ms/new-console-template for more information
using KanbanBoard.AccesDonnee.EFCore;
using KanbanBoard.AccesDonnee.Models;
using Microsoft.EntityFrameworkCore;

const string ConnectionString =
    @"Server=(localdb)\MSSQLLocalDB;Database=KanbanBoardDb;Trusted_Connection=True;TrustServerCertificate=True";

Console.WriteLine("=== Test KanbanBoard - Connexion EF Core ===\n");

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(ConnectionString)
    .Options;

try
{
    using var db = new AppDbContext(options);

    // 1. Test de connexion
    Console.Write("Connexion à la base... ");
    var canConnect = await db.Database.CanConnectAsync();
    Console.WriteLine(canConnect ? "OK ✓" : "ÉCHEC ✗");

    if (!canConnect)
        return;

    // 2. Lecture des utilisateurs
    Console.WriteLine("\n--- Utilisateurs en base ---");
    var users = await db.USERs
        .OrderBy(u => u.Id)
        .ToListAsync();

    if (users.Count == 0)
    {
        Console.WriteLine("(aucun utilisateur trouvé)");
    }
    else
    {
        foreach (var u in users)
        {
            var roleLabel = u.IsGlobalAdmin == true ? "[ADMIN]" : "       ";
            Console.WriteLine($"  {roleLabel} #{u.Id,-3} {u.Username,-15} {u.Email}");
        }
        Console.WriteLine($"\nTotal : {users.Count} utilisateur(s)");
    }

    // 3. Comptage rapide des autres tables
    Console.WriteLine("\n--- Statistiques ---");
    Console.WriteLine($"  Boards     : {await db.BOARDs.CountAsync()}");
    Console.WriteLine($"  Colonnes   : {await db.BOARD_COLUMNs.CountAsync()}");
    Console.WriteLine($"  Cartes     : {await db.CARDs.CountAsync()}");
    Console.WriteLine($"  Labels     : {await db.LABELs.CountAsync()}");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ ERREUR : {ex.GetType().Name}");
    Console.WriteLine($"   {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"   Inner : {ex.InnerException.Message}");
}

Console.WriteLine("\nAppuie sur Entrée pour quitter.");
Console.ReadLine();
