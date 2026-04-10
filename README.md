

---

# 📄 👉 Ton README prêt à utiliser

# 🎬 SuiviSeriesTV

Application web ASP.NET Core pour gérer et suivre séries, films et anime avec un dashboard intelligent.

---

## 🚀 Fonctionnalités

* 📺 Gestion des séries, films et anime
* 📊 Dashboard avec statistiques (Chart.js)
* 🔄 Suivi de progression (épisodes / saisons)
* ⭐ Favoris et commentaires
* 🔍 Recherche via API TMDb
* 👤 Authentification utilisateur
* 🛠️ Interface admin

---

## 🛠️ Technologies utilisées

* ASP.NET Core (.NET 8)
* Entity Framework Core
* SQLite
* Chart.js
* HTML / CSS / JavaScript

---

## ⚙️ Prérequis

Avant de commencer, assure-toi d’avoir installé :

* .NET 8 SDK
* Git
* Visual Studio 2022 ou VS Code

Vérifie avec :

```bash
dotnet --version
```

---

## 📥 Installation

Clone le projet :

```bash
git clone https://github.com/moctargoumarattama/SuiviSeriesTV.git
cd SuiviSeriesTV/SuiviSeriesTV
```

---

## ⚙️ Configuration

1. Crée un fichier `appsettings.json` à partir d’un modèle :

2. Exemple de configuration :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=suiviseriestv.db"
  },
  "TmdbSettings": {
    "ApiKey": "VOTRE_CLE_TMDB"
  },
  "EmailSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "email@example.com",
    "Password": "mot_de_passe"
  }
}
```

⚠️ Important :

* Sans clé TMDb → la recherche ne fonctionnera pas
* Sans SMTP → les emails ne fonctionneront pas

---

## 🗄️ Base de données

Applique les migrations :

```bash
dotnet restore
dotnet ef database update
```

Si la commande échoue :

```bash
dotnet tool install --global dotnet-ef
```

---

## ▶️ Lancer l'application

```bash
dotnet run
```

Puis ouvre ton navigateur :

```
https://localhost:5001
ou
http://localhost:5000
```

---

## 📸 Aperçu

👉 (Ajoute ici des captures d’écran de ton application)

---

## 📁 Structure du projet

```
/Controllers
/Models
/Views
/Services
/Data
/wwwroot
```

---

## 👨‍💻 Auteur

**Moctar Goumar Attama**

---

## 📌 Remarques

* Les dossiers `bin/` et `obj/` ne sont pas inclus (normal)
* Les fichiers `.db` ne sont pas versionnés
* Le projet est prêt à être cloné et utilisé

---

## 🚀 Objectif du projet

Créer une application moderne de suivi de contenus avec une expérience utilisateur fluide et des fonctionnalités avancées.

---

---

# 🚀 Ce que tu dois faire maintenant

1. Va sur ton repo GitHub
2. Clique sur **Add file → Create new file**
3. Nom :

```
README.md
```

4. Colle le contenu
5. Clique sur **Commit changes**

---

# 🔥 Impact direct

👉 Avant :

* repo technique
* difficile à comprendre

👉 Après :

* projet clair
* installable
* présentable à un recruteur 💼

---


