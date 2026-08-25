# Contributing to Friendout

Thanks for your interest in Friendout! This is a self-hosted group activity planning app, currently maintained as a solo/portfolio project — but contributions, feedback, and ideas are genuinely welcome.

See [ROADMAP.md](./ROADMAP.md) for the current direction and planned features.

## Ways to Help

You don't need to write code to contribute:

- **Try it out** — deploy it, use it with friends, and report what's confusing or broken
- **Report bugs** — open an [Issue](../../issues) with steps to reproduce
- **Suggest features / discuss scope** — open a [Discussion](../../discussions), especially useful for the bigger items on the roadmap (e.g. messaging, permissions)
- **Design feedback** — UI/UX suggestions are welcome, especially around the dark "watchtower" theme
- **Code** — see below

## Picking Up an Issue

- Issues labeled `good first issue` are a good entry point if you're new to the codebase
- Issues labeled `help wanted` are open for anyone
- Comment on an issue before starting significant work, to avoid duplicate effort

## Development Setup

The stack:
- Backend: .NET 9, Clean Architecture, Entity Framework Core
- Frontend: React + TypeScript, Tailwind CSS v4
- Deployment: Docker Compose

See the main [README](./README.md) for setup instructions.

## Code Conventions

- **Commit messages**: [Conventional Commits](https://www.conventionalcommits.org/), with a descriptive body explaining the *why*, not just the *what*
- **Branching**: one branch per feature/fix (e.g. `feat/private-activities`, `fix/css-grid-overflow`)
- **Code comments**: always in English, including XML doc comments on the backend
- **Pull requests**: keep them focused — one feature or fix per PR when possible; link the related issue

## Reporting Security Issues

Please don't open a public issue for security vulnerabilities. Instead, use GitHub's [private vulnerability reporting](../../security/advisories/new) — it opens a private discussion with the maintainer, without exposing the issue publicly until it's resolved.

## Questions?

Open a [Discussion](../../discussions) — happy to clarify scope, architecture decisions, or anything else before you dive in.
