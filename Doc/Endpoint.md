# API Documentation

## 1. Activités

### GET /api/activities
- Fichier : `app/api/activities/route.ts`
- Description : retourne la liste de toutes les activités (`getAllActivities`).

### POST /api/activities
- Fichier : `app/api/activities/route.ts`
- Description : crée une activité.
  - Accepte multipart/form-data (avec image) ou JSON.
  - Nécessite une session NextAuth (`session.user.id`).

### GET /api/activities/[activityId]
- Fichier : `app/api/activities/[activityId]/route.ts`
- Description : retourne les détails d’une activité (`getActivityDetails`).

### PUT /api/activities/[activityId]
- Fichier : `app/api/activities/[activityId]/route.ts`
- Description : met à jour une activité (multipart ou JSON).

### DELETE /api/activities/[activityId]
- Fichier : `app/api/activities/[activityId]/route.ts`
- Description : supprime une activité (`deleteActivity`).

---

## 1.1. Commentaires d’activité

### GET /api/activities/[activityId]/comments
- Fichier : `app/api/activities/[activityId]/comments/route.ts`
- Description : liste les commentaires (`getActivityComments`).

### POST /api/activities/[activityId]/comments
- Fichier : `app/api/activities/[activityId]/comments/route.ts`
- Description : ajoute un commentaire (`addComment`).
- Nécessite une session (`session.user`).

### PUT /api/activities/[activityId]/comments/[commentId]
- Fichier : `app/api/activities/[activityId]/comments/[commentId]/route.ts`
- Description : met à jour un commentaire (`updateComment`).
- Nécessite une session (`session.user`).

### DELETE /api/activities/[activityId]/comments/[commentId]
- Fichier : `app/api/activities/[activityId]/comments/[commentId]/route.ts`
- Description : supprime un commentaire (`deleteComment`).
- Nécessite une session (`session.user`).

---

## 1.2. RSVPs d’activité

### GET /api/activities/[activityId]/rsvps
- Fichier : `app/api/activities/[activityId]/rsvps/route.ts`
- Description : liste les RSVP (`getRsvpsByActivityId`).

### POST /api/activities/[activityId]/rsvps
- Fichier : `app/api/activities/[activityId]/rsvps/route.ts`
- Description : crée ou met à jour un RSVP (`upsertRsvp`).
- Nécessite une session (`session.user.id`).
- Le champ `status` doit être une valeur valide de `RsvpStatus` (Prisma).

---

## 2. Utilisateurs

### GET /api/users
- Fichier : `app/api/users/route.ts`
- Description : retourne tous les utilisateurs (`getAllUsers`).

### GET /api/users/[id]
- Fichier : `app/api/users/[id]/route.ts`
- Description : retourne un utilisateur par ID (`getUserById`).
- Retourne 404 si non trouvé.

### GET /api/users/name/[name]
- Fichier : `app/api/users/name/[name]/route.ts`
- Description : retourne un utilisateur par nom (`getUserByName`).
- Retourne 404 si non trouvé.

---

## 3. Achievements

### GET /api/achievements
- Fichier : `app/api/achievements/route.ts`
- Description : retourne les achievements de l’utilisateur (`getUserAchievements`).
- Nécessite `session.user.id`.

### POST /api/achievements
- Fichier : `app/api/achievements/route.ts`
- Description :
  - Si `achievementCode` est fourni : débloque un achievement (`unlockAchievement`).
  - Sinon si `action` est fournie : vérifie/débloque les achievements liés à l’action (`checkAndUnlockAchievements`).
- Nécessite `session.user.id`.

---

## 4. Équipements utilisateur

### GET /api/user-equipments
- Fichier : `app/api/user-equipments/route.ts`
- Query : `activityId` (obligatoire)
- Description : retourne les équipements de l’utilisateur pour une activité.

### POST /api/user-equipments
- Fichier : `app/api/user-equipments/route.ts`
- Body : `equipmentId`, `activityId`, `hasEquipment`
- Description :
  - Si `hasEquipment === true` : crée l’entrée si elle n’existe pas.
  - Sinon : supprime l’entrée.

---

## 5. Authentification (NextAuth)

### GET /api/auth/[...nextauth]
### POST /api/auth/[...nextauth]
- Fichier : `app/api/auth/[...nextauth]/route.ts`
- Description : handler NextAuth (OAuth, callbacks, etc.).

---

## 6. Fichiers uploadés

### GET /api/uploads/[...path]
- Fichier : `app/api/uploads/[...path]/route.ts`
- Description :
  - Sert un fichier depuis `public/uploads/**`.
  - Détermine le Content-Type selon l’extension.
  - Retourne 404 si non trouvé.
  - Ajoute des headers de cache.


Resource architecture react :
https://sandroroth.com/blog/project-structure/

## exemple : 
```
src/
├── assets/                  # Images, icônes, logos, etc.
├── components/              # Composants UI globaux (Button, Modal, Spinner...)
├── config/                  # Configs globales (API base URL, themes)
├── features/
│   ├── auth/
│   │   ├── api/             # login, register
│   │   ├── components/      # LoginForm, RegisterForm
│   │   ├── hooks/           # useLogin, useRegister
│   │   ├── routes/          # LoginPage, RegisterPage
│   │   ├── types/
│   │   └── index.ts
│   │
│   ├── friends/
│   │   ├── api/             # fetchFriends, addFriend, removeFriend
│   │   ├── components/      # FriendCard, FriendsList
│   │   ├── hooks/           # useFriends, useAddFriend
│   │   ├── routes/          # FriendsPage
│   │   ├── types/           # Friend interface
│   │   └── index.ts
│   │
│   ├── events/
│   │   ├── api/             # fetchEvents, createEvent
│   │   ├── components/      # EventCard, EventsList
│   │   ├── hooks/           # useEvents, useCreateEvent
│   │   ├── routes/          # EventsPage
│   │   ├── types/           # Event interface
│   │   └── index.ts
│   │
│   ├── messages/
│   │   ├── api/             # fetchMessages, sendMessage
│   │   ├── components/      # ChatWindow, MessageItem
│   │   ├── hooks/           # useMessages, useSendMessage
│   │   ├── routes/          # ChatPage
│   │   ├── types/           # Message interface
│   │   └── index.ts
│   │
│   └── notifications/
│       ├── api/             # fetchNotifications, markAsRead
│       ├── components/      # NotificationItem, NotificationsList
│       ├── hooks/           # useNotifications
│       ├── routes/          # NotificationsPage
│       ├── types/           # Notification interface
│       └── index.ts
│
├── hooks/                   # Hooks partagés (useDebounce, useModal)
├── lib/                     # Librairies / fonctions génériques (ex: fetch wrapper)
├── providers/               # Contexts globaux (AuthProvider, QueryProvider)
└── routes/                  # Router principal
```
