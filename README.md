# Friendout

> Une app pour organiser des activités avec tes amis — sans prise de tête.

> ⚠️ **Projet en cours de développement (WIP)** — des fonctionnalités peuvent être incomplètes ou changer.

---

*[Read in English](README.en.md)*

---

## 📸 Aperçu

![Page des activités](friendout-frontend/docs/screenshots/activities.png)

---

## 🙋 À propos de ce projet

Je suis **développeur junior** et Friendout est mon premier vrai projet fullstack de A à Z.

L'idée est née d'un besoin très concret : avec mon groupe d'amis sur Discord, on voulait organiser des activités ensemble — jeux, sorties, soirées — et ça finissait toujours dans un chaos de messages. Qui est dispo ? Qui veut participer ? Quel jour ? Friendout est ma réponse à ce problème.

Je partage ce code en open source parce que j'ai moi-même appris énormément en lisant du code d'autres développeurs. J'espère que ce projet pourra aider quelqu'un d'autre dans sa progression. Les retours et les critiques constructives sont les bienvenus — c'est comme ça qu'on apprend.

---

## 🎯 C'est quoi Friendout ?

**Friendout** est une application web pour **créer et rejoindre des activités entre amis**. L'authentification se fait uniquement via **Discord OAuth** — pas de mot de passe, pas de formulaire d'inscription.

Concrètement, tu peux :
- Créer une activité (jeu, sortie, événement...) avec une date et une description
- Laisser les membres d'un serveur Discord la rejoindre
- Gérer tes participations depuis une interface simple et rapide

---

## 🛠️ Stack technique

| Couche | Technologie |
|---|---|
| Frontend | React 19, TypeScript, Vite 7, Tailwind CSS 4, shadcn/ui |
| Backend | ASP.NET Core 9 (C#), Entity Framework Core 9 |
| Base de données | MySQL 8.4 |
| Authentification | Discord OAuth 2.0 + JWT |
| Infrastructure | Docker + Docker Compose, Nginx |

---

## 🚀 Démarrer le projet

### Prérequis

- [Docker](https://www.docker.com/) & Docker Compose
- Une [application Discord](https://discord.com/developers/applications) (Client ID + Secret)

### 1. Cloner le dépôt

```bash
git clone https://github.com/cobra1407/friendout.git
cd friendout
```

### 2. Configurer les variables d'environnement

```bash
cp .env.docker.example .env.docker
```

Édite `.env.docker` et renseigne toutes les valeurs requises (credentials Discord, clé JWT, mots de passe DB).

### 3. Lancer avec Docker

```bash
docker compose --env-file .env.docker up --build
```

L'application sera disponible sur `http://localhost`.

---

## 💻 Développement local (sans Docker)

### Backend

```bash
cd friendout-backend/Friendout.API
cp .env.example .env
# Renseigne les valeurs dans .env
dotnet run
```

API disponible sur `http://localhost:5122`. Swagger UI sur `http://localhost:5122/swagger`.

### Frontend

```bash
cd friendout-frontend
cp .env.example .env
# Renseigne les valeurs dans .env
pnpm install
pnpm dev
```

Application disponible sur `http://localhost:5173`.

---

## 📁 Structure du projet

```
friendout/
├── docker-compose.yml
├── .env.docker.example          # Template variables Docker
├── friendout-backend/           # API ASP.NET Core 9
│   ├── Friendout.API/           # Controllers, config, point d'entrée
│   ├── Friendout.Domain/        # Entités, DbContext, seeds
│   ├── Friendout.Infrastructure/# Services, repositories
│   └── Friendout.Tests/         # Tests unitaires & intégration
└── friendout-frontend/          # SPA React + Vite
    └── src/
        ├── components/          # Composants UI partagés
        ├── features/            # Modules par domaine métier
        ├── contexts/            # Contextes React
        ├── i18n/                # Traductions FR/EN
        └── lib/                 # Client API, utils, helpers
```

---

## 🔧 Configuration Discord

1. Va sur le [Discord Developer Portal](https://discord.com/developers/applications)
2. Crée une nouvelle application
3. Dans **OAuth2**, ajoute l'URI de redirection : `http://localhost:5122/api/auth/callback/discord`
4. Copie le **Client ID** et le **Client Secret** dans ton `.env` / `.env.docker`

---

## 🔑 Variables d'environnement

### Backend (`friendout-backend/Friendout.API/.env`)

| Variable | Description |
|---|---|
| `ConnectionStrings__FriendoutDatabase` | Chaîne de connexion MySQL |
| `Authentication__Discord__ClientId` | Client ID Discord OAuth |
| `Authentication__Discord__ClientSecret` | Client Secret Discord OAuth |
| `Jwt__Key` | Clé de signature JWT (min. 32 caractères) |
| `Jwt__Issuer` | URL de l'émetteur du token |
| `Jwt__Audience` | URL de l'audience du token |
| `Discord__AllowedGuildIds` | IDs de serveurs Discord autorisés, séparés par des virgules (optionnel — vide = tout le monde) |

### Frontend (`friendout-frontend/.env`)

| Variable | Description |
|---|---|
| `VITE_API_BASE_URL` | URL de base de l'API backend |
| `VITE_DISCORD_AUTH_URL` | Point d'entrée Discord OAuth |
| `VITE_ENV` | Environnement (`development` / `production`) |

---

## 🤝 Contribuer

Les retours, issues et pull requests sont les bienvenus ! Ce projet est avant tout un projet d'apprentissage, donc n'hésite pas à signaler des erreurs ou à suggérer des améliorations.

1. Fork le dépôt
2. Crée une branche : `git checkout -b feature/ma-fonctionnalite`
3. Commit tes changements : `git commit -m "feat: ajouter ma fonctionnalité"`
4. Push et ouvre une Pull Request

---

## 📄 Licence

Ce projet est sous licence [MIT](LICENSE).
