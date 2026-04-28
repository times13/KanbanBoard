# 🗂️ KanbanBoard

Tableau Kanban de collaboration d'équipe — Projet **PDR 7, MBDS 2025-2026**

---

## 🚀 Stack technique

* **Backend** : ASP.NET Core 9.0 MVC + SignalR
* **ORM** : Entity Framework Core 9.0 (DataBase First / Scaffolding)
* **Base de données** : SQL Server (LocalDB en développement)
* **Authentification** : Cookie Authentication (custom) + BCrypt
* **Frontend** : Razor + Bootstrap + Sortable.js

---

## 🏗️ Architecture (3-tier)

```bash
KanbanBoard.LibrairieMetier   → Entités + interfaces IxxxDA
KanbanBoard.AccesDonnee       → EF Core, AppDbContext, implémentations DA
KanbanBoard.Web               → Controllers, Views, SignalR Hubs
KanbanBoard.ConsoleApp        → Tests rapides du Métier sans le Web
```

---

## 🗺️ Diagramme de la base de données

### 🔗 Version interactive

👉 https://dbdiagram.io/d/69f0e0ceddb9320fdc7caf4c

---

## ⚙️ Installation & exécution

### 1. Cloner le projet

```bash
git clone git@github.com:times13/KanbanBoard.git
cd KanbanBoard
```

### 2. Restaurer les dépendances

```bash
dotnet restore
```

### 3. Build

```bash
dotnet build
```

### 4. Lancer le projet Web

```bash
dotnet run --project KanbanBoard.Web
```

---

## 🧪 Lancer la Console (tests métier)

```bash
dotnet run --project KanbanBoard.ConsoleApp
```

---

## 🧱 Base de données (EF Core)

```bash
cd KanbanBoard.AccesDonnee

dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## 🔐 Configuration

Les fichiers sensibles ne sont pas versionnés :

* `appsettings.Development.json` ❌ (non inclus dans Git)

Exemple :

```json
{
  "ConnectionStrings": {
    "Default": "votre_chaine_de_connexion"
  }
}
```

---

## 👥 Travail en équipe (Git)

### 🔹 Créer une branche

```bash
git checkout -b feature/nom-feature
```

### 🔹 Push

```bash
git push origin feature/nom-feature
```

### 🔹 Pull avant de travailler

```bash
git pull origin main
```

---

## 📌 Bonnes pratiques

* ❌ Ne jamais travailler directement sur `main`
* ✔ Utiliser des branches `feature/*`
* ✔ Faire des commits clairs
* ✔ Toujours pull avant push

---

## 📄 Licence

Projet académique — MBDS 2025-2026
