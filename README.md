# KanbanBoard

Tableau Kanban de Collaboration d'Équipe — Projet PDR 7, MBDS 2025-2026.

## Stack technique

- **Backend** : ASP.NET Core 9.0 MVC + SignalR
- **ORM** : Entity Framework Core 9.0 (Code First)
- **Base de données** : SQL Server (LocalDB en développement)
- **Auth** : Cookie authentication (maison) + BCrypt
- **Front** : Razor + Bootstrap + Sortable.js

## Architecture (3-tier)

KanbanBoard.LibrairieMetier   → Entités + interfaces IxxxDA
KanbanBoard.AccesDonnee       → EF Core, AppDbContext, implémentations DA
KanbanBoard.Web               → Controllers, Views, SignalR Hubs
KanbanBoard.ConsoleApp        → Tests rapides du Métier sans le Web
