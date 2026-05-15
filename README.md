# 🗂️ KanbanBoard

> **Application web Kanban collaborative temps réel** — Projet PDR 7, MBDS 2025-2026

Une application web complète de gestion de projets en mode Kanban (style Trello/Asana), construite en **ASP.NET Core 9.0 MVC** avec collaboration temps réel via **SignalR**, gestion fine des rôles, et thème clair/sombre.

---

## 📋 Sommaire

- [Vue d'ensemble](#-vue-densemble)
- [Fonctionnalités](#-fonctionnalités)
- [Stack technique](#-stack-technique)
- [Architecture](#-architecture)
- [Modèle de données](#-modèle-de-données)
- [Système de permissions](#-système-de-permissions)
- [Installation](#%EF%B8%8F-installation--exécution)
- [Notes pédagogiques](#-notes-pédagogiques)
- [Roadmap](#-roadmap)
- [Licence](#-licence)

---

## 🎯 Vue d'ensemble

**KanbanBoard** est une application de gestion de tâches collaborative qui permet à plusieurs utilisateurs de travailler simultanément sur un même tableau. Inspirée de Trello, Asana et Linear, elle implémente :

- 📋 **Tableaux Kanban** avec colonnes personnalisables et cartes drag & drop
- 🤝 **Collaboration temps réel** : tous les changements (déplacements, commentaires, ajouts) sont propagés instantanément aux autres utilisateurs
- 👥 **Gestion fine des rôles** : Owner / Admin / Member / Viewer
- 💬 **Commentaires** avec compteur de non-lus en temps réel
- 📎 **Pièces jointes** (images, PDF, Office)
- 🏷️ **Étiquettes (labels)** colorées avec assignation N:N
- 🔔 **Notifications personnelles** (invitations, assignations, etc.)
- 🕐 **Journal d'activité** complet par tableau (audit log)
- 🌙 **Mode clair / sombre** avec basculement persistant

---

## ✨ Fonctionnalités

### 🔐 Authentification & utilisateurs
- Inscription / connexion par email + mot de passe
- Hachage des mots de passe via **BCrypt** (60 caractères, sel inclus)
- Authentification par cookie maison (sans Identity Framework)
- Validation côté serveur des emails et mots de passe

### 📋 Tableaux Kanban
- Création / consultation / modification de tableaux
- Colonnes personnalisables (création, renommage, suppression, réordonnancement)
- 3 colonnes par défaut à la création (`À faire` / `En cours` / `Terminé`)
- Suppression sécurisée avec confirmation des cartes contenues

### 🎴 Cartes
- **CRUD complet** : titre, description, priorité (Low/Medium/High/Critical), date d'échéance, assignation
- **Drag & drop** entre colonnes avec Sortable.js + synchronisation temps réel
- **Verrouillage optimiste** via `ROWVERSION` SQL Server (évite les conflits)
- **Vue Détails** dédiée (consultation) séparée de la **Vue Édition** (modification)
- Suppression sécurisée avec confirmation

### 👥 Gestion des membres & rôles
- 4 rôles : **Owner** (créateur, immuable) / **Admin** (gestion complète) / **Member** (édition cartes) / **Viewer** (lecture seule)
- Invitation par email avec autocomplete des utilisateurs existants
- Changement de rôle, retrait de membre, quitter un tableau
- Restrictions strictes côté serveur (impossible de retirer le Owner)

### 💬 Commentaires
- Ajout / suppression de commentaires sur chaque carte
- Compteur de commentaires **non-lus** par carte (table `CARD_READ`)
- Badge animé rouge sur les cartes avec des commentaires non lus
- Marquage automatique comme lu lors de la consultation

### 📎 Pièces jointes
- Upload de fichiers (10 MB max) : images, PDF, Office, txt
- Validation côté client (taille) ET serveur (taille + extension)
- Stockage dans `wwwroot/uploads/` avec préfixe GUID anti-collision
- Téléchargement direct, vignette pour les images
- Suppression par l'auteur ou un Admin

### 🏷️ Étiquettes (labels)
- Création de labels propres à chaque tableau (nom + couleur hexa `#RRGGBB`)
- 8 couleurs préréglées (Rouge, Orange, Jaune, Vert, Bleu, Violet, Rose, Gris)
- Color picker HTML5 + aperçu live
- Relation **N:N** avec les cartes (table de jointure `CARD_LABEL`)
- Modification / suppression avec impact visible (combien de cartes affectées)
- Affichage des badges colorés en haut des cartes Kanban
- Assignation/désassignation depuis la vue Détails

### 🔔 Notifications personnelles
- Notification au destinataire pour : invitation, retrait, changement de rôle, assignation, commentaire, ajout de pièce jointe
- Cloche dans la navbar (visible sur toutes les pages) avec badge `unread`
- Marquage individuel ou en masse comme lu
- Propagation **temps réel** via SignalR

### 🕐 Journal d'activité
- Audit complet par tableau : créations, modifications, suppressions, déplacements
- 25 types d'actions trackées (cartes, colonnes, membres, commentaires, attachements, labels)
- Affichage en panneau latéral (offcanvas Bootstrap)
- Détails enrichis : *"Times a déplacé la carte 'task 1' de 'En cours' vers 'À faire'"*
- Auto-purge à 100 entrées par tableau (rotation)
- Mise à jour temps réel quand le panneau est ouvert

### ⚡ Temps réel (SignalR)
- Toutes les actions sont propagées aux autres utilisateurs connectés au tableau
- Groupes SignalR : `board-{boardId}` et `user-{userId}`
- Reconnexion automatique en cas de perte de connexion
- Toasts non-bloquants pour signaler les changements externes

### 🎨 Interface utilisateur
- Design inspiré de **Linear / Notion** (typographie Inter, ombres douces violettes)
- **Mode clair** (défaut) : fonds cream/beige chaleureux, cartes blanches éclatantes
- **Mode sombre** : fonds noirs avec teinte violette, contraste optimisé
- Toggle 🌙/☀️ persisté en `localStorage`
- Anti-flash : application du thème avant le rendu de la page
- Animations subtiles (hover lift, transitions 0.2s)

---

## 🚀 Stack technique

| Couche | Technologie |
|---|---|
| **Framework** | ASP.NET Core 9.0 MVC |
| **ORM** | Entity Framework Core 9.0 (Database First / Scaffolding) |
| **Base de données** | SQL Server 2022 (LocalDB en dev) |
| **Authentification** | Cookie Authentication maison + BCrypt.Net-Next 4.0 |
| **Temps réel** | SignalR 9.0 |
| **Frontend** | Razor Views, Bootstrap 5, Sortable.js, JavaScript vanilla |
| **Validation** | DataAnnotations + ModelState |
| **CSS** | CSS Variables (custom), Inter (Google Fonts) |
| **Versioning** | Git + GitHub |

---

## 🏗️ Architecture

Le projet suit une architecture **N-tier (3 couches)** avec une stricte séparation des responsabilités :
KanbanBoard.sln
│
├── 📦 KanbanBoard.LibrairieMetier      → Contrats et modèles partagés
│   ├── Interfaces/                      → IBoardDA, ICardDA, ILabelDA, etc. (pattern Repository)
│   ├── ViewModels/                      → DTOs pour le transit Controller ↔ Vue
│   ├── Constants/                       → ActivityAction, ActivityEntityType
│   └── Results/                         → Enums pour les résultats d'opération
│
├── 🗄️ KanbanBoard.AccesDonnee          → Couche d'accès aux données (EF Core)
│   ├── Models/                          → Entités scaffoldées depuis SQL Server
│   ├── EFCore/AppDbContext.cs           → DbContext, configuration EF
│   └── Implementations/                 → BoardDA, CardDA, LabelDA, etc.
│
├── 🌐 KanbanBoard.Web                  → Présentation (MVC + SignalR)
│   ├── Controllers/                     → 9 controllers (Account, Board, Card, Column, Comment, Attachment, Label, Activity, Notification)
│   ├── Views/                           → Vues Razor par area
│   ├── Hubs/KanbanHub.cs                → Hub SignalR
│   ├── Services/                        → ActivityLogService, NotificationService
│   ├── wwwroot/css/site-custom.css      → Thème Linear/Notion (clair + sombre)
│   └── Program.cs                       → DI, middleware, configuration
│
└── 🧪 KanbanBoard.ConsoleApp           → Tests rapides du métier (sans le Web)

### Pattern Repository

Toutes les classes DA (Data Access) implémentent une interface définie dans `LibrairieMetier` :

```csharp
public interface IBoardDA
{
    Task<KanbanBoardViewModel?> GetBoardDetailsAsync(int boardId, int userId);
    Task<int> CreateBoardAsync(int ownerId, string title, string? description);
    Task<bool> UserHasAccessAsync(int boardId, int userId);
    // ...
}
```

Cela permet :
- **Découplage** : la couche Web ne dépend que des interfaces
- **Testabilité** : on peut substituer une implémentation mock
- **DI claire** dans `Program.cs`

---

## 🗺️ Modèle de données

12+ tables avec des contraintes `CHECK` SQL Server pour garantir l'intégrité métier.

### Tables principales

| Table | Rôle |
|---|---|
| **USER** | Utilisateurs (Username, Email, PasswordHash BCrypt) |
| **BOARD** | Tableaux Kanban (Title, Description, OwnerId) |
| **BOARD_MEMBER** | Membres d'un tableau (BoardId, UserId, Role) — N:N |
| **BOARD_COLUMN** | Colonnes d'un tableau avec `Position` + `RowVersion` |
| **CARD** | Cartes (Title, Description, Priority, DueDate, AssigneeId, IsArchived, `RowVersion`) |
| **COMMENT** | Commentaires sur les cartes |
| **ATTACHMENT** | Pièces jointes (FileName, FileUrl, FileSizeKB, UploadedById) |
| **LABEL** | Étiquettes propres à un tableau (Name, Color `#RRGGBB`) |
| **CARD_LABEL** | Liaison N:N entre Cards et Labels |
| **CARD_READ** | Suivi des "lectures" pour le calcul des unread comments |
| **NOTIFICATION** | Notifications personnelles (Type, Message, IsRead, ActorId, UserId) |
| **ACTIVITY_LOG** | Journal d'audit par tableau (25 actions, Details NVARCHAR(500)) |

### Contraintes métier

- `CK_USER_Email` : format email valide
- `CK_LABEL_Color` : hexadécimal `#RRGGBB`
- `CK_BOARD_MEMBER_Role` : `Owner` / `Admin` / `Member` / `Viewer`
- `CK_CARD_Priority` : `Low` / `Medium` / `High` / `Critical`
- `CK_ACTIVITY_LOG_Action` : 25 actions valides
- `CK_ACTIVITY_LOG_EntityType` : 7 types d'entités

### Verrouillage optimiste

Les tables `CARD` et `BOARD_COLUMN` ont une colonne `RowVersion` (type `ROWVERSION`) qui change à chaque update. EF Core détecte les conflits de concurrence et lève une `DbUpdateConcurrencyException`.

### Cascades

- `CASCADE` pour les relations fortes (Board → Column → Card → Comment)
- `NO ACTION` pour les multi-paths (CARD_READ, NOTIFICATION.CardId) — gestion manuelle dans le code pour éviter les références cycliques

### 🔗 Diagramme interactif

👉 [Voir le diagramme sur dbdiagram.io](https://dbdiagram.io/d/69f0e0ceddb9320fdc7caf4c)

---

## 🔐 Système de permissions

Quatre rôles hiérarchiques :

| Rôle | Permissions |
|---|---|
| **Owner** | Tout (immuable, ne peut pas être retiré ni rétrogradé) |
| **Admin** | Créer/supprimer cartes & colonnes, gérer membres, gérer labels |
| **Member** | Créer/modifier cartes, commenter, uploader, assigner labels |
| **Viewer** | Lecture seule (voir le tableau, télécharger les pièces jointes) |

Les vérifications sont **toujours faites côté serveur** dans chaque action de controller, jamais uniquement en JavaScript.

---

## ⚙️ Installation & exécution

### Prérequis
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server LocalDB (inclus avec Visual Studio 2022) ou SQL Server 2022
- Git

### 1. Cloner le projet
```bash
git clone https://github.com/times13/KanbanBoard.git
cd KanbanBoard
```

### 2. Créer la base de données
```bash
# Lancer le script SQL dans SSMS ou via sqlcmd
sqlcmd -S "(localdb)\MSSQLLocalDB" -i sql/01_create_database.sql
```

### 3. Configurer la connexion
Créer `KanbanBoard.Web/appsettings.Development.json` :

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=KanbanBoardDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 4. Restaurer et builder
```bash
dotnet restore
dotnet build
```

### 5. Lancer l'application
```bash
dotnet run --project KanbanBoard.Web
```

L'application sera disponible sur `https://localhost:7249`.

### 6. (Optionnel) Tester le métier en console
```bash
dotnet run --project KanbanBoard.ConsoleApp
```

---

## 🎓 Notes pédagogiques

Ce projet démontre la maîtrise des concepts suivants :

### Patterns d'architecture
- **N-tier architecture** : séparation stricte présentation / accès données / contrats
- **Repository pattern** : interfaces dans LibrairieMetier, implémentations dans AccesDonnee
- **Dependency Injection** : tous les DA et services injectés via `Program.cs`
- **DTO / ViewModel pattern** : pas de fuite des entités EF vers les vues

### Bases de données
- **Database First** avec scaffolding EF Core
- **Contraintes CHECK** SQL Server pour l'intégrité métier
- **Verrouillage optimiste** via `ROWVERSION`
- **Cascades intelligentes** (CASCADE vs NO ACTION) selon les relations
- **Relations N:N** (CARD_LABEL, BOARD_MEMBER)
- **Audit log** avec rotation automatique

### Sécurité
- **Hachage BCrypt** des mots de passe (jamais en clair)
- **Cookie Authentication** avec claims
- **Anti-forgery tokens** sur toutes les actions POST
- **Validation côté serveur** systématique
- **Autorisation rôle-based** dans chaque controller

### Temps réel
- **SignalR Hubs** avec groupes par tableau
- **Reconnexion automatique** côté client
- **Race conditions** gérées (HTTP redirect vs SignalR reload)

### Frontend moderne
- **CSS Variables** pour le theming (clair/sombre)
- **localStorage** pour la persistance utilisateur
- **Drag & drop** avec Sortable.js
- **Fetch API + JSON** pour les interactions AJAX
- **Bootstrap 5** avec overrides custom

---

## 🛣️ Roadmap

Features identifiées pour des évolutions futures :

- 🗃️ **Vue Archives** : afficher les cartes archivées (`IsArchived = true` + `ArchivedAt` déjà en base)
- 🔍 **Filtres** : filtrer par label, assignee, priorité, date d'échéance
- 🔎 **Recherche full-text** sur les titres et descriptions
- 📅 **Vue Calendrier** : alternative au Kanban basée sur les dates d'échéance
- 📊 **Statistiques** : nombre de cartes par colonne/membre, vélocité, etc.
- 📱 **Mode mobile** : refonte responsive pour smartphones
- 🌐 **Internationalisation** (i18n) : multi-langue
- 📧 **Notifications par email** en complément du temps réel

---

## Auteurs

Étudiant MBDS 2025-2026 (Faculté des Sciences UEH × Université Côte d'Azur)


---

## 📄 Licence

Projet académique réalisé dans le cadre du Master 2 MBDS (Multimédia, Bases de Données et Systèmes d'Information).

Année universitaire **2025-2026**.