
README.md 

# 🎬 SuiviSeriesTV

Application web ASP.NET Core MVC pour suivre des séries, films et anime, avec gestion utilisateur, base de données SQLite, dashboard et fonctionnalités avancées.

---

## 📌 Objectif

Ce projet permet de :

* gérer une bibliothèque de séries, films et anime
* suivre la progression de visionnage
* afficher des statistiques et un dashboard
* utiliser une base de données SQLite avec Entity Framework Core
* exécuter l'application localement sur un autre PC

---

## 🛠️ Technologies utilisées

* ASP.NET Core MVC
* .NET 8
* Entity Framework Core
* SQLite
* HTML / CSS / JavaScript
* Chart.js

---

## ✅ Prérequis

Avant de lancer le projet, il faut installer :

* **Git** → pour télécharger le projet depuis GitHub
* **.NET 8 SDK** → pour compiler et exécuter l’application
* **Visual Studio 2022** ou **Visual Studio Code** → pour ouvrir et modifier le projet
* **dotnet-ef** → pour gérer les migrations de la base de données

---

## 🔎 Vérifier que .NET est bien installé

Dans le terminal :

```bash
dotnet --version
```

Pourquoi ?

Cette commande permet de vérifier que le SDK .NET est bien installé sur la machine.
Si aucune version ne s’affiche, il faut d’abord installer .NET 8 SDK.

---

## 📥 Étape 1 — Récupérer le projet

```bash
git clone https://github.com/moctargoumarattama/SuiviSeriesTV.git
cd SuiviSeriesTV/SuiviSeriesTV
```

Pourquoi ?

* `git clone` télécharge le projet depuis GitHub
* `cd SuiviSeriesTV/SuiviSeriesTV` permet d’entrer dans le dossier contenant réellement l’application ASP.NET Core

---

## 📦 Étape 2 — Restaurer les dépendances NuGet

```bash
dotnet restore
```

Pourquoi ?

Cette commande télécharge et installe automatiquement tous les packages définis dans le fichier `.csproj`.

Par exemple, ce projet utilise notamment :

* `Microsoft.EntityFrameworkCore`
* `Microsoft.EntityFrameworkCore.Sqlite`
* `Microsoft.EntityFrameworkCore.Design`
* `Microsoft.EntityFrameworkCore.Tools`

Tu n’as normalement pas besoin de les réinstaller un par un si le projet est déjà bien versionné, car `dotnet restore` s’en charge automatiquement.

---

## 🧱 Étape 3 — Installer l’outil Entity Framework si nécessaire

```bash
dotnet tool install --global dotnet-ef
```

Pourquoi ?

Cette commande installe l’outil `dotnet-ef`, qui permet de :

* créer des migrations
* appliquer les migrations
* mettre à jour la base de données

Tu n’as besoin de le faire qu’une seule fois par machine.

Pour vérifier s’il est déjà installé :

```bash
dotnet ef
```

---

## 🗄️ Étape 4 — Créer ou mettre à jour la base de données

```bash
dotnet ef database update
```

Pourquoi ?

Cette commande applique les migrations déjà présentes dans le projet et crée automatiquement la base SQLite si elle n’existe pas encore.

Autrement dit :

* elle prépare la base de données
* elle crée les tables nécessaires
* elle permet au projet de fonctionner correctement sur une autre machine

---

## 🧪 Étape 5 — Lancer l’application

```bash
dotnet run
```

Pourquoi ?

Cette commande compile puis démarre l’application localement.

Ensuite, il faut ouvrir le navigateur à l’adresse affichée dans le terminal, par exemple :

```text
https://localhost:5001
ou
http://localhost:5000
```

L’URL exacte peut varier selon la configuration locale.

---

## 🧭 Résumé rapide des commandes à exécuter

```bash
git clone https://github.com/moctargoumarattama/SuiviSeriesTV.git
cd SuiviSeriesTV/SuiviSeriesTV
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef database update
dotnet run
```

---

## 🏗️ Comment le projet a été créé à l’origine

Voici les commandes utilisées au moment de la création initiale du projet :

```bash
# 1) Créer le projet
mkdir SuiviSeriesTV
cd SuiviSeriesTV
dotnet new mvc

# 2) Installer les packages EF Core + SQLite
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.11

# 3) Installer l'outil EF si besoin
dotnet tool install --global dotnet-ef

# 4) Créer la migration initiale (si tu ne prends pas celle déjà fournie)
dotnet ef migrations add InitialCreate

# 5) Appliquer la migration vers SQLite
dotnet ef database update

# 6) Lancer l'application
dotnet run
```

### Important

Ces commandes servent surtout à **construire le projet depuis zéro**.

Pour une personne qui télécharge simplement le projet depuis GitHub, les étapes les plus importantes sont généralement :

```bash
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef database update
dotnet run
```

---

## 📂 Dépendances principales utilisées

Ce projet utilise notamment les packages suivants :

```text
Microsoft.EntityFrameworkCore
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Design
Microsoft.EntityFrameworkCore.Tools
```

### À quoi servent-ils ?

* `Microsoft.EntityFrameworkCore`
  Sert à manipuler la base de données avec Entity Framework Core.

* `Microsoft.EntityFrameworkCore.Sqlite`
  Sert à connecter l’application à SQLite.

* `Microsoft.EntityFrameworkCore.Design`
  Sert aux outils de design et aux migrations.

* `Microsoft.EntityFrameworkCore.Tools`
  Sert aux commandes liées à Entity Framework dans le terminal.

---

## ⚠️ Remarques importantes

* Les dossiers `bin/` et `obj/` ne doivent pas être envoyés sur GitHub.
* Les fichiers `.db` ne doivent pas être versionnés.
* Si une fonctionnalité dépend d’une clé API ou d’un paramètre local, il faut les configurer dans `appsettings.json`.
* Si `dotnet ef database update` échoue, vérifie que `dotnet-ef` est bien installé.

---

## 👨‍💻 Auteur

**Moctar Goumar Attama**

---

## 🚀 Utilisation

Une fois l’application lancée, tu peux :

* ouvrir l’interface web localement
* ajouter et gérer des contenus
* travailler sur le projet avec Visual Studio ou VS Code
* continuer le développement normalement sur une autre machine

Tu peux mettre ce contenu dans `README.md` sur GitHub directement.

Le seul truc que je te conseille encore d’ajouter après, c’est une section **captures d’écran** pour que le repo fasse plus professionnel.
