# Friendout Roadmap

This document outlines the direction of Friendout: what's already built, and what's planned next.
It's meant to give visitors and potential contributors a clear picture of where the project is going — and where help is welcome.

If you'd like to contribute, see [CONTRIBUTING.md](./CONTRIBUTING.md).

---

## ✅ Current State

Friendout already supports:

- Activity creation, with public sharing via unguessable tokens
- Sub-activities management
- Virtual locations management
- Real-time updates via WebSockets
- Access control & access requests (join requests for activities)
- User preferences management
- Admin dashboard
- Email sending (notifications)
- Calendar export
- Activity search & filtering
- Equipment management for activities (what's needed for an activity)
- User-created equipment list templates (reusable across activities, so you don't re-enter the same gear list every time you create an activity)
- Rate limiting
- Comments on activities
- In-app notifications (with per-user notification settings, in addition to email)
- OAuth login (Google/other providers)
- Discord-guild-based access control (restrict signup/access to members of allowed Discord servers)
- Automatic activity reminders (scheduled job)
- File & image uploads
- Localization (i18n)
- Health checks
- Self-hosted deployment via Docker Compose (Clean Architecture, .NET 9 + React/TypeScript)

---

## 🧭 Planned Features

Listed in suggested priority order (based on dependencies between them, not fixed in stone). Each one will get its own GitHub Issue — check the [Issues tab](../../issues) to see current status, or to pick one up.

### 👥 Maximum Number of Participants
- Organizer can cap the number of spots for an activity
- Needs: waitlist behavior when full? Auto-close registration at the limit?
- Small, self-contained — good starting point

### 💰 Approximate Price / Budget
- Let organizers set a price range for an activity (e.g. "5–15 €")
- Displayed on the activity card so participants know the expected cost before joining
- Small, self-contained — good starting point

### 🤝 Friends / Contacts
- A friends list, with friend requests (send/accept/decline)
- Cross-cutting feature that other planned features build on:
  - **Invitations** (below): pick people from your friends list, in addition to inviting by email
  - **Messaging**: message a friend directly, not just within an activity thread
- Needs: one-way follow vs. mutual friend request? Friend-only visibility for profile/activity history?

### 🔒 Activity Visibility States (Draft / Private / Public)
- Introduce a visibility status per activity, with three levels:
  - **Draft** — visible only to the creator, for preparing an activity before deciding to go ahead with it
  - **Private** — visible to the creator + explicitly invited people
  - **Public** — visible to everyone, using the existing public-link + access-request flow
- Builds on the existing access control / access request system rather than starting from scratch
- **Invitations**: by email, or by picking from the Friends list above once it exists
- **Accept/decline**: reuse the existing `ParticipationStatus` mechanism rather than a new one
- Still open: can status move backward (e.g. Public → Private)? What happens to existing participants if visibility is downgraded?

### 🔑 Roles & Permissions
- Define who can edit an activity, manage participants, or moderate messages
- Likely roles: organizer, co-organizer, participant
- Extends the existing access control system with more granular roles
- Natural follow-up once visibility states above are in place

### 🤖 MCP Server (AI Integration)
- Expose Friendout through a [Model Context Protocol](https://modelcontextprotocol.io/) server, so users can connect their own AI assistant (Claude, etc.) and create/manage activities via natural language instead of the web UI
- Requires a personal access token system (user-generated tokens, separate from the OAuth login flow used for the web app)
- Builds on the Roles & Permissions system above: a token should be scopable (e.g. read-only, create-activity-only, full access) rather than granting full account access by default
- Needs: which actions are exposed first (create activity, search, join an activity)? Token expiry/revocation? Rate limiting reuse from the existing system?

### 📊 Proposals / Interest Polls
- Let a user float an idea to a group without committing to a full activity yet
- People react/vote to signal interest (e.g. "I'm in", "maybe", "not for me")
- If interest is high enough, the proposal converts into a real activity, carrying over the interested people as initial participants
- Complements the Draft status above: Draft = testing an idea alone, Proposal = testing an idea with the group
- Needs: minimum interest threshold to suggest conversion (manual vs automatic)? Expiry date for a proposal? Can a proposal have a rough date range instead of a fixed date?

### 🚗 Transportation Coordination
- Let participants indicate how they're getting there: car (as driver, with seats available), train, other
- Goal: help the group organize carpooling without a separate spreadsheet or chat
- Benefits from having a stable participant list, so best after the features above

### 💬 In-app Messaging
- Full messaging system between activity participants
- Likely scope: per-activity chat thread, notifications on new messages
- Can build on the existing WebSocket infrastructure and the Friends list above — biggest scope, best tackled once the rest is stable
- Open questions: message history retention? Read receipts?

### 🏆 Achievements
- Gamification system rewarding users for participation/activity (data model exists, not yet built out)
- Needs: which milestones trigger achievements? Displayed where in the UI (profile page)?
- Nice-to-have polish, best done once core features are in place

---

## 💡 Have an idea or a question?

Open a [Discussion](../../discussions) or an [Issue](../../issues) — feedback on scope and priorities is welcome, this order isn't set in stone.
